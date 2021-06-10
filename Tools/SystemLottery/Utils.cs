using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SystemLottery
{
    public static class Utils
    {
        private const string URL = "http://a8.to/LotteryResult.ashx";
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Upload(string type, string key, Dictionary<string, string> data)
        {
            WebClient wc = new WebClient();
            wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Mobile/9B176 MicroMessenger/4.3.2";
            string url = URL + "?Type=" + type + "&Key=" + key;
            string postData = string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            return Encoding.UTF8.GetString(wc.UploadData(url, "POST", Encoding.UTF8.GetBytes(postData)));
        }
    }
}
