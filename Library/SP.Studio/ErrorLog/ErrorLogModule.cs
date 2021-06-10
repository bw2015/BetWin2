using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.Configuration;
using System.IO;

using SP.Studio.Net;
using SP.Studio.IO;
using SP.Studio.Web;

namespace SP.Studio.ErrorLog
{
    /// <summary>
    /// 创建错误信息
    /// </summary>
    public class ErrorLogModule : IHttpModule
    {
        #region IHttpModule 成员

        public void Dispose()
        {

        }

        public void Init(HttpApplication context)
        {
            context.Error += new EventHandler(context_Error);
        }

        /// <summary>
        /// 处理错误日志
        /// </summary>
        void context_Error(object sender, EventArgs e)
        {
            HttpApplication app = ((HttpApplication)sender);
            Exception ex = app.Server.GetLastError();
            int httpCode;
            string errorID;
            string detail = ErrorAgent.CreateDetail(ex, out errorID, out httpCode);
            string title = ex.Message;
            var domain = app.Request.Url.Authority;
            if (ConfigurationManager.AppSettings.AllKeys.Contains("ErrorLog"))
            {
                string log = ConfigurationManager.AppSettings["ErrorLog"];
                string data;
                if (log.StartsWith("http://"))  // 保存到web服务器
                {
                    data = string.Format("ErrorID={0}&Title={1}&HttpCode={2}&Detail={3}&Domain={4}", errorID, HttpUtility.UrlEncode(title), httpCode, HttpUtility.UrlEncode(detail), HttpUtility.UrlEncode(domain));
                    NetAgent.UploadDataSync(log, data, null, Encoding.UTF8);
                }
                else if (log.EndsWith(".db"))   // 保存到Sqlite数据库
                {
                    new ErrorAgent().AddErrorLog(errorID, domain, title, detail, httpCode);
                }
                else if (Regex.IsMatch(log, @"^~|^(a-z):", RegexOptions.IgnoreCase))    // 保存到本地路径
                {
                    if (log.StartsWith("~")) log = app.Server.MapPath(log);
                    log += @"\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
                    data = string.Format("{2}\nTime\t:\t{0}\n{1}\n\n{2}\n", DateTime.Now, detail, string.Empty.PadLeft(128, '-'));
                    FileAgent.Write(log, data);
                }
                else if (WebAgent.IsEmail(log))     // 发送到邮箱
                {
                    throw new Exception("没有定义发送到电子邮箱的方法");
                }
            }
            else
            {
                new ErrorAgent().AddErrorLog(errorID, domain, title, detail, httpCode);
            }
        }

        #endregion
    }
}
