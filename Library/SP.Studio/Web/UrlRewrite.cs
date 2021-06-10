using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Configuration;
using System.Xml;

namespace SP.Studio.Web
{
    /// <summary>
    /// URL重写类。使用方法：
    /// add <section name="UrlRewriting" requirePermission="false" type="SP.Studio.Web.UrlRewriting" /> To configSections   只允许一个 <configSections> 元素。它必须是根 <configuration> 元素的第一个子元素 
    /// 在configuration添加 <UrlRewriting> <Rule Name="竞拍详细页面"> <LookFor></LookFor><SendTo></SendTo></Rule> .... </UrlRewriting>
    /// Modules过滤模块 <httpModules> <add name="UrlRewriter" type="SP.Studio.Web.UrlRewriterMoule" />  </httpModules>  To System.Web
    /// </summary>
    public class UrlRewriterMoule : IHttpModule
    {
        /// <summary>
        /// 路径
        /// </summary>
        protected virtual string Path
        {
            get
            {
                string path = HttpContext.Current.Request.RawUrl;
                if (path.Contains("?")) return path.Substring(0, path.LastIndexOf('?'));
                return path;
            }
        }

        static List<UrlRewriter> UrlList;

        /// <summary>
        /// 替换动态的标记。 用于被继承的类中写入该值
        /// </summary>
        protected Dictionary<string, string> UrlMatchList = new Dictionary<string, string>();

        static UrlRewriterMoule()
        {
            UrlList = (List<UrlRewriter>)ConfigurationManager.GetSection("UrlRewriting");
        }

        public void Dispose()
        {

        }

        /// <summary>
        /// 替换转化出去的地址
        /// </summary>
        protected virtual string SendTo(string sendTo)
        {
            return sendTo;
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
            context.AcquireRequestState += new EventHandler(context_AcquireRequestState);
            context.Error += context_Error;
            context.EndRequest += context_EndRequest;
        }

        /// <summary>
        /// 页面执行完成之后触发
        /// </summary>
        protected virtual void context_EndRequest(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 发生错误时候的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void context_Error(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 在该事件中才可以取到Session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void context_AcquireRequestState(object sender, EventArgs e)
        {

        }

        protected virtual void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            foreach (UrlRewriter u in UrlList)
            {
                if (u.LookFor.IsMatch(Path))
                {
                    string url = u.LookFor.Replace(Path, this.SendTo(u.SendTo));
                    if (UrlMatchList != null && UrlMatchList.Count > 0)
                    {
                        foreach (var urlMatch in UrlMatchList)
                        {
                            url = url.Replace(urlMatch.Key, urlMatch.Value);
                        }
                    }
                    string queryString = app.Request.QueryString.ToString();
                    if (url.Contains("?"))
                    {
                        var query = new Uri("http://localhost" + url).Query;
                        if (query.StartsWith("?")) query = query.Substring(1);
                        if (string.IsNullOrEmpty(queryString))
                        {
                            queryString = query;
                        }
                        else if (!string.IsNullOrEmpty(query))
                        {
                            queryString = query + (query.EndsWith("&") ? "" : "&") + queryString;
                        }
                        url = url.Substring(0, url.IndexOf('?'));
                    }

                    HttpContext.Current.RewritePath(url, null, queryString);

                    return;
                }
            }
        }
    }



    public class UrlRewriting : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<UrlRewriter> list = new List<UrlRewriter>();
            foreach (XmlNode node in section.ChildNodes)
            {
                string lookFor, sendTo;
                lookFor = sendTo = null;
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "LookFor":
                            lookFor = node2.InnerText;
                            break;
                        case "SendTo":
                            sendTo = node2.InnerText;
                            break;
                    }
                }
                list.Add(new UrlRewriter(lookFor, sendTo));
            }
            return list;
        }
    }

    /// <summary>
    /// URL重写配置类
    /// </summary>
    public struct UrlRewriter
    {
        public UrlRewriter(string lookFor, string sendTo)
        {
            LookFor = new Regex(lookFor, RegexOptions.IgnoreCase);
            SendTo = sendTo;
        }

        public readonly Regex LookFor;

        public readonly string SendTo;
    }

    /// <summary>
    /// 限定的只允许post动作访问
    /// </summary>
    public class MyHttpPostAttribute : Attribute { }

    /// <summary>
    /// 只允许get动作访问
    /// </summary>
    public class MyHttpGetAttribute : Attribute { }
}
