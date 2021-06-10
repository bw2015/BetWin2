using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.IO;
using System.Threading;
using System.IO.Compression;

namespace SP.Studio.IO
{
    public class ZipAgent
    {
        /// <summary>
        /// 命令行文件路径
        /// </summary>
        private static readonly string z7, rar;

        static ZipAgent()
        {
            z7 = HttpContext.Current == null ? "" : HttpContext.Current.Server.MapPath("~/Bin/7z.exe");
            rar = HttpContext.Current == null ? "" : HttpContext.Current.Server.MapPath("~/Bin/rar.exe");
        }

        /// <summary>
        /// 单个文件或者通配符压缩
        /// </summary>
        /// <param name="zipPath">ZIP文件路径</param>
        /// <param name="file">要压缩的文件 支持通配符表达</param>
        /// <param name="isDelete">压缩完成之后是否删除压缩文件</param>
        /// <returns></returns>
        public static bool Compression(string zipPath, string file, bool isDelete = false)
        {
            return Compression(zipPath, new string[] { file }, isDelete);
        }
        
        /// <summary>
        /// 压缩成zip/rar文件
        /// </summary>
        /// <param name="zipPath">zip文件路径</param>
        /// <param name="files">要压缩的文件列表 支持通配符表达</param>
        /// <param name="isDelete">压缩完成之后是否删除压缩文件</param>
        public static bool Compression(string zipPath, string[] files, bool isDelete = false)
        {
            FileInfo file = new FileInfo(zipPath);
            switch (file.Extension)
            {
                case ".zip":
                    ZIP(zipPath, files);
                    break;
                case ".rar":
                    RAR(zipPath, files);
                    break;
            }
            if (isDelete)
            {
                foreach (string f in files)
                {
                    File.Delete(f);
                }
            }
            return true;
        }

        private static void RAR(string zipPath, params string[] files)
        {
            if (string.IsNullOrEmpty(rar) || !File.Exists(rar)) return;
            Process process = new Process();
            process.StartInfo.FileName = rar;
            process.StartInfo.Arguments = string.Format("a -ep -m5 \"{0}\" {1}", zipPath, string.Join(" ", files.ToList().ConvertAll(t => "\"" + t + "\"").ToArray()));
            process.Start();
            process.WaitForExit();
        }

        private static void ZIP(string zipPath, params string[] files)
        {
            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string file in files)
                {
                    string name = file.Substring(file.LastIndexOf(@"\") + 1);
                    zip.CreateEntryFromFile(file, name);
                }
            }
        }

        /// <summary>
        /// 解压缩 支持rar和zip后缀
        /// </summary>
        /// <param name="zipPath">压缩文件的物理路径</param>
        /// <param name="savePath">要解压缩到的路径</param>
        /// <param name="zipFolderName">是否包含压缩包文件吗。 如果为false则解压缩到当前目录</param>
        /// <param name="isDelete"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool UnCompression(string zipPath, string savePath = null, bool zipFolderName = false, bool isDelete = false, string password = null)
        {
            if (!File.Exists(zipPath)) return false;
            if (!Directory.Exists(savePath)) return false;

            zipPath = zipPath.Replace('/', '\\');
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = zipPath.Substring(0, zipPath.LastIndexOf('\\'));
            }
            savePath = savePath.Replace('/', '\\');

            string name = zipPath.Substring(zipPath.LastIndexOf('\\') + 1);
            if (name.Contains('.')) name = name.Substring(0, name.LastIndexOf('.'));

            if (zipFolderName)
            {
                savePath += "\\" + name;
                if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
            }

            string agruments = null;
            string command = null;

            string extName = zipPath.Substring(zipPath.LastIndexOf('.') + 1).ToLower();
            switch (extName)
            {
                case "zip":
                    agruments = UnZIP(zipPath, savePath, null, out command);
                    break;
                case "rar":
                    agruments = UnRAR(zipPath, savePath, null, out command);
                    break;
            }


            Process p = new Process();
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = agruments;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();

            if (isDelete) File.Delete(zipPath);
            return true;
        }

        /// <summary>
        /// 解压缩文件(RAR)
        /// </summary>
        /// <param name="zipPath">压缩包的位置</param>
        /// <param name="savePath">要解压缩到的位置</param>
        /// <param name="zipFolderName">是否创建于压缩包同名的文件目录</param>
        /// <param name="isDelete">解压完成是否删除压缩包</param>
        /// <returns></returns>
        private static string UnRAR(string zipPath, string savePath, string password, out string command)
        {
            command = rar;
            return string.Format(" x -y -o {2} \"{0}\" \"{1}\"", zipPath, savePath, string.IsNullOrEmpty(password) ? "" : string.Format("-p\"{0}\"", password));
        }

        /// <summary>
        /// 解压缩ZIP格式
        /// </summary>
        private static string UnZIP(string zipPath, string savePath, string password, out string command)
        {
            command = z7;
            return string.Format(" e -y \"{0}\" -o\"{1}\"", zipPath, savePath);
        }
    }
}
