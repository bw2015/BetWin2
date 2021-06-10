using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Web.GateWay.App_Code;

namespace Web.GateWay.Handler
{
    /// <summary>
    /// Redirect 的摘要说明
    /// </summary>
    public class Redirect : IHttpHandler
    {

        private const string KEY = "BetWin";

        private Regex regex = new Regex(@"^/(?<Type>\w+)/(?<SiteID>\d{4})(?<Code>\w{3})$", RegexOptions.IgnoreCase);


        private Regex cacheRegex = new Regex(@"^/(?<ID>[0-9A-F]{32})$", RegexOptions.IgnoreCase);

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Expires = -1;
            context.Response.ExpiresAbsolute = DateTime.Now.AddYears(-1);
            string url = context.Request.RawUrl;
            if (regex.IsMatch(url))
            {
                this.GetSpeed(context);
            }
            else if (cacheRegex.IsMatch(url))
            {
                this.GetCache(context);
            }
            else
            {
                this.GetRedirect(context);
            }
        }

        /// <summary>
        /// 获取域名列表
        /// </summary>
        /// <param name="context"></param>
        private void GetSpeed(HttpContext context)
        {
            string url = context.Request.RawUrl;
            string type = regex.Match(url).Groups["Type"].Value;
            int siteid = int.Parse(regex.Match(url).Groups["SiteID"].Value);
            string code = regex.Match(url).Groups["Code"].Value;
            if (siteid == 1011)
            {
                if (code != "2AB")
                {
                    context.Response.Write(code);
                    return;
                }
            }
            else if (!MD5.toMD5(siteid + KEY).StartsWith(code))
            {
                context.Response.Redirect("http://www.baidu.com");
            }
            url = string.Format("/handler/redirect.ashx?code=/{0}/{1}{2}&{3}", new GatewayAgent().GetType(siteid, type), siteid, code, Guid.NewGuid().ToString("N"));

            WebAgent.SuccAndGo(url);
        }

        private void GetRedirect(HttpContext context)
        {
            string url = WebAgent.QS("code");
            string type = regex.Match(url).Groups["Type"].Value;
            int siteid = int.Parse(regex.Match(url).Groups["SiteID"].Value);
            string code = regex.Match(url).Groups["Code"].Value;
            if (siteid == 1011)
            {
                if (code != "2AB")
                {
                    context.Response.Write(code);
                    return;
                }
            }
            else if (!MD5.toMD5(siteid + KEY).StartsWith(code))
            {
                context.Response.Redirect("http://www.baidu.com");
            }
            using (GatewayAgent agent = new GatewayAgent())
            {
                List<string> domain = agent.GetMainDomainList(siteid);
                if (domain.Count == 0)
                {
                    context.Response.Write("Not Found Domain");
                    return;
                }

                url = domain.FirstOrDefault();
                switch (type)
                {
                    case "wx":
                        url += "/wechat/";
                        break;
                    case "pc":
                        url += "/";
                        break;
                    case "mobile":
                        url += "/mobile/login.html?" + Guid.NewGuid().ToString("N").Substring(0, 8);
                        break;
                    default:
                        url += ("/" + type + "/");
                        break;
                }
                context.Response.Redirect(url);

                //string start = agent.GetContent(siteid, "APP-START");

                //string html = File.ReadAllText(context.Server.MapPath("~/Handler/redirect.html"), Encoding.UTF8);
                //context.Response.ContentType = "text/html";
                //context.Response.ContentEncoding = Encoding.UTF8;
                //html = html.Replace("${Domain}", string.Join(",", domain))
                //    .Replace("${START}", start);
                //context.Response.Write(html);
            }
        }

        private void GetCache(HttpContext context)
        {
            Guid id = Guid.Parse(cacheRegex.Match(context.Request.RawUrl).Groups["ID"].Value);
            using (GatewayAgent agent = new GatewayAgent())
            {
                byte type;
                XElement root = agent.GetCacheData(id, out type);
                if (root == null)
                {
                    context.Response.Write(id);
                    return;
                }
                StringBuilder sb = new StringBuilder();
                switch (type)
                {
                    case 0:
                        sb.Append("<html><head><title>正在提交</title></head><body>")
                            .AppendFormat("<form action=\"{0}\" method=\"post\" id=\"form1\">", root.Attribute("_gateway").Value);
                        foreach (XElement item in root.Elements())
                        {
                            sb.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", item.Attribute("key").Value, item.Value);
                        }
                        sb.Append("</form>")
                            .AppendFormat("<script> document.getElementById('form1').submit(); </script>")
                            .Append("</body></html>");
                        break;
                }
                context.Response.Write(sb);
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