using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.GateWay.Handler
{
    /// <summary>
    /// VR 的破解缓存类
    /// </summary>
    public class VR : IHttpHandler
    {

        private static string TOKEN = null;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            switch (context.Request.HttpMethod)
            {
                case "POST":
                    this.saveToken(context.Request.Form["Token"]);
                    break;
                default:
                    string url = context.Request.QueryString["url"];
                    WebAgent.Redirect(url + "?" + TOKEN);
                    break;
            }
        }

        /// <summary>
        /// 保存Token
        /// </summary>
        /// <returns></returns>
        private void saveToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            TOKEN = token;
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