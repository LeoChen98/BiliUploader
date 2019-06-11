using System;
using System.IO;
using System.Web;
using System.Text;

namespace BiliUploader
{
    /// <summary>
    /// 用于将文件转换为Base64
    /// </summary>
    internal class File2Base64Helper
    {
        #region Public Enums

        /// <summary>
        /// 文件类型枚举
        /// </summary>
        public enum FileExtension
        {
            JPG = 255216,
            GIF = 7173,
            PNG = 13780,
            SWF = 6787,
            RAR = 8297,
            ZIP = 8075,
            _7Z = 55122,
            VALIDFILE = 9999999
        }

        #endregion Public Enums

        #region Public Methods

        /// <summary>
        /// 获取文件类型
        /// </summary>
        /// <param name="btype">文件前两个字节</param>
        /// <returns>文件类型</returns>
        public static FileExtension CheckFileExtension(byte[] btype)
        {
            string fileType = string.Empty;

            fileType = btype[0].ToString();
            fileType += btype[1].ToString();
            FileExtension extension;
            try
            {
                extension = (FileExtension)Enum.Parse(typeof(FileExtension), fileType);
            }
            catch
            {
                extension = FileExtension.VALIDFILE;
            }
            return extension;
        }

        /// <summary>
        /// 图像文件转换到Base64
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns>Base64</returns>
        public static string ImageToBase64(string filename)
        {
            using (FileStream fs = File.OpenRead(filename))
            {
                byte[] tmp = new byte[fs.Length];
                fs.Read(tmp, 0, (int)fs.Length);
                string header;
                switch (CheckFileExtension(new byte[2] { tmp[0], tmp[1] }))
                {
                    case FileExtension.JPG:
                        header = "data:image/jpeg;base64,";
                        break;

                    case FileExtension.GIF:
                        header = "data:image/gif;base64,";
                        break;

                    case FileExtension.PNG:
                        header = "data:image/png;base64,";
                        break;

                    default:
                        header = "data:image/png;base64,";
                        break;
                }
                return HttpUtility.UrlEncode(header + Convert.ToBase64String(tmp),Encoding.UTF8);
            }
        }

        /// <summary>
        /// 文件转换到Base64
        /// </summary>
        /// <param name="filename">文件路径</param>
        /// <returns>Base64</returns>
        public static string ToBase64(string filename)
        {
            using (FileStream fs = File.OpenRead(filename))
            {
                byte[] tmp = new byte[fs.Length];
                fs.Read(tmp, 0, (int)fs.Length);
                return Convert.ToBase64String(tmp);
            }
        }

        #endregion Public Methods
    }
}