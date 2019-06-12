using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace BiliUploader
{
    class compressor
    {
        /// <summary>
        /// 压制函数
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns>压制结果</returns>
        public static async Task<string> compress(string filename)
        {
            Console.WriteLine("开始压制：" + filename);
            VideoInfo info = new VideoInfo();

            GetVideoInfo(filename, ref info);

            string Arguments = "-i " + filename + " -vcodec h264 -acodec aac";

            if (!string.IsNullOrEmpty(variables.v_size)) Arguments += " -s " + variables.v_size;
            else
            {
                if (int.Parse(info.Width) > 1920 || int.Parse(info.Height) > 1080)
                {
                    if (int.Parse(info.Width) >= int.Parse(info.Height))
                        Arguments += " -vf scale=1920:-1";
                    else
                        Arguments += " -vf scale=-1:1080";
                }
            }

            if (!string.IsNullOrEmpty(variables.v_fps)) Arguments += " -r " + variables.v_fps;
            else
                if (int.Parse(info.Fps) > 60) Arguments += " -r 60";

            if (!string.IsNullOrEmpty(variables.v_rate)) Arguments += " -b:v " + variables.v_rate + "K";
            else
                if (int.Parse(info.Rate) > 6000) Arguments += " -b:v 6000K";

            if (!string.IsNullOrEmpty(variables.v_maxrate)) Arguments += " -maxrate " + variables.v_maxrate + "K";
            else Arguments += " -maxrate 24000K";

            Arguments += " tmptoupload_" + new FileInfo(filename).Name + " -y";
            await DoCompress(Arguments);

            Console.WriteLine("压制成功：" + filename);

            return "tmptoupload_" + new FileInfo(filename).Name;
        }

        /// <summary>
        /// 执行压制
        /// </summary>
        /// <param name="arguments">压制参数</param>
        /// <returns></returns>
        private static async Task DoCompress(string arguments)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "ffmpeg.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                }
            };
            process.Start();

            await Task.Run(() =>{
                while (!process.HasExited)
                {
                    Console.WriteLine(process.StandardError.ReadToEnd());
                }
            });
        }

        /// <summary>
        /// 获取视频信息
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <param name="info">视频信息</param>
        private static void GetVideoInfo(string filename, ref VideoInfo info)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "ffmpeg.exe",
                    Arguments = "-i " + filename,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                }
            };
            process.Start();
            string str = process.StandardError.ReadToEnd();
            string[] msg = str.Split('\n');
            foreach (string i in msg)
            {
                if (i.Contains("Stream"))
                {
                    string[] tmp = i.Split(new char[] { ':' }, 4);
                    if (tmp[2] == " Video")
                    {
                        string[] tmp1 = tmp[3].Split(',');
                        info.Width = tmp1[2].Split(' ')[1].Split('x')[0];
                        info.Height = tmp1[2].Split(' ')[1].Split('x')[1];
                        info.Scale = tmp1[2].Split(' ')[5].Replace("]", "");
                        info.ScaleX = info.Scale.Split(':')[0];
                        info.ScaleY = info.Scale.Split(':')[1];
                        info.Fps = tmp1[4].Split(' ')[1];
                        info.Rate = tmp1[3].Split(' ')[1];
                    }
                }
            }
        }

        /// <summary>
        /// 视频信息模板
        /// </summary>
        private class VideoInfo
        {
            /// <summary>
            /// 视频宽
            /// </summary>
            public string Width { get; set; }
            /// <summary>
            /// 视频高
            /// </summary>
            public string Height { get; set; }
            /// <summary>
            /// 宽高比
            /// </summary>
            public string Scale { get; set; }
            /// <summary>
            /// 宽高比宽
            /// </summary>
            public string ScaleX { get; set; }
            /// <summary>
            /// 宽高比高
            /// </summary>
            public string ScaleY { get; set; }
            /// <summary>
            /// 视频帧率
            /// </summary>
            public string Fps { get; set; }
            /// <summary>
            /// 视频码率
            /// </summary>
            public string Rate { get; set; }
        }

        /// <summary>
        /// 删除缓存文件
        /// </summary>
        public static void DeleteTmp()
        {
            Regex reg = new Regex("tmptoupload_ *.*");
            foreach (string file in variables.FileList)
            {
                if (reg.IsMatch(file))
                {
                    File.Delete(file);
                }
            }
        }
    }
}
