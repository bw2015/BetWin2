using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.IO;

using SP.Studio.Net;
using SP.Studio.Model;
using SP.Studio.Web;

namespace Web.GateWay.Plugin
{
    /// <summary>
    /// 网关转发程序
    /// </summary>
    public class Alipay : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string url = context.Request.UrlReferrer == null ? string.Empty : context.Request.UrlReferrer.Authority;
            if (string.IsNullOrEmpty(url) || url.EndsWith("alipay.com") || url.EndsWith("allinpay.com"))
            {
                this.Notify(context);
            }
            else
            {
                this.Page(context);
            }
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="context"></param>
        private void Notify(HttpContext context)
        {
            context.Response.ContentType = "text/json";
            string gateway = WebAgent.GetParam("_gateway");
            if (string.IsNullOrEmpty(gateway))
            {
                context.Response.Write(false, "网关错误");
            }
            string url = string.Format("http://{0}/handler/Payment/AlipayAccount", gateway);
            List<string> data = new List<string>();
            foreach (string key in context.Request.Form.AllKeys)
            {
                data.Add(string.Format("{0}={1}", key, context.Request.Form[key]));
            }
            context.Response.Write(NetAgent.UploadData(url, string.Join("&", data), Encoding.UTF8));
        }

        /// <summary>
        /// 显示充值页面
        /// </summary>
        /// <param name="context"></param>
        private void Page(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            string keys = string.Join("|", new string[] { "Account", "QRCode", "AccountName", "Money", "Name", "OrderID", "UserName", "Time" }.Select(t => context.Request.Form[t]));
            string sign = context.Request.Form["Sign"];
            if (sign != SP.Studio.Security.MD5.Encryp(keys))
            {
                context.Response.Write(false, "密钥错误");
            }
            long time = WebAgent.QF("Time", (long)0);
            if (Math.Abs(time - WebAgent.GetTimeStamp()) > 300)
            {
                context.Response.Write(false, "订单超时，请重新提交");
            }
            string file = context.Server.MapPath("Alipay.html");
            if (!File.Exists(file))
            {
                context.Response.Write(false, "没有执行文件");
            }
            string html = File.ReadAllText(file, Encoding.UTF8);
            foreach (string key in context.Request.Form.AllKeys)
            {
                html = html.Replace("${" + key + "}", context.Request.Form[key]);
            }
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