using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data.Linq;

using SP.Studio.Core;
using SP.Studio.Web;

namespace SP.Studio.PageBase
{
    /// <summary>
    /// 页面基类
    /// </summary>
    public abstract class Pagebase : Page
    {
        /// <summary>
        /// 页面内容输出
        /// </summary>
        protected StringBuilder sb = new StringBuilder();

        protected virtual string QS(string key)
        {
            return WebAgent.QS(key);
        }

        /// <summary>
        /// 过滤SQL注入字符
        /// Query String Safe
        /// </summary>
        protected virtual string QSS(string key)
        {
            return HttpUtility.HtmlEncode(QS(key));
        }

        protected string QF(string key)
        {
            return HttpContext.Current.Request.Form[key] ?? string.Empty;
        }


        /// <summary>
        /// RequestFormSafe
        /// </summary>
        protected string QFS(string key)
        {
            return HttpUtility.HtmlEncode(QF(key));
        }


        protected virtual int QS(string key, int def)
        {
            int i;
            if (int.TryParse(QS(key), out i))
            {
                return i;
            }
            else
            {
                return def;
            }
        }

        protected virtual string QS(string key, string def)
        {
            if (string.IsNullOrEmpty(QS(key))) return def;
            return QS(key);
        }

        protected virtual T QS<T>(string key, T def)
        {
            if (Web.WebAgent.IsType<T>(QS(key)))
                return (T)QS(key).GetValue(typeof(T));
            else
                return def;
        }

        protected virtual int QF(string key, int def)
        {
            int i;
            if (int.TryParse(QF(key), out i))
            {
                return i;
            }
            else
            {
                return def;
            }
        }

        protected virtual T QF<T>(string key, T def)
        {
            if (HttpContext.Current.Request.Form.AllKeys.Select(t => t.ToLower()).Contains(key.ToLower()) && WebAgent.IsType<T>(QF(key)))
            {
                return (T)QF(key).GetValue(typeof(T));
            }
            return def;
        }

        protected override void OnPreLoad(EventArgs e)
        {
            if (Page.IsPostBack)
            {
                Validate();
            }
            base.OnPreLoad(e);
        }

        protected Control FindControl(string id, Control parent = null)
        {
            if (parent == null) parent = Page;
            Control control = parent.FindControl(id);
            if (control != null) return control;
            foreach (Control con in parent.Controls)
            {
                control = this.FindControl(id, con);
                if (control != null) return control;
            }
            return null;
        }

        /// <summary>
        /// Linq的数据库操作对象
        /// </summary>
        protected T DC<T>() where T : DataContext, new()
        {
            return SP.Studio.Data.DbSetting.GetSetting().CreateDataContext<T>();
        }

        /// <summary>
        /// 显示列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action"></param>
        protected virtual void Show<T>(IEnumerable<T> list, Func<T, string> action)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in list)
            {
                sb.Append(action(t));
            }
            Response.Write(sb);
        }

        protected virtual T Show<T>(T? value) where T : struct
        {
            return value == null ? default(T) : value.Value;
        }
    }


}
