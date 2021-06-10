using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Net;
using System.Diagnostics;
using BW.Agent;
using System.IO;

using SP.Studio.ErrorLog;

namespace BW.Framework
{
    /// <summary>
    /// http访问执行方法
    /// </summary>
    public sealed class BetModule : IHttpModule
    {
        #region ==============  系统常量  ==============

        /// <summary>
        /// 站点信息
        /// </summary>
        public const string SITEINFO = "SITEINFO";

        /// <summary>
        /// 通过http头传递的站点ID信息
        /// </summary>
        public const string SITEID = "SITEID";

        /// <summary>
        /// 用户信息
        /// </summary>
        public const string USERINFO = "USERINFO";

        /// <summary>
        /// 用户登录的cookie值保存字段
        /// </summary>
        internal const string USERKEY = "USER";

        /// <summary>
        /// 管理员登录cookie保存的值
        /// </summary>
        internal const string AMDINKEY = "ADMIN";

        /// <summary>
        /// 内部信息的传递KEY
        /// </summary>
        internal const string MESSAGE = "MESSAGE";

        /// <summary>
        /// 当前登录的管理员
        /// </summary>
        internal const string ADMININFO = "ADMININFO";

        /// <summary>
        /// 唯一浏览器标识
        /// </summary>
        internal const string BOWSER = "GHOST";

        /// <summary>
        /// 效率运行监视器
        /// </summary>
        internal const string STOPWATCH = "STOPWATCH";

        /// <summary>
        /// 读取锁定信息的常量
        /// </summary>
        internal const string LOCK_NOTIFY = "LOCK_NOTIFY";

        /// <summary>
        /// 当前的时间
        /// </summary>
        internal const string DATETIME = "DATETIME_NOW";

        /// <summary>
        /// 按用户分布存储的数量
        /// </summary>
        public const int TABLE_COUNT = 5;

        #endregion

        private int SiteID = 0;

        /// <summary>
        /// 系统的缓存依赖项（依赖于站点缓存，如果站点缓存发生变化则所有的缓存失效）
        /// </summary>
        internal static CacheDependency SiteCacheDependency
        {
            get
            {
                return new CacheDependency(null, new string[] { SITEINFO + "_" + SysSetting.GetSetting().GetSiteID() });
            }
        }

        private Stopwatch sw = null;

        public void Dispose()
        {

        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.Error += context_Error;
            context.EndRequest += Context_EndRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            sw = new Stopwatch();
            sw.Start();
            app.Context.Items.Add(STOPWATCH, sw);
            app.Context.Items.Add(DATETIME, DateTime.Now);

            string url = app.Request.RawUrl;
            string domain = app.Request.Url.Authority;
            int siteID = this.SiteID = SysSetting.GetSetting().GetSiteID();

            if (siteID == 0)
            {
                Utils.ShowError(app.Context, HttpStatusCode.BadRequest);
                return;
            }

            app.Context.Items.Add(SITEINFO, SiteAgent.Instance().GetSiteInfo());
            app.Context.Items.Add(USERINFO, UserAgent.Instance().GetUserInfo());
            if (url.StartsWith("/admin"))
            {
                app.Context.Items.Add(ADMININFO, AdminAgent.Instance().GetAdminInfo());
            }

            //UserAgent.Instance().GetBowserID();

            app.Response.Headers.Set("Server", "nginx/1.2.8");
        }

        void context_Error(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (!SysSetting.START)
            {
                BW.Utils.ShowError(app.Context, HttpStatusCode.ResetContent, Guid.Empty.ToString("N"));
            }
            Exception ex = app.Server.GetLastError();
            int httpCode;
            string errorId;
            string content = ErrorAgent.CreateDetail(ex, out errorId, out httpCode);

            try
            {
                SNAPAgent.Instance().AddErrorLog(this.SiteID, errorId, httpCode, content, ex.Message);
            }
            catch (Exception logEx)
            {
                string logPath = app.Server.MapPath("~/App_Data/ErrorLog/" + DateTime.Now.ToString("yyyyMMdd") + ".log");
                SP.Studio.IO.FileAgent.CreateDirectory(logPath, true);

                File.AppendAllText(logPath, string.Format("[{0}]错误日志写入数据库失败\n\r{1}\n\r", DateTime.Now, logEx.Message));
                File.AppendAllText(logPath, string.Format("ErrorID:{0}\n{1}\n\r\n\r{2}\n\r\n\r", errorId, content, "".PadLeft(32, '=')));

            }
            BW.Utils.ShowError(app.Context, HttpStatusCode.InternalServerError, errorId);
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            if (sw != null)
            {
                short time = (short)sw.ElapsedMilliseconds;
                SNAPAgent.Instance().AddHandlerLog(time);
                sw.Stop();
            }
        }
    }
}
