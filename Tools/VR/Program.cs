using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Web;

using SP.Studio.Net;
using SP.Studio.Core;

namespace VR
{
    class Program
    {
        /// <summary>
        /// 密钥
        /// </summary>
        private static string KEY
        {
            get
            {
                return ConfigurationManager.AppSettings["key"];
            }
        }

        static void Main(string[] args)
        {
            //POST(GATEWAY.REGISTER, new
            //{
            //    playerName = "ceshi01"
            //});

            using (WebClient wc = new WebClient())
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("version", "1.0");
                dic.Add("id", "BY");
                dic.Add("data", AES.Encrypt(string.Format("playerName=ceshi02&loginTime={0}", DateTime.Now.AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ssZ")), KEY));
                string postData = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));

                string url = GATEWAY.LOGIN + "?" + postData;
                WebHeaderCollection header;
                HttpStatusCode code = NetAgent.GetHttpCode(url, out header);

                Console.WriteLine("登录地址：{0}", url);
                Console.WriteLine("返回信息：{0}", code);

                foreach (string key in header.AllKeys)
                {
                    Console.WriteLine("\t{0}:{1}", key, header[key]);
                }
            }

        }

        private static void POST(string gateway, object data)
        {
            string json = data.ToJson();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("version", "1.0");
            dic.Add("id", "BY");
            dic.Add("data", AES.Encrypt(json, KEY));
            string postData = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = NetAgent.UploadData(gateway, postData, Encoding.UTF8);

            Console.WriteLine("加密前信息：{0}", json);
            Console.WriteLine("发送信息：{0}", postData);
            Console.WriteLine("返回信息：{0}", result);
            Console.WriteLine("解密信息：{0}", AES.Decrypt(result, KEY));
        }
    }

    /// <summary>
    /// 网关接口地址
    /// </summary>
    class GATEWAY
    {
        /// <summary>
        /// 新增玩家账户
        /// </summary>
        public const string REGISTER = "http://vr.betwin.ph/Account/CreateUser";

        public const string LOGIN = "http://vr.betwin.ph/Account/LoginValidate";
    }
}
