using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Runtime.InteropServices;

using System.IO;

namespace SP.Studio.IO
{
    /// <summary>
    /// 文件操作
    /// </summary>
    public static class FileAgent
    {
        /// <summary>
        /// 创建文件夹
        /// </summary>
        public static void CreateDirectory(string path, bool isIncludeFile)
        {
            path = MapPath(path.Replace('/', '\\'));
            if (isIncludeFile)
            {
                path = path.Substring(0, path.LastIndexOf('\\'));
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 在指定的文件夹后创建子目录
        /// </summary>
        /// <param name="path">根目录</param>
        /// <param name="format">子目录</param>
        public static string CreateDirectory(string path, string format = "yyyy/MM/dd")
        {
            string folder = DateTime.Now.ToString(format);
            if (format.Contains('/') && !folder.Contains('/')) folder = folder.Replace('-', '/');
            path = MapPath(path);
            if (!Directory.Exists(path)) throw new Exception("创建子目录时失败。 无法找到根目录：" + path);
            path += "\\" + folder;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return folder;
        }

        /// <summary>
        /// 返回與虛擬路徑相對的物理路徑
        /// </summary>
        public static string MapPath(string path)
        {
            if (!Regex.IsMatch(path, @"^[a-z]\:", RegexOptions.IgnoreCase))
            {
                if (HttpContext.Current != null) path = HttpContext.Current.Server.MapPath(path);
            }

            return path;
        }

        /// <summary>
        /// 創建一個隨機的文件名
        /// </summary>
        public static string CreateRandomFileName(string extName)
        {
            return string.Format("{0}{1}.{2}", DateTime.Now.ToString("HHmmss"), Guid.NewGuid().ToString("N").Substring(0, 6), extName);
        }

        /// <summary>
        /// 保存文件流到到文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="newFile">文件路径</param>
        public static void SaveStreamToFile(Stream stream, string newFile)
        {
            if (stream == null || stream.Length == 0 || string.IsNullOrEmpty(newFile)) { return; }
            byte[] buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buffer, 0, buffer.Length);
            CreateDirectory(newFile, true);
            FileStream fileStream = new FileStream(newFile, FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                fileStream.Write(buffer, 0, buffer.Length);
                fileStream.Flush();
            }
            finally
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }


        /// <summary>
        /// 读取文本
        /// </summary>
        public static string ReadText(string filePath, Encoding encoding = null)
        {
            if (!File.Exists(filePath)) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, encoding);
            string text = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            return text;

        }

        /// <summary>
        /// 写入文本
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        /// <param name="append"></param>
        public static void Write(string filePath, string text, Encoding encoding = null, bool append = true)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            using (StreamWriter sw = new StreamWriter(filePath, append, encoding))
            {
                sw.WriteLine(text);
            }
        }

        /// <summary>
        /// 获取目录下的所有文件
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static List<string> GetFiles(string directory)
        {
            if (!Directory.Exists(directory)) return null;
            List<string> list = new List<string>();
            GetFiles(directory, ref list);
            return list;
        }

        private static void GetFiles(string directory, ref List<string> list)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                list.Add(file);
            }
            foreach (string folder in Directory.GetDirectories(directory))
            {
                GetFiles(folder, ref list);
            }
        }


        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static int GetFileSize(string file)
        {
            if (!File.Exists(file)) return 0;
            return (int)new FileInfo(file).Length;
        }

        /// <summary>
        /// 获取一个文件的MD5值
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            string result = null;
            try
            {
                System.Security.Cryptography.MD5 md5;
                byte[] retVal;
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {
                    md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    retVal = md5.ComputeHash(file);
                    file.Close();
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                result = sb.ToString();
            }
            catch
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// 从二进制流获取MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string GetMD5(byte[] buffer)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = md5.ComputeHash(buffer);
            return string.Join(string.Empty, data.Select(t => t.ToString("x2")));
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        /// <summary>
        /// 检查文件是否被占用
        /// </summary>
        public static bool IsUseFile(string filepath)
        {
            if (!File.Exists(filepath)) return false;
            IntPtr vHandle = _lopen(filepath, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == HFILE_ERROR) return true;
            CloseHandle(vHandle);
            return false;
        }

    }
}
