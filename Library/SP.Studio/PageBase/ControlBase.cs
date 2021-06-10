using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Web;

namespace SP.Studio.PageBase
{
    /// <summary>
    /// 用户控件的基类
    /// </summary>
    public abstract class ControlBase : System.Web.UI.UserControl
    {
        /// <summary>
        /// 是否选中当前页面
        /// </summary>
        /// <param name="url">当前页面地址（支持正则表达式）</param>
        /// <returns></returns>
        protected virtual bool IsCurrent(string page)
        {
            string url = Request.RawUrl;
            return Regex.IsMatch(url, page, RegexOptions.IgnoreCase);
        }

        protected virtual string QS(string key)
        {
            return WebAgent.QS(key);
        }

        protected virtual T QS<T>(string key, T defaultValue)
        {
            return WebAgent.GetParam(key, defaultValue);
        }

        protected virtual T Show<T>(T? value) where T : struct
        {
            return value == null ? default(T) : value.Value;
        }
    }
}
