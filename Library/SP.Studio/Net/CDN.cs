using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;

using SP.Studio.IO;

namespace SP.Studio.Net
{
    /// <summary>
    /// CDN Content Delivery Network
    /// 内容分发系统 本地网络加速
    /// </summary>
    public class CDN : IHttpModule
    {
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication context = (HttpApplication)sender;
            string path = context.Request.RawUrl;
            if (path.Contains('?')) path = path.Substring(0, path.IndexOf('?'));
            if (!File.Exists(context.Server.MapPath(path)))
            {
                FileAgent.CreateDirectory(context.Server.MapPath(path), true);
                NetAgent.DownloadFile(string.Format("http:/{0}", path), context.Server.MapPath(path));
            }
        }
    }
}
