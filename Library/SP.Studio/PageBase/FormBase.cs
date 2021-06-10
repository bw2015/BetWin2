using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;

using SP.Studio.Core;
using SP.Studio.Web;

namespace SP.Studio.PageBase
{
    /// <summary>
    /// 表单对象的基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FormBase<T> : Pagebase where T : class,new()
    {
        /// <summary>
        /// 显示表单
        /// </summary>
        protected virtual string ShowForm()
        {
            Type type = typeof(T);
            StringBuilder sb = new StringBuilder();
            foreach (PropertyInfo property in type.GetProperties().Where(t => t.HasAttribute(typeof(FormAttribute))))
            {
                FormAttribute att = property.GetAttribute<FormAttribute>();
                if (string.IsNullOrEmpty(att.Label)) continue;
                sb.Append("<tr>")
                    .AppendFormat("<th>{0} {1}:</th>", att.Require ? "<label class=\"require\">*</label>" : "", att.Label)
                    .Append("<td>");
                sb.Append(att.ToControl(property.Name));
                sb.Append("</td></tr>");
            }
            return sb.ToString();
        }


        /// <summary>
        /// 插入
        /// </summary>
        /// <returns></returns>
        protected virtual bool Insert()
        {
            T t = Request.Form.Fill<T>();
            return true;
        }
    }
}
