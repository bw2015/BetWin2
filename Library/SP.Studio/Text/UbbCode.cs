using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.Studio.Text
{
    public class UbbCode
    {
        /// <summary>
        /// 只换行
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConverBR(string str)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            return str.Replace("\n", "<br />");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="safe">是否完全过滤非安全自负 比如说引号、尖括号</param>
        /// <returns></returns>
        public static string NoHtml(string str, bool safe = false)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (!safe)
            {
                str = Regex.Replace(str, @"\<[^\>]+\>", @"", RegexOptions.IgnoreCase);
            }
            else
            {
                str = str.Replace("\"", "”").Replace("'", "’").Replace("<", "&lt;").Replace(">", "&gt;");
            }
            return str;
        }

        /// <summary>
        /// 去除不安全的XML字符
        /// </summary>
        public static string SafeXML(string str)
        {
            return str.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;").Replace("\"", "&quot;").Replace("'", "apos");
        }
    }
}
