using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Text;
using System.IO;
using System.Configuration;


namespace Web.GateWay.game
{
    /// <summary>
    /// BBIN 的摘要说明
    /// </summary>
    public class BBIN : IHttpHandler
    {
        private string domain
        {
            get
            {
                return HttpContext.Current.Request.Url.Authority;
            }
        }

        /// <summary>
        /// 远程网关
        /// </summary>
        private string GATEWAY
        {
            get
            {
                return "http://linkapi.ayl.ag/app/WebService/XML/display.php/";
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/xml";
            string url;
            switch (context.Request.HttpMethod)
            {
                case "POST":
                     url = string.Format("{0}{1}", GATEWAY, context.Request.QueryString["Method"]);
                    string data = string.Join("&", context.Request.Form.AllKeys.Select(t => string.Format("{0}={1}", t, context.Request.Form[t])));
                    url += "?" + data;
                    using (WebClient wc = new WebClient())
                    {
                        byte[] result = wc.DownloadData(url);
                        string strResult = Encoding.UTF8.GetString(result);
                        context.Response.Write(strResult);
                    }
                    break;
                case "GET":
                     url = string.Format("{0}{1}", GATEWAY, context.Request.QueryString["Method"]);
                    break;
                default:
                    context.Response.Write(context.Request.HttpMethod);
                    break;
            }
            
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}