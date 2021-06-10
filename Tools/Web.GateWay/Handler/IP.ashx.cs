using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SP.Studio.Web;
namespace Web.GateWay.Handler
{
    /// <summary>
    /// IP 的摘要说明
    /// </summary>
    public class IP : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/javascript";
            string ipAddress = IPAgent.GetAddress();
            context.Response.Write(string.Format("document.write('{0} ({1})');", IPAgent.IP, string.IsNullOrEmpty(ipAddress) ? "未知" : ipAddress));
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