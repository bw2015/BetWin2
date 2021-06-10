using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;

using SP.Studio.Data;
using SP.Studio.Web;
using SP.Studio.Array;
using Web.GateWay.App_Code;

namespace Web.GateWay
{
    /// <summary>
    /// 邀请码的跳转
    /// </summary>
    public class Invite : IHttpHandler
    {
        private static Dictionary<string, string> _rediretc;
        private static Dictionary<string, string> Rediretc
        {
            get
            {
                if (_rediretc == null)
                {
                    _rediretc = new Dictionary<string, string>();
                    string path = HttpContext.Current.Server.MapPath("~/App_Data/Redirect.xml");
                    if (File.Exists(path))
                    {
                        XElement root = XElement.Parse(File.ReadAllText(path, Encoding.UTF8));
                        foreach (XElement item in root.Elements())
                        {
                            _rediretc.Add(item.Attribute("key").Value, item.Attribute("value").Value);
                        }
                    }
                }
                return _rediretc;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string id = context.Request.QueryString["ID"];
            if (Rediretc.ContainsKey(id))
            {
                WebAgent.Redirect(Rediretc[id]);
                return;
            }

            string url = context.Request.RawUrl;

            using (GatewayAgent agent = new GatewayAgent())
            {
                string domain = agent.GetInviteDomain(id).FirstOrDefault();
                int siteId = agent.GetSiteID(id);
                string protocol = "http";
                Regex https = new Regex(":443$");
                if (!string.IsNullOrEmpty(domain) && https.IsMatch(domain))
                {
                    protocol = "https";
                    domain = https.Replace(domain, string.Empty);
                }
                if (!string.IsNullOrEmpty(domain))
                {
                    ///跳转至微信
                    if (url.StartsWith("/wx/"))
                    {
                        domain = string.Format("{0}://{1}/wechat/index.html#{2}", protocol, domain, id);
                        if (WebAgent.IsWechat())
                        {
                            var setting = new WXAgent().GetWXSetting(siteId);
                            if (!string.IsNullOrEmpty(setting.AppId))
                            {
                                string wx_url = string.Format("http://{0}/wxapi/{1}/login", "a8.to", siteId);
                                string state = string.Format("{0}-wx", id);
                                domain = SP.Studio.GateWay.WeChat.WX.GetAuthorizeUrl(setting.AppId, wx_url, state, "snsapi_userinfo");

                            }
                        }
                        WebAgent.Redirect(domain);
                    }
                    if (WebAgent.IsMobile())
                    {
                        domain = string.Format("{0}://{1}/mobile/register.html?{2}", protocol, domain, id);
                    }
                    else
                    {
                        domain = string.Format("{0}://{1}/register.html#{2}", protocol, domain, id);
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.Write("404 Not Found");
                    return;
                }
                string html = File.ReadAllText(context.Server.MapPath("~/Handler/invite.html"), Encoding.UTF8);
                html = html.Replace("${LINK}", domain);
                context.Response.Write(html);
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