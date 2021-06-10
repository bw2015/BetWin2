using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace Web.GateWay.Handler
{
    /// <summary>
    /// PList 的摘要说明
    /// </summary>
    public class PList : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/xml";
            string xml = File.ReadAllText(context.Server.MapPath("~/App_Data/plist.xml"), Encoding.UTF8);
            Regex regex = new Regex(@"/(?<Name>\w+)$");
            if (!regex.IsMatch(context.Request.RawUrl))
            {
                return;
            }

            string name = regex.Match(context.Request.RawUrl).Groups["Name"].Value;
            string domain = context.Request.Url.Authority;
            if (domain.Contains(":")) domain = domain.Substring(0, domain.IndexOf(':'));
            xml = xml.Replace("${Domain}", domain).Replace("${Name}", name);
            context.Response.Write(xml);
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