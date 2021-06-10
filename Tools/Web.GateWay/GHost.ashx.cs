using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;

using SP.Studio.Web;

namespace Web.GateWay
{
    /// <summary>
    /// 写入客户端Cookie
    /// </summary>
    public class GHost : IHttpHandler
    {
        private const string KEY = "GHOST";

        public void ProcessRequest(HttpContext context)
        {
            string ghost = WebAgent.QC(KEY);
            if (!WebAgent.IsType<Guid>(ghost))
            {
                ghost = Guid.NewGuid().ToString("N").ToLower();
                context.Response.Cookies[KEY].Value = ghost;
                context.Response.Cookies[KEY].Expires = DateTime.Now.AddYears(1);
            }

            StringBuilder sb = new StringBuilder();

            context.Response.ContentType = "application/x-javascript";
            string file = context.Server.MapPath("~/scripts/ghost.js");
            string script = File.ReadAllText(file, Encoding.UTF8).Replace("00000000000000000000000000000000", ghost);
            context.Response.Write(script);

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