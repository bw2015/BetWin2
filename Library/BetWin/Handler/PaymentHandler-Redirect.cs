using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Reflection;
using System.Net;
using System.Diagnostics;
using System.Web.Caching;

using BW.Common.Users;

using BW.Agent;
using BW.Common.Sites;
using BW.PageBase;
using BW.Framework;
using SP.Studio.Web;
using SP.Studio.Core;

using BW.GateWay.Payment;

using SP.Studio.Array;
using SP.Studio.Model;
using SP.Studio.ErrorLog;


namespace BW.Handler
{
    /// <summary>
    /// 内部的跳转页面
    /// </summary>
    partial class PaymentHandler
    {
        #region ===========  常量   ==============

        /// <summary>
        /// 微信支付页面
        /// </summary>
        public const string REDIRECT_WX = "wx";

        /// <summary>
        /// QQ支付页面
        /// </summary>
        public const string REDIRECT_QQ = "qq";

        /// <summary>
        /// 支付宝支付页面
        /// </summary>
        public const string REDIRECT_ALIPAY = "alipay";

        /// <summary>
        /// 线下银行卡支付页面
        /// </summary>
        public const string REDIRECT_BANK = "bank";

        /// <summary>
        /// 通用的二维码扫描界面
        /// </summary>
        public const string REDIRECT_QRCODE = "qrcode";

        /// <summary>
        /// 基于插件的扫码支付
        /// </summary>
        public const string REDIRECT_PLUS = "pluscode";

        /// <summary>
        /// 支付提交页面
        /// </summary>
        public const string REDIRECT_PAY = "pay";

        #endregion


        /// <summary>
        /// 显示跳转域名
        /// </summary>
        /// <param name="context"></param>
        private void ShowRedirect(HttpContext context)
        {
            switch (WebAgent.GetParam(IPayment._GATETYPE))
            {
                case REDIRECT_WX:
                    if (WebAgent.IsWechat())
                    {
                        this.WechatPayment(context);
                    }
                    else
                    {
                        this.Wechat(context);
                    }
                    break;
                case REDIRECT_QQ:
                    this.QQ(context);
                    break;
                case REDIRECT_ALIPAY:
                    this.Alipay(context);
                    break;
                case REDIRECT_BANK:
                    this.Bank(context);
                    break;
                case REDIRECT_PLUS:
                    this.Plus(context);
                    break;
                case REDIRECT_QRCODE:
                    this.QRCode(context);
                    break;
                case REDIRECT_PAY:
                    this.Pay(context);
                    break;
                default:
                    this.Redirect(context);
                    break;
            }
        }

        /// <summary>
        /// 获取传递过来的数据，生成一个js变量
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string getPostData(HttpContext context)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in context.Request.Form.AllKeys)
            {
                dic.Add(key, context.Request.Form[key]);
            }
            return string.Format("<script type=\"text/javascript\">window[\"postdata\"] = {0};</script>", dic.ToJson());
        }


        /// <summary>
        /// 基于插件的扫码支付
        /// </summary>
        /// <param name="context"></param>
        private void Plus(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string html = this.getPostData(context) + BW.Resources.Res.Payment_PlusCode;
            context.Response.Write(html);
        }

        /// <summary>
        /// 跳转域名
        /// </summary>
        /// <param name="context"></param>
        private void Redirect(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string url = WebAgent.GetParam(IPayment._GATEWAY);
            StringBuilder sb = new StringBuilder();
            sb.Append("<html>")
                .Append("<head><title>正在加载</title></head>")
                .Append("<body>");

            sb.AppendFormat("<form action=\"{0}\" method=\"{1}\" id=\"{2}\">", url, context.Request.HttpMethod, this.GetType().Name);

            switch (context.Request.HttpMethod)
            {
                case "GET":
                    foreach (string key in context.Request.QueryString.AllKeys)
                    {
                        if (key == IPayment._GATEWAY) continue;
                        sb.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, context.Request.QueryString[key]);
                    }
                    break;
                case "POST":
                    foreach (string key in context.Request.Form.AllKeys)
                    {
                        if (key == IPayment._GATEWAY) continue;
                        sb.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, context.Request.Form[key]);
                    }
                    break;
            }
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);
            sb.Append("</body></html>");

            context.Response.Write(sb);
        }

        /// <summary>
        /// 通用二维码页面
        /// </summary>
        /// <param name="context"></param>
        private void QRCode(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string html = this.getPostData(context) + BW.Resources.Res.Payment_QRCode;
            context.Response.Write(html);
        }

        /// <summary>
        /// 微信二维码
        /// </summary>
        /// <param name="context"></param>
        private void Wechat(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            string id = Guid.NewGuid().ToString("N");
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (string key in context.Request.Form.AllKeys)
            {
                data.Add(key, context.Request.Form[key]);
            }

            HttpRuntime.Cache.Insert(id, data, BetModule.SiteCacheDependency, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));

            string html = this.ReplaceContent(context, BW.Resources.Res.Payment_Wechat, "Site", SiteInfo.Name, "ID", id);
            context.Response.Write(html);
        }

        /// <summary>
        /// 微信公共号支付页面
        /// </summary>
        /// <param name="context"></param>
        private void WechatPayment(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string id = WebAgent.QS("id");
            if (string.IsNullOrEmpty(id))
            {
                context.Response.Write("二维码已过期");
                return;
            }
            Dictionary<string, string> data = (Dictionary<string, string>)HttpRuntime.Cache[id];
            if (data == null)
            {
                context.Response.Write("二维码已过期");
                return;
            }
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> item in data)
            {
                sb.AppendFormat("<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", item.Key, item.Value);
            }

            string html = this.ReplaceContent(context, BW.Resources.Res.Payment_WechatSubmit, "Input", sb.ToString());
            context.Response.ContentType = "text/html";
            context.Response.Write(html);
        }

        /// <summary>
        /// QQ的二维码支付
        /// </summary>
        /// <param name="context"></param>
        private void QQ(HttpContext context)
        {
            context.Response.ContentType = "text/html";

            string id = Guid.NewGuid().ToString("N");
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (string key in context.Request.Form.AllKeys)
            {
                data.Add(key, context.Request.Form[key]);
            }
            HttpRuntime.Cache.Insert(id, data, BetModule.SiteCacheDependency, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
            string html = this.ReplaceContent(context, BW.Resources.Res.Payment_QQ, "Site", SiteInfo.Name, "ID", id);
            context.Response.ContentType = "text/html";
            context.Response.Write(html);
        }

        /// <summary>
        /// 支付宝的二维码显示页面
        /// </summary>
        /// <param name="context"></param>
        private void Alipay(HttpContext context)
        {
            string html = this.ReplaceContent(context, BW.Resources.Res.Payment_Alipay, "Site", SiteInfo.Name);
            context.Response.ContentType = "text/html";
            context.Response.Write(html);
        }

        /// <summary>
        /// 网银支付
        /// </summary>
        /// <param name="context"></param>
        private void Bank(HttpContext context)
        {
            string html = this.ReplaceContent(context, BW.Resources.Res.Payment_Bank);
            context.Response.ContentType = "text/html";
            context.Response.Write(html);
        }
    }
}
