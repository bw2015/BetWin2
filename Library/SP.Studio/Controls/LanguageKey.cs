using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

namespace SP.Studio.Controls
{
    /// <summary>
    /// 控件的语言包
    /// </summary>
    public enum LanguageKey
    {
        /// <summary>
        /// 首页
        /// </summary>
        First,
        /// <summary>
        /// 上一页
        /// </summary>
        Previous,
        /// <summary>
        /// 下一页
        /// </summary>
        Next,
        /// <summary>
        /// 尾页
        /// </summary>
        Last,
        /// <summary>
        /// 共有{0}条记录
        /// </summary>
        Records,
        /// <summary>
        /// 页次
        /// </summary>
        PageIndex,
        /// <summary>
        /// 跳转至
        /// </summary>
        Jump
    }

    
    /// <summary>
    /// 选择的语言
    /// </summary>
    public enum LanguageType
    {
        /// <summary>
        /// 中文
        /// </summary>
        CN,
        /// <summary>
        /// 繁体中文
        /// </summary>
        TW,
        /// <summary>
        /// 英文
        /// </summary>
        EN
    }

    /// <summary>
    /// 语言设定
    /// </summary>
    internal class Language
    {
        static Dictionary<string, string> lang;

        /// <summary>
        /// 静态构造
        /// </summary>
        static Language()
        {
            lang = new Dictionary<string, string>();

            ResourceManager rm = new ResourceManager(typeof(Files.Language));
            foreach (string language in Enum.GetNames(typeof(LanguageType)))
            {
                foreach (string key in Enum.GetNames(typeof(LanguageKey)))
                {
                    string name = string.Format("Controls_{0}_{1}", key, language);
                    lang.Add(name, rm.GetString(name));
                }
            }
        }

        internal static string Get(LanguageKey key, LanguageType language = LanguageType.CN)
        {
            string name = string.Format("Controls_{0}_{1}", key, language);
            return (string)lang[name];
        }
    }

}
