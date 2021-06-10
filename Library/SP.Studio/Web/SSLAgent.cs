using Newtonsoft.Json.Linq;
using SP.Studio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SP.Studio.Web
{
    public static class SSLAgent
    {
        /// <summary>
        /// 解析SSL证书的内容
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static CertInfo GetCertInfo(string cert)
        {
            string data = "type=paste&cert=" + HttpUtility.UrlEncode(cert);
            string url = "https://myssl.com/api/v1/tools/cert_decode";
            string result = NetAgent.UploadData(url, data, Encoding.UTF8);
            try
            {
                JObject info = JObject.Parse(result);
                if (info["error"].Type != JTokenType.Null)
                {
                    return new CertInfo()
                    {
                        Message = info["error"].Value<string>()
                    };
                }

                info = (JObject)info["data"]["info"];

                return new CertInfo()
                {
                    Success = true,
                    Message = info["subject"]["common_name"].Value<string>(),
                    ExpireAt = info["end_date"].Value<DateTime>(),
                    Domain = ((JArray)info["sans"]).Select(t => t.Value<string>()).ToArray()
                };
            }
            catch (Exception ex)
            {
                return new CertInfo()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }

    /// <summary>
    /// 证书内容
    /// </summary>
    public struct CertInfo
    {
        /// <summary>
        /// 解析成功
        /// </summary>
        public bool Success;

        /// <summary>
        /// 返回的信息
        /// </summary>
        public string Message;

        public DateTime ExpireAt;

        public string[] Domain;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Name\":\"{0}\",", Message)
                .AppendFormat("\"ExpireAt\":\"{0}\",", ExpireAt)
                .AppendFormat("\"Domain\":[{0}]", string.Join(",", this.Domain.Select(t => $"\"{t}\"")))
                .Append("}");
            return sb.ToString();
        }
    }

}
