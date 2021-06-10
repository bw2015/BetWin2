using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Text
{
    internal static class StringAgent
    { /// <summary>
        /// 获取一个字符串指定标签的内容
        /// </summary>
        /// <param name="str">要搜寻的字符串</param>
        /// <param name="beforeTag">开头的标签</param>
        /// <param name="endTag">结尾的标签</param>
        /// <param name="hasTag">返回结果是否包含标签</param>
        /// <returns></returns>
        internal static string[] GetStringValue(string str, string beforeTag, string endTag, bool hasTag = false)
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
        internal static string GetString(string str, string beforeTag, string endTag)
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
    }
}
