using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.GateWay.Handler
{
    /// <summary>
    /// 跨平台的参数配置文件
    /// </summary>
    public class Config : IHttpHandler
    {

        private static Dictionary<string, string> cache = new Dictionary<string, string>();

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            switch (context.Request.HttpMethod)
            {
                case "POST":
                    string post_key = context.Request.Form["Key"];
                    string post_value = context.Request.Form["Value"];
                    if (!string.IsNullOrEmpty(post_key))
                    {
                        if (cache.ContainsKey(post_key))
                        {
                            cache[post_key] = post_value;
                        }
                        else
                        {
                            cache.Add(post_key, post_value);
                        }
                    }
                    context.Response.Write(post_value);
                    break;
                case "GET":
                    string get_key = context.Request.QueryString["Key"];
                    string get_value = string.Empty;
                    if (!string.IsNullOrEmpty(get_key))
                    {
                        get_value = cache.ContainsKey(get_key) ? cache[get_key] : string.Empty;
                    }
                    context.Response.Write(get_value);
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