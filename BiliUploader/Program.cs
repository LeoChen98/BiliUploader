using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiliUploader
{
    internal class Program
    {
        #region Private Methods

        private static void Main(string[] args)
        {
            Console.WriteLine("BiliUploader @ " + Application.ProductVersion.ToString());
            Console.WriteLine("Copyrights (c) 2019 zhangbudademao.com. All rights reserved.");
            //处理入参
            bool IsList = false;
            for (int args_index = 0; args_index < args.Length; args_index++)
            {
                switch (args[args_index])
                {
                    case "-c"://cookies
                        variables.CookiesString = args[++args_index];
                        break;
                    case "-ls"://list start
                        IsList = true;
                        break;
                    case "-le"://list end
                        IsList = false;
                        break;
                    case "-title":
                        variables.Title = args[++args_index];
                        break;
                    case "-cover"://cover image
                        variables.CoverFile = args[++args_index];
                        break;
                    case "-type"://type id
                        variables.type = int.Parse(args[++args_index]);
                        break;
                    case "-tags"://tags
                        variables.tags = args[++args_index];
                        break;
                    case "-desc"://description
                        variables.desc = args[++args_index];
                        break;
                    case "-dynamic"://dynamic
                        variables.dynamic = args[++args_index];
                        break;
                    case "-dt"://publish time(if smaller than the submit time +4h, dt=st+4h)
                        variables.dt = int.Parse(args[++args_index]);
                        break;
                    case "-copyright":
                        variables.copyright = int.Parse(args[++args_index]);
                        break;
                    case "-mid":
                        variables.mission_id = int.Parse(args[++args_index]);
                        break;
                    case "-subtitle":
                        variables.Subtitle = args[++args_index];
                        break;
                    case "-f":
                        variables.IsIgnoreError = true;
                        break;
                    case "-com":
                        string[] settings = args[++args_index].Split(',');
                        if (File.Exists("ffmpeg.exe"))
                        {
                            if (settings.Length == 4)
                            {
                                variables.v_size = settings[0];
                                variables.v_fps = settings[1];
                                variables.v_rate = settings[2];
                                variables.v_maxrate = settings[3];
                                variables.IsCompress = true;
                            }
                            else
                            {
                                Console.Error.WriteLine("入参格式错误，将不会对视频进行处理。");
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("未找到ffmpeg，将不会对视频进行处理。");
                        }
                        break;
                    case "-d":
                        variables.IsDeleteTmp = true;
                        break;
                    case "-h":
                    case "-?":
                    case "?":
                        Console.WriteLine(Properties.Resources.helpstr);
                        Console.ReadKey();
                        return;
                    default:
                        if (IsList)
                        {
                            variables.FileList.Add(args[args_index]);
                        }
                        break;
                }
            }

            //上传封面
            if (!string.IsNullOrEmpty(variables.CoverFile))
            {
                Uploader.SetCover();
            }

            //分P构造上传任务
            List<Task> UploadQueue = new List<Task>();
            for (int p = 1; p <= variables.FileList.Count; p++)
            {
                UploadQueue.Add(UploadTask(p));
            }

            //等待分P上传任务结束
            foreach (Task task in UploadQueue)
            {
                task.Wait();
            }

            //提交投稿
            if (variables.IsIgnoreError || !variables.IsHasError)
            {
                Console.WriteLine("投稿准备就绪：");
                Console.WriteLine("投稿标题：" + variables.Title);
                Console.WriteLine("分P：" + variables.FileList.Count);
                Console.WriteLine("封面：" + variables.CoverFile);
                Console.WriteLine("分区：" + variables.type);
                Console.WriteLine("参与活动：" + variables.mission_id);
                Console.WriteLine("标签：" + variables.tags);
                Console.WriteLine("简介：" + variables.desc);
                Console.WriteLine("动态：" + variables.dynamic);
                if (variables.dt != -1) Console.WriteLine("投稿时间：" + variables.dt);
                Console.WriteLine("版权：" + variables.copyright);
                Console.WriteLine("字幕语言：" + (variables.Subtitle == "" ? "禁用" : variables.Subtitle));

                Uploader.Publish();
            }

            //删除压制缓存
            if (variables.IsDeleteTmp)
            {
                compressor.DeleteTmp();
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 上传任务
        /// </summary>
        /// <param name="p">分P</param>
        /// <returns></returns>
        private static async Task UploadTask(int p)
        {
            //视频压制
            if (variables.IsCompress)
            {
                variables.FileList[p-1] = await compressor.compress(variables.FileList[p - 1]);
            }
            //上传
            await new Uploader().DoUpload(variables.FileList[p - 1], p);
        }


        #endregion Private Methods
    }
}