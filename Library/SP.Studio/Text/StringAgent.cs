using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Core;
using SP.Studio.Web;

namespace SP.Studio.Text
{
    /// <summary>
    /// 字符串的扩展处理
    /// </summary>
    public static class StringAgent
    {
        /// <summary>
        /// 获取一个字符串指定标签的内容
        /// </summary>
        /// <param name="str">要搜寻的字符串</param>
        /// <param name="beforeTag">开头的标签</param>
        /// <param name="endTag">结尾的标签</param>
        /// <param name="hasTag">返回结果是否包含标签</param>
        /// <returns></returns>
        public static string[] GetStringValue(string str, string beforeTag, string endTag, bool hasTag = false)
        {
            if (string.IsNullOrEmpty(str)) return new string[] { };

            List<string> list = new List<string>();
            int begin = 0;
            int end = 0;
            while (true)
            {
                begin = str.IndexOf(beforeTag);
                if (begin == -1) break;
                begin += beforeTag.Length;
                str = str.Substring(begin);
                end = str.IndexOf(endTag);
                if (end == -1) break;
                list.Add((hasTag ? beforeTag : "") + str.Substring(0, end) + (hasTag ? endTag : ""));
            }
            return list.ToArray();
        }

        /// <summary>
        /// 获取指定标签内的内容
        /// </summary>
        /// <param name="str">内容</param>
        /// <param name="beforeTag">开始标签</param>
        /// <param name="endTag">结尾标签</param>
        /// <returns>有用信息</returns>
        public static string GetString(string str, string beforeTag, string endTag)
        {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            int begin = str.IndexOf(beforeTag);
            if (begin == -1) return null;
            begin += beforeTag.Length;
            str = str.Substring(begin);
            int end = str.IndexOf(endTag);
            if (end == -1) return null;
            return str.Substring(0, end);
        }

        /// <summary>
        /// 使用正则表达式查找指定的值，如果不符合规则则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="regex">输出第一个组的ID</param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static T GetString<T>(string input, string pattern, T def)
        {
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (!regex.IsMatch(input)) return def;
            Match match = regex.Match(input);
            string value = match.Groups[1].Value;
            if (!WebAgent.IsType<T>(value)) return def;
            return value.GetValue<T>();
        }

        /// <summary>
        /// 过滤所有的HTML字符
        /// </summary>
        public static string NoHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            Regex regex = new Regex(@"\<[^\>]+\>|\&[a-z]+\;");
            return regex.Replace(html, "").Replace("\n", "").Replace("\r", "");
        }

        /// <summary>
        /// 转换成为html的换行
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ConverBR(string html)
        {
            return html.Replace("\n", "<br />");
        }


    }
}
