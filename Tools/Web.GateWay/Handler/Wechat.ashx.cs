using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Web.GateWay.App_Code;
using System.Reflection;
using SP.Studio.GateWay.WeChat;
using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.Json;

namespace Web.GateWay.Handler
{
    /// <summary>
    /// 微信公众号接口的请求
    /// </summary>
    public class Wechat : IHttpHandler
    {
        private WXSetting wx;
        private int siteId;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            Regex regex = new Regex(@"^/wxapi/(?<SiteID>\d{4})/(?<Type>\w+)", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(context.Request.RawUrl)) return;

            siteId = int.Parse(regex.Match(context.Request.RawUrl).Groups["SiteID"].Value);
            string type = regex.Match(context.Request.RawUrl).Groups["Type"].Value;
            wx = new WXAgent().GetWXSetting(siteId);
            string token = new WXAgent().GetToken(siteId, wx);
            MethodInfo method = this.GetType().GetMethod(type, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                context.Response.Write(false, "未找到方法" + type);
                return;
            }

            method.Invoke(this, new object[] { context });
        }

        private void checkSignature(HttpContext context)
        {
            if (context.Request.HttpMethod != "GET") return;
            if (WX.checkSignature(wx.Token, WebAgent.QS("timestamp"), WebAgent.QS("nonce"), WebAgent.QS("signature")))
            {
                context.Response.Write(WebAgent.QS("echostr"));
                context.Response.End();
            }
        }

        /// <summary>
        /// 微信快捷登录
        /// </summary>
        /// <param name="context"></param>
        private void login(HttpContext context)
        {
            string code = WebAgent.QS("code");
            string state = HttpUtility.HtmlDecode(WebAgent.QS("state"));
            if (string.IsNullOrEmpty(code)) return;
            string invite = null;
            string type = null;

            Regex regex = new Regex(@"(?<ID>.+)\-(?<Type>\w+)");
            if (!regex.IsMatch(state))
            {
                invite = state;
            }
            else
            {
                Match match = regex.Match(state);
                invite = match.Groups["ID"].Value;
                type = match.Groups["Type"].Value;
            }

            UserToken token = WX.GetUserToken(wx.AppId, wx.AppSecret, code);

            if (string.IsNullOrEmpty(token.openid))
            {
                context.Response.Write(false, token.errmsg);
                return;
            }
            if (!new WXAgent().SaveUserToken(siteId, token))
            {
                context.Response.Write(false, "微信授权失败");
                return;
            }

            if (!new WXAgent().SaveUserInfo(siteId, token.openid))
            {
                context.Response.Write(false, "用户信息获取失败");
                return;
            }

            string domain = new GatewayAgent().GetMainDomainList(siteId).FirstOrDefault();
            context.Response.ContentType = "text/html";
            string gateway = string.Format("{0}/handler/user/account/wechatlogin", domain);

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>正在进入</title><body>")
                .AppendFormat("<form action=\"{0}\" id=\"form1\" method=\"post\">", gateway)
                .AppendFormat("<input type=\"hidden\" name=\"openid\" value=\"{0}\" />", token.openid)
                .AppendFormat("<input type=\"hidden\" name=\"invite\" value=\"{0}\" />", invite)
                .AppendFormat("<input type=\"hidden\" name=\"type\" value=\"{0}\" />", type)
                .Append("</form>")
                .AppendFormat("<script> document.getElementById('form1').submit(); </script>")
                .Append("</body></html>");

            context.Response.Write(sb);
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