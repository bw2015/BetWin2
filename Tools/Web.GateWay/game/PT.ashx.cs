using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Collections.Specialized;
using System.Configuration;

using SP.Studio.Web;

namespace Web.GateWay.games
{
    /// <summary>
    /// PT接口的发送
    /// </summary>
    public class PT : IHttpHandler
    {
        /// <summary>
        /// 远程网关
        /// </summary>
        private const string GATEWAY = "https://kioskpublicapi.luckydragon88.com/";

        private string domain
        {
            get
            {
                return HttpContext.Current.Request.Url.Authority;
            }
        }

        /// <summary>
        /// p12证书的物理路径
        /// </summary>
        private string certFilePath
        {
            get
            {
                return HttpContext.Current.Server.MapPath("~/App_Data/PT/VBETCNYTLE.p12");
            }
        }

        /// <summary>
        /// 证书密码
        /// </summary>
        private const string password = "iQ3xuZrS";

        /// <summary>
        /// X_ENTITY_KEY
        /// </summary>
        private const string key = "fed1a9ff3c69783af2e7fc055d4ea64e16963357de4148248586fcf31dae20795f667b5c4bd702c9a545eee56eb473404cd2af0975f21aa7478ad32ea340c088";

        public void ProcessRequest(HttpContext context)
        {
            switch (context.Request.QueryString["ac"])
            {
                // 单点登录进入
                case "game":
                    context.Response.ContentType = "text/html";
                    StringBuilder sb = new StringBuilder();
                    if (WebAgent.IsMobile())
                    {
                        sb.Append("<html><head><meta charset=\"UTF-8\" /><title>正在进入游戏</title>")
                            .Append("<script type=\"text/javascript\" src=\"https://login.greatfortune88.com/jswrapper/integration.js.php?casino=greatfortune88\"></script>")
                            .Append("<script type=\"text/javascript\" src=\"../scripts/game-pt-mobile.js\"></script>")
                            .Append("</head><body>")
                            .AppendFormat("<input type=\"hidden\" id=\"playername\" value=\"{0}\" />", WebAgent.QF("PlayerName"))
                            .AppendFormat("<input type=\"hidden\" id=\"password\" value=\"{0}\" />", WebAgent.QF("Password"))
                            .AppendFormat("<input type=\"hidden\" id=\"key\" value=\"{0}\" />", WebAgent.QF("Key"))
                            .Append("正在进入游戏...")
                            .Append("</body></html>");
                    }
                    else
                    {
                        sb.Append("<html>")
                           .Append("<head>")
                           .Append("<script type=\"text/javascript\" src=\"https://login.greatfortune88.com/jswrapper/integration.js.php?casino=greatfortune88\"></script>")
                           .Append("</head>")
                           .AppendFormat("<script type=\"text/javascript\">")
                           .Append("iapiSetCallout('Login', calloutLogin);")
                           .Append(" function login(realMode) { ")
                           .AppendFormat("iapiLogin(\"{0}\", \"{1}\", realMode, \"en\");", WebAgent.QF("PlayerName"), WebAgent.QF("Password"))
                           .Append("}")
                           .Append(" function calloutLogin(response) {")
                           .Append("var code = response.errorCode; if (code && code != 6){  alert(\"Login failed, \" + response.errorText); }else{")
                           .AppendFormat("location.href=\"http://cache.download.banner.greatfortune88.com/casinoclient.html?game={0}&language=ZH-CN&nolobby=1\";", WebAgent.QF("Key"))
                           .Append("}")
                           .Append("};")
                           .Append("</script>")
                           .Append("</head><body onload=\"login(1);\">")
                           .Append("正在进入游戏...")
                           .Append("</body></html>");
                    }
                    context.Response.Write(sb);
                    break;
                default:
                    context.Response.ContentType = "text/json";
                    List<string> list = new List<string>();
                    foreach (string key in context.Request.QueryString.AllKeys)
                    {
                        list.Add(key);
                        list.Add(context.Request.QueryString[key]);
                    }
                    string url = GATEWAY + string.Join("/", list);
                    context.Response.Write(this.DownloadData(url));
                    break;
            }
        }

        private bool InstallCertificate(StoreLocation location, StoreName storeName)
        {
            //try
            //{
            if (!File.Exists(this.certFilePath))
            {
                return false;
            }
            byte[] certData = File.ReadAllBytes(this.certFilePath);

            X509Certificate2 cert = new X509Certificate2(certData, password,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            X509Store store = new X509Store(storeName, location);
            StorePermission sp = new StorePermission(PermissionState.Unrestricted);
            sp.Flags = StorePermissionFlags.OpenStore;
            sp.Assert();
            store.Open(OpenFlags.MaxAllowed);
            store.Remove(cert);
            store.Add(cert);
            store.Close();
            return true;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message + "\ncertFilePath:" + certFilePath);
            //}
        }

        private NameValueCollection GetHttpHeader()
        {
            NameValueCollection headers = new NameValueCollection();
            headers.Add("Cache-Control", "max-age=0");
            headers.Add("Keep-Alive", "timeout=5, max=100");
            headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.3");
            headers.Add("Accept-Language", "es-ES,es;q=0.8");
            headers.Add("Pragma", "");
            headers.Add("X_ENTITY_KEY", key);

            return headers;
        }

        private string DownloadData(string url)
        {
            //try
            //{
            ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            Uri uri = new Uri(@url);
            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;

            if (!InstallCertificate(StoreLocation.CurrentUser, StoreName.My))
            {
                return string.Format("没有找到证书文件：{0}", this.certFilePath);
            }

            X509Certificate cer = new X509Certificate(this.certFilePath, password);
            request.ClientCertificates.Add(cer);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "post";
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Proxy = null;

            NameValueCollection headers = GetHttpHeader();
            request.Headers.Add(headers);
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                return reader.ReadToEnd();
            }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception(ex.Message + "\n" + url);
            //}
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