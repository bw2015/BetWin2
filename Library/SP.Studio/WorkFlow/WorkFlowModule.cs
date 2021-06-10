using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

namespace SP.Studio.WorkFlow
{
    /// <summary>
    /// 工作流的URL事件
    /// 本类中绑定了 Init 事件。
    /// 工作流页面的URL重写事件
    /// </summary>
    public class WorkFlowModule : IHttpModule
    {
        static WorkFlowModule()
        {
            WorkFlowSetting.Install();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            Regex regex = new Regex(@"/WorkFlow/(?<PageID>\d+).aspx.*", RegexOptions.IgnoreCase);
            string url = app.Request.RawUrl;
            if (regex.IsMatch(url))
            {
                int pageID = int.Parse(regex.Replace(url, "${PageID}"));
                WorkPage page = WorkAgent.GetWorkPage(pageID);
                if (string.IsNullOrEmpty(page.Url)) throw new Exception(string.Format("页面{0}(ID:{1})未配置URL", page.Name, page.ID));
                app.Context.Items.Add("PAGEINFO", page);
                app.Context.RewritePath(page.Url);
            }
            //if (url.Equals("/WorkFlow/Create.aspx", StringComparison.CurrentCultureIgnoreCase)) { WorkAgent.CreateConfigurationFile(); WorkFlowSetting.LoadWorkFlowCache(); app.Context.Response.End(); }
            //if (url.Equals("/WorkFlow/Cache.aspx", StringComparison.CurrentCultureIgnoreCase)) { WorkFlowSetting.LoadWorkFlowCache(); app.Context.Response.End(); }
        }
    }
}
