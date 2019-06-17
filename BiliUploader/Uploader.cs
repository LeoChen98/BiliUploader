using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BiliUploader
{
    internal class Uploader
    {
        #region Private Fields

        /// <summary>
        /// 上传文件信息
        /// </summary>
        private FileInfo fi;

        /// <summary>
        /// 不带后缀名的远程文件名
        /// </summary>
        private string fns;

        /// <summary>
        /// 指示PreUpload是否成功
        /// </summary>
        private bool IsPreloadSucceed = false;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// 授权字符串
        /// </summary>
        public string Auth { get; private set; }

        /// <summary>
        /// biz_id
        /// </summary>
        public string Biz_id { get; private set; }

        /// <summary>
        /// 分片数
        /// </summary>
        public int ChunkCount { get; private set; }

        /// <summary>
        /// 分片大小
        /// </summary>
        public int ChunkSize { get; private set; }

        /// <summary>
        /// 上传主机
        /// </summary>
        public string EndPoint { get; private set; }

        /// <summary>
        /// 上传路径
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadCount { get; private set; }

        /// <summary>
        /// UploadId
        /// </summary>
        public string UploadId { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 发布稿件
        /// </summary>
        public static void Publish()
        {
            //检查是否被ban
            string str = http.GetBody("https://member.bilibili.com/x/geetest/pre/add", variables.Cookies);
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0 && (bool)obj["data"]["limit"]["add"] == false)
                {
                    //开始投稿
                    PublishInfo info = new PublishInfo()
                    {
                        copyright = variables.copyright,
                        videos = new List<VideoInfo>(),
                        mission_id = variables.mission_id,
                        tid = variables.type,
                        cover = variables.CoverFile,
                        title = variables.Title,
                        tag = variables.tags,
                        desc_format_id = GetFormatId(),
                        desc = variables.desc,
                        dynamic = variables.dynamic,
                        dtime = variables.dt,
                        subtitle = new SubtitleInfo()
                    };
                    //添加分P列表
                    for (int i = 0; i < variables.FileList.Count; i++)
                    {
                        info.videos.Add(new VideoInfo()
                        {
                            filename = variables.FileList[i],
                            title = "P" + (i + 1)
                        });
                    }
                    //设置字幕
                    if (!string.IsNullOrEmpty(variables.Subtitle))
                    {
                        info.subtitle.open = 1;
                        info.subtitle.lan = variables.Subtitle;
                    }

                    string json = JsonConvert.SerializeObject(info);
                    str = http.PostBody("https://member.bilibili.com/x/vu/web/add?csrf=" + variables.csrf, json, variables.Cookies, "application/json;charset=utf-8");
                    if (!string.IsNullOrEmpty(str))
                    {
                        obj = JObject.Parse(str);
                        if ((int)obj["code"] == 0)
                        {
                            Console.WriteLine("投稿成功！av号：av" + obj["data"]["aid"].ToString());
                        }
                        else
                        {
                            Console.Error.WriteLine("投稿失败。");
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("投稿失败。");
                    }
                }
                else
                {
                    Console.Error.WriteLine("投稿失败，账号被ban。");
                }
            }
            else
            {
                Console.Error.WriteLine("投稿失败。");
            }
        }

        /// <summary>
        /// 上传封面
        /// </summary>
        public static void SetCover()
        {
            Console.WriteLine("开始上传封面...");
            string str = http.PostBody("https://member.bilibili.com/x/vu/web/cover/up", "cover=" + File2Base64Helper.ImageToBase64(variables.CoverFile) + "&csrf=" + variables.csrf, variables.Cookies);
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0)
                {
                    variables.CoverFile = obj["data"]["url"].ToString().Remove(0, 5);
                    Console.WriteLine("封面上传完成。");
                    return;
                }
            }
            variables.IsHasError = true;
            variables.CoverFile = string.Empty;
            Console.Error.WriteLine("上传封面错误！");
        }

        /// <summary>
        /// 执行上传
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="pid">分Pid</param>
        public async Task DoUpload(string filename, int pid = 1)
        {
            Console.WriteLine("正在准备上传：P" + pid + "。");
            await Task.Run(() =>
            {
                PreUpload(filename);
                if (IsPreloadSucceed)
                {
                    Console.WriteLine("开始上传：P" + pid + "。");
                    //创建分片队列
                    List<Task<bool>> UploadQueue = new List<Task<bool>>();
                    int taskwaiting = ChunkCount;
                    while (taskwaiting > 0)
                    {
                        for (int i = 0; i < UploadQueue.Count; i++)
                        {
                            if (UploadQueue[i].IsCompleted)
                            {
                                if (UploadQueue[i].Result == false)
                                {
                                    Console.Error.WriteLine("上传错误：P" + pid + "分片上传错误。");
                                    variables.IsHasError = true;
                                    return;
                                }
                                else
                                {
                                    UploadQueue.Remove(UploadQueue[i]);
                                }
                            }
                        }

                        while (UploadQueue.Count < ThreadCount && taskwaiting > 0)
                        {
                            UploadQueue.Add(ChunkUpload(filename, pid, ChunkCount - taskwaiting));
                            taskwaiting--;
                        }
                    }

                    //等待所有分片上传
                    foreach (Task<bool> task in UploadQueue)
                    {
                        task.Wait();
                        if (task.Result == false)
                        {
                            Console.Error.WriteLine("上传错误：P" + pid + "分片上传错误。");
                            variables.IsHasError = true;
                            return;
                        }
                    }

                    //完成分片上传
                    if (variables.IsIgnoreError || !variables.IsHasError)
                    {
                        if (http.Options("https:" + EndPoint + Key + "?output=json&name=" + fi.Name + "&profile=ugcupos%2Fbup&uploadId=" + UploadId + "&biz_id=" + Biz_id, variables.Cookies))
                        {
                            PartInfo pi = new PartInfo()
                            {
                                parts = new List<ChunkInfo>()
                            };

                            for (int i = 1; i <= ChunkCount; i++)
                            {
                                pi.parts.Add(new ChunkInfo() { partNumber = i });
                            }

                            WebHeaderCollection headers = new WebHeaderCollection();
                            headers.Add("X-Upos-Auth", Auth);

                            string str = http.PostBody("https:" + EndPoint + Key + "?output=json&name=" + fi.Name + "&profile=ugcupos%2Fbup&uploadId=" + UploadId + "&biz_id=" + Biz_id, JsonConvert.SerializeObject(pi), variables.Cookies, "application/json; charset=utf-8", "", "", headers);
                            if (!string.IsNullOrEmpty(str))
                            {
                                JObject obj = JObject.Parse(str);
                                if ((int)obj["OK"] == 1)
                                {
                                    fns = Key.Remove(0, 5);
                                    fns = fns.Remove(fns.LastIndexOf('.'));

                                    variables.FileList[pid - 1] = fns;
                                    Console.WriteLine("上传完成：P" + pid + "。");
                                }
                                else
                                {
                                    Console.Error.WriteLine("上传错误：P" + pid + "上传错误。无法完成上传。");
                                    variables.IsHasError = true;
                                    return;
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("上传错误：P" + pid + "上传错误。无法完成上传。");
                                variables.IsHasError = true;
                                return;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("上传错误：P" + pid + "上传错误。无法完成上传。");
                            variables.IsHasError = true;
                            return;
                        }
                    }

                    //默认封面
                    if (pid == 1 && variables.CoverFile == "")
                    {
                        Console.WriteLine("未设置稿件封面，将使用P1的第一张系统截图作为封面。");
                        SetDefaultCover();
                    }
                }
                else
                {
                    Console.Error.WriteLine("上传错误：P" + pid + "上传预配置接口错误。");
                    variables.IsHasError = true;
                }
            });
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 获取formatid
        /// </summary>
        /// <returns>formatid</returns>
        private static int GetFormatId()
        {
            string str = http.GetBody("https://member.bilibili.com/x/web/archive/desc/format?typeid=" + variables.type + "&copyright=1", variables.Cookies);
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0)
                {
                    return obj["data"].HasValues ? (int)obj["data"]["id"] : -1;
                }
            }
            return 0;
        }

        /// <summary>
        /// 分片上传
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="chunkid">分片id</param>
        /// <returns>分片上传结果</returns>
        private async Task<bool> ChunkUpload(string filename, int pid, int chunkid)
        {
            Console.WriteLine("开始上传：P" + pid + "分片" + chunkid + "。");
            int retry = 0;

            int size = ChunkSize * (chunkid + 1) > fi.Length ? (int)(fi.Length - ChunkSize * chunkid) : ChunkSize;
            int start = ChunkSize * chunkid;
            int end = ChunkSize * (chunkid + 1) > fi.Length ? (int)fi.Length : ChunkSize * (chunkid + 1);
            int total = (int)fi.Length;

            string url = "https:" + EndPoint + Key;
            string query = "?partNumber=" + (chunkid + 1) + "&uploadId=" + UploadId + "&chunk=" + chunkid + "&chunks=" + ChunkCount + "&size=" + size + "&start=" + start + "&end=" + end + "&total=" + total;
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("X-Upos-Auth", Auth);
        retry:
            try
            {
                if (http.Options(url + query, variables.Cookies))
                {
                    string str = await http.PutFile(url + query, filename, start, size, variables.Cookies, "", "", headers);
                    if (str.Contains("SUCCESS"))
                    {
                        Console.WriteLine("上传成功：P" + pid + "分片" + chunkid + "。");
                        return true;
                    }
                    else
                    {
                        if (retry < 10)
                        {
                            retry++;
                            Console.Error.WriteLine("上传错误：P" + pid + "分片" + chunkid + "上传错误。原因：" + str + "；重试：" + retry);
                            goto retry;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch
            {
                if (retry < 10)
                {
                    retry++;
                    Console.Error.WriteLine("上传错误：P" + pid + "分片" + chunkid + "上传错误。重试：" + retry);
                    goto retry;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 上传预配置接口
        /// </summary>
        /// <param name="filename">文件路径</param>
        private void PreUpload(string filename)
        {
            try
            {
                fi = new FileInfo(filename);
                string query = "name=" + fi.Name + "&size=" + fi.Length + "&r=upos&profile=ugcupos%2Fbup&ssl=0&version=2.4.14&build=2041400&os=upos&upcdn=ws";
                string str = http.GetBody("https://member.bilibili.com/preupload?" + query, variables.Cookies);
                if (!string.IsNullOrEmpty(str))
                {
                    JObject obj = JObject.Parse(str);
                    if ((int)obj["OK"] == 1)
                    {
                        Key = obj["upos_uri"].ToString().Remove(0, 6);
                        EndPoint = obj["endpoint"].ToString();
                        ThreadCount = (int)obj["threads"];
                        ChunkSize = (int)obj["chunk_size"];
                        ChunkCount = fi.Length % ChunkSize == 0 ? (int)(fi.Length / ChunkSize) : (int)(fi.Length / ChunkSize) + 1;
                        Biz_id = obj["biz_id"].ToString();
                        Auth = obj["auth"].ToString();

                        if (http.Options("https:" + EndPoint + Key + "?uploads&output=json", variables.Cookies))
                        {
                            WebHeaderCollection headers = new WebHeaderCollection();
                            headers.Add("X-Upos-Auth", Auth);
                            str = http.PostBody("https:" + EndPoint + Key + "?uploads&output=json", "", variables.Cookies, "application/x-www-form-urlencoded;charset=utf-8", "", "", headers);
                            if (!string.IsNullOrEmpty(str))
                            {
                                obj = JObject.Parse(str);
                                if ((int)obj["OK"] == 1)
                                {
                                    UploadId = obj["upload_id"].ToString();
                                    IsPreloadSucceed = true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                IsPreloadSucceed = false;
            }
        }

        /// <summary>
        /// 设置默认封面
        /// </summary>
        private void SetDefaultCover()
        {
            string str = http.GetBody("https://member.bilibili.com/x/web/archive/recovers?fns=" + fns, variables.Cookies);
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0)
                {
                    variables.CoverFile = obj["data"][0].ToString();
                }
            }
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// 分片信息模板
        /// </summary>
        private class ChunkInfo
        {
            #region Public Fields

            public string eTag = "etag";
            public int partNumber;

            #endregion Public Fields
        }

        /// <summary>
        /// 分片上传合并模板
        /// </summary>
        private class PartInfo
        {
            #region Public Fields

            public List<ChunkInfo> parts;

            #endregion Public Fields
        }

        /// <summary>
        /// 稿件信息模板
        /// </summary>
        private class PublishInfo
        {
            #region Public Fields

            public int copyright = 1;
            public string cover;
            public string desc;
            public int desc_format_id;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue(-1)]
            public int dtime;

            public string dynamic;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore), DefaultValue(-1)]
            public int mission_id = -1;

            public int no_reprint = 1;
            public SubtitleInfo subtitle;
            public string tag;
            public int tid;
            public string title;
            public List<VideoInfo> videos;

            #endregion Public Fields
        }

        private class SubtitleInfo
        {
            #region Public Fields

            public string lan = "";
            public int open = 0;

            #endregion Public Fields
        }

        /// <summary>
        /// 分p视频信息模板
        /// </summary>
        private class VideoInfo
        {
            #region Public Fields

            public string desc = "";
            public string filename;
            public string title;

            #endregion Public Fields
        }

        #endregion Private Classes
    }
}