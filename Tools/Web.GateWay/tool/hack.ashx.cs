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
    /// hack 的摘要说明
    /// </summary>
    public class hack : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string content = string.Join("\n\r", ErrorAgent.GetLog(context));
            string folder = context.Server.MapPath("~/App_Data/Hack");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = DateTime.Now.ToString("yyyyMMdd") + ".log";
            string file = folder + @"\" + fileName;

            File.AppendAllText(file, content, Encoding.UTF8);

            context.Response.ContentType = "image/png";
            context.Server.Transfer("~/images/app-bg2.jpg");
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