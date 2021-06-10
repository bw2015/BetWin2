using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using SP.Studio.Security;
using Web.GateWay.App_Code;
using SP.Studio.Array;

namespace Web.GateWay.Handler
{
    /// <summary>
    /// 生成一个plist
    /// </summary>
    public class App : IHttpHandler
    {
        private const string KEY = "BetWin";

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string url = context.Request.RawUrl;
            Regex regex = new Regex(@"/(?<SiteID>\d{4})(?<Sign>[0-9A-Z]{2,3})$");
            if (!regex.IsMatch(url)) return;

            int siteId = int.Parse(regex.Match(url).Groups["SiteID"].Value);
            string sign = regex.Match(url).Groups["Sign"].Value;

            switch (sign)
            {
                case "AG":
                case "PT":
                    this.ShowAppDownloadPage(context, sign);
                    break;
                default:
                    this.ShowAppDownloadPage(context, siteId, sign);
                    break;
            }


        }

        private void ShowAppDownloadPage(HttpContext context, int siteId, string sign)
        {
            if (!MD5.toMD5(siteId + KEY).ToUpper().StartsWith(sign)) return;

            string file = context.Server.MapPath("~/app/app.html");
            if (!File.Exists(file)) return;
            string html = File.ReadAllText(file, Encoding.UTF8);

            using (GatewayAgent agent = new GatewayAgent())
            {
                Dictionary<string, string> dic = agent.GetSiteSetting(siteId);
                if (dic == null)
                {
                    context.Response.Write(siteId);
                    return;
                }
                List<string> domain = agent.GetDomain(siteId);

                string apk = dic.Get("APPAndroid", string.Empty);
                string ipa = dic.Get("APPIOS", string.Empty);

                if (!string.IsNullOrEmpty(apk) && !apk.StartsWith("http"))
                {
                    apk = string.Format("http://{0}{1}", domain.FirstOrDefault(), apk);
                }
                html = html.Replace("${NAME}", dic.Get("SiteName", "APP下载"))
                    .Replace("${APK}", apk)
                    .Replace("${IPA}", ipa);
            }
            context.Response.Write(html);
        }

        private void ShowAppDownloadPage(HttpContext context, string type)
        {
            string file = context.Server.MapPath("~/app/app.html");
            if (!File.Exists(file)) return;
            string html = File.ReadAllText(file, Encoding.UTF8);
            string apk = string.Empty;
            string ipa = string.Empty;
            switch (type)
            {
                case "AG":
                    apk = "https://a8.to/app/ag.apk";
                    ipa = "";
                    break;
                case "PT":
                    apk = "https://a8.to/app/pt-slot.apk";
                    break;
            }
            html = html.Replace("${NAME}", string.Format("{0}客户端下载", type))
                   .Replace("${APK}", apk)
                   .Replace("${IPA}", ipa);
            context.Response.Write(html);
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