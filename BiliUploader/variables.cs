using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace BiliUploader
{
    /// <summary>
    /// 通用变量
    /// </summary>
    internal class variables
    {

        #region Private Fields
        private static string _CookiesString;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Cookies集合
        /// </summary>
        public static CookieCollection Cookies { get; private set; }

        public static string csrf { get; private set; }

        /// <summary>
        /// Cookies字符串
        /// </summary>
        public static string CookiesString
        {
            get { return _CookiesString; }
            set
            {
                _CookiesString = value;
                Cookies = SetCookies(value);
                Console.WriteLine("已登录：" + Cookies["DedeUserID"].Value);
            }
        }

        /// <summary>
        /// 指示是否忽略错误
        /// </summary>
        public static bool IsIgnoreError { get; set; }

        /// <summary>
        /// 指示是否出现错误
        /// </summary>
        public static bool IsHasError { get; set; }

        /// <summary>
        /// 视频文件列表
        /// </summary>
        public static List<string> FileList = new List<string>();

        /// <summary>
        /// 指示是否删除压制缓存
        /// </summary>
        public static bool IsDeleteTmp { get; set; }

        /// <summary>
        /// 稿件标题
        /// </summary>
        public static string Title { get; set; }

        /// <summary>
        /// 封面图片
        /// </summary>
        public static string CoverFile { get; set; }

        /// <summary>
        /// 分区id
        /// </summary>
        public static int type { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public static string tags { get; set; }

        /// <summary>
        /// 稿件简介
        /// </summary>
        public static string desc { get; set; }

        /// <summary>
        /// 动态
        /// </summary>
        public static string dynamic { get; set; }

        /// <summary>
        /// 视频处理大小
        /// </summary>
        public static string v_size { get; set; }

        /// <summary>
        /// 视频处理帧数
        /// </summary>
        public static string v_fps { get; set; }

        /// <summary>
        /// 视频处理码率
        /// </summary>
        public static string v_rate { get; set; }
        /// <summary>
        /// 视频处理最高码率
        /// </summary>
        public static string v_maxrate { get; set; }

        /// <summary>
        /// 指示视频是否压制
        /// </summary>
        public static bool IsCompress { get; set; }

        /// <summary>
        /// 版权信息
        /// </summary>
        public static int copyright = 1;
        /// <summary>
        /// 活动id
        /// </summary>
        public static int mission_id = -1;

        private static int _dt = -1;

        /// <summary>
        /// 投稿时间
        /// </summary>
        public static int dt {
            get {
                if (_dt != -1 && GetNowTimeStamp() + 14400 > _dt) return GetNowTimeStamp() + 14400;
                else return _dt;
            }
            set { _dt = value; }
        }
        #endregion Public Properties

        #region Public Methods
        /// <summary>
        /// 字幕语言
        /// </summary>
        public static string Subtitle { get; set; }

        /// <summary>
        /// 获取当前时间戳
        /// </summary>
        /// <returns></returns>
        private static int GetNowTimeStamp()
        {
            return (int)(DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        /// <summary>
        /// 设置Cookies
        /// </summary>
        /// <param name="cookiestr">Cookies的字符串</param>
        public static CookieCollection SetCookies(string cookiestr)
        {
            try
            {
                CookieCollection public_cookie;
                public_cookie = new CookieCollection();
                cookiestr = cookiestr.Replace(",", "%2C");//转义“，”
                string[] cookiestrs = Regex.Split(cookiestr, "; ");
                foreach (string i in cookiestrs)
                {
                    string[] cookie = Regex.Split(i, "=");
                    public_cookie.Add(new Cookie(cookie[0], cookie[1]) { Domain = "member.bilibili.com" });

                    if (cookie[0] == "bili_jct")
                        csrf = cookie[1];
                }
                return public_cookie;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion Public Methods
    }
}