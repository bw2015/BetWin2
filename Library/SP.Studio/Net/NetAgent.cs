using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SP.Studio.Net
{
    /// <summary>
    /// 网络代理类
    /// </summary>
    public static class NetAgent
    {
        public static WebClient CreateWebClient(string url = null, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            WebClient wc = new WebClient();
            wc.Encoding = encoding;
            wc.Headers.Add("Accept", "*/*");
            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains('?')) url = url.Substring(0, url.IndexOf('?'));
                wc.Headers.Add("Referer", url);
            }
            wc.Headers.Add("Cookie", "");
            wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/27.0.1453.12 Safari/537.36");
            return wc;
        }

        /// <summary>
        /// 下载小文件
        /// </summary>
        /// <param name="savePath">物理路径</param>
        public static int DownloadFile(string url, string savePath, WebClient wc = null)
        {
            int fileSize = 0;
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url);
                isNew = true;
            }
            try
            {
                FileAgent.CreateDirectory(savePath, true);
                wc.DownloadFile(url, savePath);
                FileInfo f = new FileInfo(savePath);
                fileSize = (int)f.Length;
            }
            catch
            {
                fileSize = 0;
            }
            finally
            {
                if (!isNew) wc.Cookies();
                if (isNew) wc.Dispose();
            }
            return fileSize;
        }


        /// <summary>
        /// 下载大文件
        /// </summary>
        /// <param name="url">远程URL</param>
        /// <param name="filename">本地路径</param>
        /// <param name="progressName">变量的名字</param>
        public static long DownloadFile(string url, string filename, DownloadProgress progress)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            long totalBytes = response.ContentLength;
            long totalDownloadedByte = 0;
            Stream st = response.GetResponseStream();
            Stream so = new FileStream(filename, FileMode.Create);
            byte[] data = new byte[1024];
            int osize = st.Read(data, 0, data.Length);
            progress.Total = totalBytes;
            while (osize > 0)
            {
                totalDownloadedByte += osize;
                so.Write(data, 0, osize);
                osize = st.Read(data, 0, data.Length);
                progress.Downloaded = totalDownloadedByte;
            }
            so.Close();
            st.Close();

            return totalBytes;
        }


        public static byte[] DownloadFile(string url, WebClient wc = null)
        {
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url);
                isNew = true;
            }
            try
            {
                return wc.DownloadData(url);
            }
            finally
            {
                if (!isNew) wc.Cookies();
                if (isNew) wc.Dispose();
            }
        }

        public static string DownloadData(string url, Encoding encoding = null, WebClient wc = null)
        {
            return DownloadData(url, encoding, null, wc);
        }

        /// <summary>
        /// 获取远程信息
        /// </summary>
        public static string DownloadData(string url, Encoding encoding, Dictionary<string, string> header, WebClient wc = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url, encoding);
                isNew = true;
            }
            if (header != null)
            {
                foreach (KeyValuePair<string, string> item in header)
                {
                    wc.Headers[item.Key] = item.Value;
                }
            }
            string strResult = null;
            try
            {
                byte[] data = wc.DownloadData(url);
                if (wc.ResponseHeaders[HttpResponseHeader.ContentEncoding] == "gzip")
                {
                    data = UnGZip(data);
                }
                strResult = encoding.GetString(data);
            }
            catch (WebException ex)
            {
                if (ex.Response == null)
                {
                    strResult = string.Format("Error:{0}", ex.Message);
                }
                else
                {
                    StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), encoding);
                    strResult = reader.ReadToEnd();
                }
            }
            finally
            {
                if (!isNew) wc.Cookies();
                if (isNew) wc.Dispose();
            }
            return strResult;
        }

        private static bool InstallCertificate(string certFilePath, string password, StoreLocation location, StoreName storeName)
        {
            try
            {
                if (!File.Exists(certFilePath))
                {
                    return false;
                }
                byte[] certData = File.ReadAllBytes(certFilePath);
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
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前用户浏览器的header投设置
        /// </summary>
        /// <returns></returns>
        private static NameValueCollection GetHttpHeader()
        {
            NameValueCollection headers = new NameValueCollection();

            if (HttpContext.Current != null)
            {
                foreach (string key in HttpContext.Current.Request.Headers.AllKeys)
                {
                    headers.Add(key, HttpContext.Current.Request.Headers[key]);
                }
            }
            else
            {
                headers.Add("Cache-Control", "max-age=0");
                headers.Add("Keep-Alive", "timeout=5, max=100");
                headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.3");
                headers.Add("Accept-Language", "es-ES,es;q=0.8");
                headers.Add("Pragma", "");
                headers.Add("X_ENTITY_KEY", "232132132132132131232104238432749832749832174983217");
            }
            return headers;
        }


        /// <summary>
        /// 使用证书获取远程网页信息
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <param name="encoding">使用的编码</param>
        /// <param name="clientP12_Path">P12证书的本地文件</param>
        /// <param name="clientP12PassWord">P12证书的密码</param>
        /// <returns></returns>
        public static string DownloadData(string url, Encoding encoding, string clientP12_Path, string clientP12PassWord)
        {
            ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            Uri uri = new Uri(@url);
            HttpWebRequest request = HttpWebRequest.Create(uri) as HttpWebRequest;

            InstallCertificate(clientP12_Path, clientP12PassWord, StoreLocation.CurrentUser, StoreName.My);

            X509Certificate cer = new X509Certificate(clientP12_Path, clientP12PassWord);
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
        }

        /// <summary>
        /// Post信息
        /// </summary>
        /// <param name="data">上传内容</param>
        public static string UploadData(string url, string data, Encoding encoding = null, WebClient wc = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url);
                isNew = true;
            }
            string strResult = null;
            try
            {
                if (string.IsNullOrEmpty(wc.Headers[HttpRequestHeader.ContentType]))
                {
                    wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                }
                if (string.IsNullOrEmpty(wc.Headers[HttpRequestHeader.UserAgent]))
                {
                    wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (iPhone; CPU iPhone OS 5_1 like Mac OS X) AppleWebKit/534.46 (KHTML, like Gecko) Mobile/9B176 MicroMessenger/4.3.2";
                }
                byte[] dataResult = wc.UploadData(url, "POST", encoding.GetBytes(data));
                strResult = encoding.GetString(dataResult);
                wc.Headers.Remove("Content-Type");
            }
            catch (WebException ex)
            {
                strResult = string.Format("Error:{0}", ex.Message);
                if (ex.Response != null)
                {
                    StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                    if (reader != null)
                    {
                        strResult = reader.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (!isNew) wc.Cookies();
                if (isNew) wc.Dispose();
            }

            return strResult;
        }

        /// <summary>
        /// 异步提交信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="uploadComplete"></param>
        /// <param name="encoding"></param>
        /// <param name="wc"></param>
        public static async void UploadDataSync(string url, string data, UploadDataCompletedEventHandler uploadComplete = null, Encoding encoding = null, WebClient wc = null)
        {
            if (encoding == null) encoding = Encoding.Default;
            if (wc == null)
            {
                wc = CreateWebClient(url);
            }
            wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
            if (uploadComplete != null)
                wc.UploadDataCompleted += uploadComplete;
            await Task.Run(() => wc.UploadDataAsync(new Uri(url), "POST", encoding.GetBytes(data)));
        }


        /// <summary>
        /// 把服務器返回的Cookies信息寫入到客戶端中
        /// </summary>
        public static void Cookies(this WebClient wc)
        {
            if (wc.ResponseHeaders == null) return;
            string setcookie = wc.ResponseHeaders[HttpResponseHeader.SetCookie];
            if (!string.IsNullOrEmpty(setcookie))
            {
                string cookie = wc.Headers[HttpRequestHeader.Cookie];
                Dictionary<string, string> cookieList = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(cookie))
                {
                    foreach (string ck in cookie.Split(';'))
                    {
                        string key = ck.Substring(0, ck.IndexOf('='));
                        string value = ck.Substring(ck.IndexOf('=') + 1);
                        if (!cookieList.ContainsKey(key)) cookieList.Add(key, value);
                    }
                }

                foreach (string ck in setcookie.Split(';'))
                {
                    string str = ck;
                    while (str.Contains(',') && str.IndexOf(',') < str.LastIndexOf('='))
                    {
                        str = str.Substring(str.IndexOf(',') + 1);
                    }
                    string key = str.IndexOf('=') != -1 ? str.Substring(0, str.IndexOf('=')) : "";
                    string value = str.Substring(str.IndexOf('=') + 1);
                    if (!cookieList.ContainsKey(key))
                        cookieList.Add(key, value);
                    else
                        cookieList[key] = value;
                }

                string[] list = new string[cookieList.Count()];
                int index = 0;
                foreach (var pair in cookieList)
                {
                    list[index] = string.Format("{0}={1}", pair.Key, pair.Value);
                    index++;
                }

                wc.Headers[HttpRequestHeader.Cookie] = list.Join(';');
            }
        }

        /// <summary>
        /// 获取网页内容
        /// </summary>
        public static string GetWebContent(string url, Encoding encoding)
        {
            string content = url;
            try
            {
                using (WebClient web = new WebClient())
                {
                    web.Headers.Add("Accept", "*/*");
                    web.Headers.Add("Referer", url);
                    web.Headers.Add("User-Agent", typeof(NetAgent).Assembly.FullName);
                    // web.Headers.Add("Authorization", "OAuth2" + token);
                    Byte[] page = web.DownloadData(url);
                    content = encoding.GetString(page);
                }
            }
            catch (System.Exception ex)
            {
                content = "Error:" + ex.Message;
            }
            return content;
        }

        /// <summary>
        /// 使用自定义header头的get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        public static string GetWebContent(string url, Encoding encoding, Dictionary<string, string> header)
        {
            using (WebClient wc = new WebClient())
            {
                foreach (KeyValuePair<string, string> item in header)
                {
                    wc.Headers.Add(item.Key, item.Value);
                }
                return DownloadData(url, encoding, wc);
            }
        }

        /// <summary>
        /// QQ是否在线
        /// </summary>
        public static bool QQOnline(string qq)
        {
            string key = "QQ" + qq;
            if (HttpRuntime.Cache[key] != null) return (bool)HttpRuntime.Cache[key];
            bool isOnline = NetAgent.DownloadFile(string.Concat("http://wpa.qq.com/pa?p=1:", qq, ":1")).Length == 2239;
            HttpRuntime.Cache.Insert(key, isOnline, null, DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
            return isOnline;
        }

        public static HttpStatusCode GetHttpCode(string url)
        {
            WebHeaderCollection header;
            return GetHttpCode(url, out header);
        }

        /// <summary>
        /// 获取URL的返回的状态值
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpStatusCode GetHttpCode(string url, out WebHeaderCollection header)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.AllowAutoRedirect = false;
            HttpWebResponse response = null;
            header = null;
            HttpStatusCode code;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                code = response.StatusCode;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    try
                    {
                        response = (HttpWebResponse)ex.Response;
                        header = response.Headers;
                        code = response.StatusCode;
                    }
                    catch
                    {
                        code = HttpStatusCode.SeeOther;
                    }
                }
                else
                {
                    code = HttpStatusCode.SeeOther;
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return code;
        }

        /// <summary>
        /// 获取内容并且返回状态码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="code"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetHttpContent(string url, out HttpStatusCode code, Encoding encoding)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowAutoRedirect = false;
            HttpWebResponse response = null;
            string result = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                code = response.StatusCode;
                result = new StreamReader(response.GetResponseStream(), encoding).ReadToEnd();
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    try
                    {
                        response = (HttpWebResponse)ex.Response;
                        code = response.StatusCode;
                        result = new StreamReader(response.GetResponseStream(), encoding).ReadToEnd();
                    }
                    catch (Exception e)
                    {
                        code = HttpStatusCode.SeeOther;
                        result = e.Message;
                    }
                }
                else
                {
                    code = HttpStatusCode.SeeOther;
                }
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return result;
        }

        /// <summary>
        /// 使用HttpWebRequest上传二进制数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="header"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string UploadData(string url, Encoding encoding, Dictionary<string, string> headers, byte[] data)
        {
            HttpWebRequest request = GetHttpWebRequest(url, "POST", headers, data);

            using (HttpWebResponse response = GetHttpWebResponse(request))
            {
                StreamReader stream = new StreamReader(response.GetResponseStream(), encoding);
                return stream.ReadToEnd();
            }
        }

        public static string UploadData(string url, Encoding encoding, Dictionary<string, string> headers, string postData)
        {
            byte[] data = encoding.GetBytes(postData);
            HttpWebRequest request = GetHttpWebRequest(url, "POST", headers, data);

            using (HttpWebResponse response = GetHttpWebResponse(request))
            {
                StreamReader stream = new StreamReader(response.GetResponseStream(), encoding);
                return stream.ReadToEnd();
            }
        }

        /// <summary>
        /// 创建一个HttpRequest对象
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpWebRequest GetHttpWebRequest(string url, string method, Dictionary<string, string> headers, byte[] data)
        {
            HttpWebRequest request;
            if (url.Contains("https://"))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true);
                request = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                request = (HttpWebRequest)WebRequest.Create(url);
            }

            request.AllowAutoRedirect = false;
            request.Method = method;
            request.KeepAlive = true;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            if (headers.ContainsKey("Accept"))
            {
                request.Accept = headers.Pop("Accept", string.Empty);
            }
            if (headers.ContainsKey("Date"))
            {
                request.Date = Convert.ToDateTime(headers.Pop("Date", DateTime.Now.ToString()));
            }
            if (headers.ContainsKey("Content-Type"))
            {
                request.ContentType = headers.Pop("Content-Type", string.Empty);
            }
            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            return request;
        }

        /// <summary>
        /// 获取一个web返回对象
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpWebResponse GetHttpWebResponse(HttpWebRequest request)
        {
            HttpWebResponse httpResponse = null;
            try
            {
                WebResponse response = request.GetResponse();
                httpResponse = (HttpWebResponse)response;

            }
            catch (WebException ex)
            {
                httpResponse = (HttpWebResponse)ex.Response;
            }
            return httpResponse;
        }

        /// <summary>
        /// 进行gzip的解压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] UnGZip(byte[] data)
        {
            using (MemoryStream dms = new MemoryStream())
            {
                using (MemoryStream cms = new MemoryStream(data))
                {
                    using (System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream(cms, System.IO.Compression.CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while ((len = gzip.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            dms.Write(bytes, 0, len);
                        }
                    }
                }
                return dms.ToArray();
            }
        }

        /// <summary>
        /// 通过反射获取HttpWebResponse实例
        /// </summary>
        /// <param name="wc"></param>
        /// <returns></returns>
        public static HttpWebResponse GetHttpWebResponse(this WebClient wc)
        {
            //System.Net.WebResponse m_WebResponse
            MemberInfo[] member = typeof(WebClient).GetMember("m_WebResponse", BindingFlags.Instance | BindingFlags.NonPublic);
            if (member == null || member.Length == 0) return null;
            FieldInfo memberInfo = (FieldInfo)member[0];
            return (HttpWebResponse)memberInfo.GetValue(wc);

        }

    }

    /// <summary>
    /// 下载的进度条对象
    /// </summary>
    public class DownloadProgress
    {
        public static readonly DownloadProgress Progress = new DownloadProgress();

        /// <summary>
        /// 总共的大小
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// 下载的大小
        /// </summary>
        public long Downloaded { get; set; }

        /// <summary>
        /// 已经下载的进度
        /// </summary>
        public float percent
        {
            get
            {
                if (Total == 0) return 0.00F;
                return (float)Downloaded / (float)Total;
            }
        }
    }
}
