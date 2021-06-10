using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SP.Studio.Text
{
    /// <summary>
    /// 字符串的扩展
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// 首字符大写
        /// </summary>
        public static string ToTitleCase(this string text)
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        /// <summary>
        /// 忽略大小写检查是否包含
        /// </summary>
        public static bool ContainsCase(this string text, string find, bool ignoreCase)
        {
            if (text == null)
                return false;
            var comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            var endIndex = text.IndexOf(find, 0, comparison);
            return endIndex >= 0;
        }

        /// <summary>
        /// 忽略大小写的替换
        /// </summary>
        public static string ReplaceCase(this string text, string find, string replace, bool ignoreCase)
        {
            var result = new StringBuilder();
            var comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            for (int index = 0; ; )
            {
                var endIndex = text.IndexOf(find, index, comparison);
                if (endIndex >= 0)
                {
                    result.Append(text.Substring(index, endIndex - index));
                    result.Append(replace);
                    index = endIndex + find.Length;
                }
                else
                {
                    result.Append(text.Substring(index));
                    break;
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// 替换使用对象替换模板数据
        /// </summary>
        public static string ToContent(this string template, params object[] objs)
        {
            foreach (object obj in objs)
            {
                if (obj == null) continue;
                foreach (var property in obj.GetType().GetProperties())
                {
                    string key = "${" + property.Name + "}";
                    if (template.IndexOf(key) == -1) continue;
                    string value = (property.GetValue(obj, null) ?? string.Empty).ToString();
                    template = template.Replace(key, value);
                }
            }
            return template;
        }

        /// <summary>
        /// 获取键值对字符串里面的值
        /// </summary>
        public static string Get(this string query, string key)
        {
            Regex regex = new Regex(@"(?<Key>[^\&]+)=(?<Value>[^\&]+)", RegexOptions.IgnoreCase);
            List<string> value = new List<string>();
            foreach (Match match in regex.Matches(query))
            {
                if (match.Groups["Key"].Value.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                {
                    value.Add(match.Groups["Value"].Value);
                }
            }
            if (value.Count == 0) return null;
            return string.Join(",", value);
        }

        /// <summary>
        /// 在左侧增加字符串
        /// </summary>
        /// <param name="str">原始字符</param>
        /// <param name="length">添加的数量</param>
        /// <param name="padding">添加的字符串</param>
        /// <returns></returns>
        public static string PadLeft(this string str, int length, string paddingString)
        {
            return string.Concat(string.Join(string.Empty, paddingString.RepeaterPadding(length)), str);
        }

        /// <summary>
        /// 在右侧增加字符串
        /// </summary>
        /// <param name="str">原始字符</param>
        /// <param name="length">添加的数量</param>
        /// <param name="padding">添加的字符串</param>
        /// <returns></returns>
        public static string PadRight(this string str, int length, string paddingString)
        {
            return string.Concat(str, string.Join(string.Empty, paddingString.RepeaterPadding(length)));
        }

        /// <summary>
        /// 把同一个字符串重复生成
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <param name="paddingString"></param>
        /// <param name="place"></param>
        /// <returns></returns>
        public static IEnumerable<string> RepeaterPadding(this string paddingString, int length)
        {
            for (var i = 0; i < length; i++)
                yield return paddingString;
        }

        public static string RepeaterPadding(this string paddingString, int length, string separator)
        {
            return string.Join(separator, paddingString.RepeaterPadding(length));
        }
    }

}
