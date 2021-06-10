using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace SP.Studio.Net
{
    /// <summary>
    /// FTP操作相关的代理类
    /// </summary>
    public class FTPAgent
    {
        /// <summary>
        /// 获取FTP目录
        /// </summary>
        /// <param name="remote">ftp开头的完整路径</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static List<string> GetDirectories(string remote, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(username)) username = "anonymous";
            if (string.IsNullOrEmpty(password)) password = "janeDoe@contoso.com";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remote);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            request.Credentials = new NetworkCredential(username, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();

            List<string> list = new List<string>();

            foreach (string line in result.Split('\n').Where(t => t.StartsWith("d")))
            {
                string value = Regex.Replace(line, @"\s{1,}$", "");
                string name = !value.Contains(' ') ? null : value.Substring(value.LastIndexOf(' ') + 1);
                if (string.IsNullOrEmpty(name)) continue;
                if (name == "." || name == "..") continue;

                list.Add(name);
            }

            return list;
        }

        public static List<string> GetFiles(string remote, string username = null, string password = null)
        {
            string result;
            return GetFiles(remote, out result, username, password).Select(t => t.FileName).ToList();
        }

        public static List<FTPFileInfo> GetFiles(string remote, out string result, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(username)) username = "anonymous";
            if (string.IsNullOrEmpty(password)) password = "janeDoe@contoso.com";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remote);
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Credentials = new NetworkCredential(username, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            result = reader.ReadToEnd();
            reader.Close();
            response.Close();

            List<FTPFileInfo> list = new List<FTPFileInfo>();

            foreach (string line in result.Split('\n').Where(t => !t.StartsWith("d")))
            {
                FTPFileInfo fileInfo = new FTPFileInfo(remote, line);
                if (string.IsNullOrEmpty(fileInfo.FileName)) continue;
                list.Add(fileInfo);
            }

            return list;
        }

        /// <summary>
        /// 获取内容（文本）
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="encoding"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetContent(string remote, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(username)) username = "anonymous";
            if (string.IsNullOrEmpty(password)) password = "janeDoe@contoso.com";

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remote);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(username, password);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();

            return result;
        }
    }

    public struct FTPFileInfo
    {
        public FTPFileInfo(string path, string line)
        {
            Regex regex = new Regex(@"(?<FileSize>\d+) \w{3} \d{2} \d{2}:\d{2} (?<FileName>[a-z0-9\.]+)", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(line))
            {
                this.FileName = null;
                this.FileSize = 0;
                this.Path = null;
            }
            else
            {
                this.FileName = regex.Match(line).Groups["FileName"].Value;
                this.FileSize = int.Parse(regex.Match(line).Groups["FileSize"].Value);
                this.Path = path + (path.EndsWith("/") ? "" : "/") + this.FileName;
            }
        }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName;

        /// <summary>
        /// 文件大小
        /// </summary>
        public int FileSize;

        /// <summary>
        /// FTP路径
        /// </summary>
        public string Path;
    }
}
