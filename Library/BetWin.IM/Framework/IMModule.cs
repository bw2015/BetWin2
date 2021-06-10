using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;

using SP.Studio.IO;
using BW.IM.Agent;
using SP.Studio.ErrorLog;

namespace BW.IM.Framework
{
    /// <summary>
    /// 访问执行方法
    /// </summary>
    public sealed class IMModule : IHttpModule
    {
        public void Dispose()
        {
            // 是否可在此执行websocket结束的方法？
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.Error += context_Error;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            app.Context.Items.Add(Utils.SITEINFO, SiteAgent.Instance().GetSiteID(app.Context));
            app.Context.Items.Add(Utils.USERINFO, UserAgent.Instance().GetUserInfo(app.Context) ?? UserAgent.Instance().GetAdminInfo(app.Context));

            app.Response.Headers.Set("Server", "nginx/1.2.8");
        }

        void context_Error(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            Exception ex = app.Server.GetLastError();
            int httpCode;
            string errorId;
            string content = ErrorAgent.CreateDetail(ex, out errorId, out httpCode);

            try
            {
                SiteAgent.Instance().AddErrorLog(0, ex, ex.Message);
            }
            catch (Exception logEx)
            {
                string logPath = app.Server.MapPath("~/App_Data/ErrorLog/" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                SP.Studio.IO.FileAgent.CreateDirectory(logPath, true);

                File.AppendAllText(logPath, string.Format("[{0}]错误日志写入数据库失败\n\r{1}\n\r", DateTime.Now, logEx.Message));

                File.AppendAllText(logPath, string.Format("ErrorID:{0}\n{1}\n\r\n\r{2}\n\r\n\r", errorId, content, "".PadLeft(32, '=')));

            }
            Utils.showerror(app.Context, errorId);
        }
    }
}
