using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Web.SessionState;
using System.Configuration;
using SP.Studio.Xml;

namespace SP.Studio.Net
{
    /// <summary>
    /// 反向代理
    /// </summary>
    public class ReverseProxy
    {
        /// <summary>
        /// 创建一个反向代理管道
        /// </summary>
        /// <param name="context">当前实例</param>
        /// <param name="url">跳转至的网址</param>
        /// <param name="islog">保存日志的条件</param>
        /// <param name="modify">对传输的内容进行篡改</param>
        public static void Create(HttpContext context, string url, Func<bool> islog = null, Func<byte[], byte[]> modify = null)
        {
            HttpWebRequest request = (HttpWebRequest)(HttpWebRequest)HttpWebRequest.Create(url);
            request.AllowAutoRedirect = false;
            request.Method = context.Request.HttpMethod;
            request.Headers[HttpRequestHeader.Cookie] = context.Request.Headers["Cookie"];
            request.Headers[HttpRequestHeader.ProxyAuthorization] = context.Request.UserHostAddress;
            request.UserAgent = context.Request.UserAgent;
            request.ContentType = context.Request.ContentType;
            request.ContentLength = context.Request.ContentLength;
            request.Referer = context.Request.UrlReferrer == null ? url : context.Request.UrlReferrer.ToString();

            HttpWebResponse respose = null;
            byte[] data = null;
            Encoding encoding = context.Request.ContentEncoding;
            switch (request.Method)
            {
                case "POST":
                    Stream stream = context.Request.InputStream;
                    byte[] post = new byte[stream.Length];
                    stream.Read(post, 0, post.Length);
                    stream.Seek(0, SeekOrigin.Begin);
                    using (Stream reqStream = request.GetRequestStream())
                    {
                        reqStream.Write(post, 0, post.Length);
                        reqStream.Close();
                    }
                    break;
            }

            try
            {
                respose = (HttpWebResponse)request.GetResponse();

                ResposeHeader(context, respose, ref encoding);
                data = new byte[respose.ContentLength == -1 ? 1024 * 1024 : (int)respose.ContentLength];

                switch (respose.StatusCode)
                {
                    case HttpStatusCode.OK:
                        Stream stream = respose.GetResponseStream();
                        int readecount = 0;
                        int _readecount = -1;
                        while (readecount != _readecount)
                        {
                            _readecount = readecount;
                            readecount += stream.Read(data, readecount, data.Length - readecount);
                        }
                        stream.Close();
                        break;
                }

                respose.Close();

            }
            catch (WebException ex)
            {
                string html = null;
                respose = (HttpWebResponse)ex.Response;
                if (respose == null)
                {
                    html = string.Format("Error:{0}", ex.Message);
                }
                else
                {
                    ResposeHeader(context, respose, ref encoding);
                    StreamReader reader = new StreamReader(respose.GetResponseStream(), encoding);
                    html = reader.ReadToEnd();
                }
                data = encoding.GetBytes(html);
            }
            finally
            {
                if (islog != null && islog())
                {
                    if (!Directory.Exists(context.Server.MapPath("~/Log/"))) Directory.CreateDirectory(context.Server.MapPath("~/Log/"));

                    List<string> post = new List<string>();
                    foreach (string key in context.Request.Form.AllKeys)
                    {
                        post.Add(string.Format("{0}={1}", key, context.Request.Form[key]));
                    }
                    string space = string.Empty.PadLeft(16, ' ');
                    string file = DateTime.Now.ToString("yyyyMMddHHmmss");
                    List<string> log = new List<string>();
                    log.Add(string.Format("[{4}]{0} {1} {2} {3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url, context.Request.UserHostAddress, context.Request.HttpMethod, file));
                    if (context.Request.UrlReferrer != null)
                    {
                        log.Add(string.Format("{0}UrlReferrer : {1}", space, context.Request.UrlReferrer));
                    }
                    if (context.Request.HttpMethod == "POST")
                    {
                        log.Add(string.Format("{0}POST : {1}", space, string.Join("&", post)));
                    }
                    log.Add(string.Format("{1}UserAgent : {0}", context.Request.UserAgent, space));
                    log.Add(string.Format("{1}Cookies : {0}", string.Join("&", context.Request.Cookies.AllKeys.ToList().ConvertAll(t => string.Format("{0}={1}", t, context.Request.Cookies[t].Value))), space));
                    log.Add(string.Empty);
                    File.AppendAllLines(context.Server.MapPath("~/Log/" + DateTime.Now.ToString("yyyyMMdd") + ".log"), log);
                }
            }
            if (modify != null) data = modify(data);
            context.Response.BinaryWrite(data);
        }

        /// <summary>
        /// 获取远程服务器的编码
        /// </summary>
        /// <param name="contentType">格式：text/html; charset=gb2312</param>
        /// <returns></returns>
        private static Encoding GetEncoding(string contentType)
        {
            Regex regex = new Regex("charset=(?<Code>[^;]+)", RegexOptions.IgnoreCase);
            if (regex.IsMatch(contentType))
            {
                return Encoding.GetEncoding(regex.Match(contentType).Groups["Code"].Value);
            }
            return Encoding.Default;
        }

        /// <summary>
        /// 设置返回头
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        private static void ResposeHeader(HttpContext context, HttpWebResponse response, ref Encoding encoding)
        {
            context.Response.StatusCode = (int)response.StatusCode;
            foreach (string key in response.Headers.Keys)
            {
                string value = response.Headers[key];
                switch (key)
                {
                    case "Content-Type":
                        context.Response.ContentType = value;
                        encoding = context.Response.ContentEncoding = GetEncoding(value);
                        break;
                    case "Accept-Ranges":
                    case "ETag":
                    case "Location":
                    case "Set-Cookie":
                        context.Response.AddHeader(key, value);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    /// <summary>
    /// URL重写类。使用方法：
    /// add <section name="ReverseProxy" requirePermission="false" type="SP.Studio.Net.ReverseProxySetting" /> To configSections   只允许一个 <configSections> 元素。它必须是根 <configuration> 元素的第一个子元素 
    /// 在configuration添加 <ReverseProxy> <Rule Name="站点名称"> <LookFor></LookFor><SendTo></SendTo></Rule> .... <ReverseProxy>
    /// Modules过滤模块 <httpHandlers>      <add type="SP.Studio.Net.ReverseProxyHandler" verb="*" path="*.*"/>    </httpHandlers>  To System.Web
    /// </summary>
    public class ReverseProxyHandler : IHttpHandler, IRequiresSessionState
    {
        private static List<ReverseProxyObject> UrlList;

        static ReverseProxyHandler()
        {
            UrlList = (List<ReverseProxyObject>)ConfigurationManager.GetSection("ReverseProxy");
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string domain = context.Request.Url.Authority;
            if (!UrlList.Exists(t => t.LookFor == domain)) return;
            ReverseProxyObject obj = UrlList.Find(t => t.LookFor == domain);
            string url = string.Format("http://{0}{1}", obj.SendTo, context.Request.RawUrl);
            ReverseProxy.Create(context, url);
        }
    }

    /// <summary>
    /// 从web.config里面读取的反向代理配置参数
    /// </summary>
    public class ReverseProxySetting : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<ReverseProxyObject> list = new List<ReverseProxyObject>();
            XDocument xDoc = new XDocument();
            using (XmlWriter xmlWriter = xDoc.CreateWriter())
            {
                section.WriteTo(xmlWriter);
            }

            foreach (XElement node in xDoc.Root.Elements("Rule"))
            {
                list.Add(new ReverseProxyObject(node));
            }
            return list;
        }
    }

    /// <summary>
    /// 反向代理的配置实体类
    /// </summary>
    public struct ReverseProxyObject
    {
        public ReverseProxyObject(XElement node)
        {
            this.LookFor = node.GetValue("LookFor");
            this.SendTo = node.GetValue("SendTo");
        }

        public readonly string LookFor;

        public readonly string SendTo;
    }
}
