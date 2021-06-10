using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.IO
{
    /// <summary>
    /// 文件擴展類
    /// </summary>
    public static class FileExtension
    {
        /// <summary>
        /// 通过文件名获取扩展名
        /// </summary>
        public static string GetExtension(this string fileName)
        {
            if (fileName.Contains('/')) fileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            if (fileName.Contains('\\')) fileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
            if (!fileName.Contains('.')) return null;
            fileName = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            if (fileName.Contains('?')) fileName = fileName.Substring(0, fileName.IndexOf('?'));
            return fileName;

        }
    }
}
