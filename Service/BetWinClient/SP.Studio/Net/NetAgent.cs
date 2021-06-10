using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SP.Studio.Net
{
    internal static class NetAgent
    {
        internal static WebClient CreateWebClient(string url = null, Encoding encoding = null)
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

        internal static string DownloadData(string url, Encoding encoding, WebClient wc = null)
        {
            bool isNew = false;
            if (wc == null)
            {
                wc = CreateWebClient(url, encoding);
                isNew = true;
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
                    strResult = string.Format("Error:{0}\n<hr />\n{1}", ex.Message, reader.ReadToEnd());
                }
            }
            finally
            {
                if (!isNew) wc.Cookies();
                if (isNew) wc.Dispose();
            }
            return strResult;
        }

        internal static string UploadData(string url, string data, Encoding encoding)
        {
            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Encoding = encoding;
                    wc.Headers.Add("Accept", "*/*");
                    wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36");
                    wc.Headers.Add(HttpRequestHeader.Referer, url);
                    wc.Headers.Add("Cookie", "");

                    byte[] result = wc.UploadData(url, "POST", encoding.GetBytes(data));
                    return encoding.GetString(result);
                }
                catch (WebException ex)
                {
                    string strResult = string.Format("Error:{0}", ex.Message);
                    if (ex.Response != null)
                    {
                        StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                        if (reader != null)
                        {
                            strResult += string.Format("\n<hr />\n{0}", reader.ReadToEnd());
                        }
                    }
                    return strResult;
                }
            }
        }

        internal static byte[] UnGZip(byte[] data)
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

                wc.Headers[HttpRequestHeader.Cookie] = string.Join(";", list);
            }
        }
    }
}
