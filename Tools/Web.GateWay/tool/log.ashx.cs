using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using SP.Studio.ErrorLog;
using System.IO;

namespace Web.GateWay.tool
{
    /// <summary>
    /// 记录HTTP Log值
    /// </summary>
    public class log : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            string content = string.Join("\n\r", ErrorAgent.GetLog(context));
            foreach (string key in context.Request.Headers.AllKeys)
            {
                content += string.Format("{0}:{1}\n\r", key, context.Request.Headers[key]);
            }
            string folder = context.Server.MapPath("~/App_Data/Log");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            string file = folder + @"\" + fileName;

            File.AppendAllText(file, content, Encoding.UTF8);

            context.Response.Write("<Cad-rc-ok>");
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