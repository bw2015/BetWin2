using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Linq.Mapping;
using System.Reflection;
using SP.Studio.Web;

namespace SP.Studio.Core
{
    /// <summary>
    /// 类型
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// 获取Lambda树所对应的属性 新
        /// </summary>
        public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> fun)
        {
            Expression exp = fun.Body;
            _memberExpression = null;
            GetMemberExpression(exp);
            if (_memberExpression == null) return null;
            return (PropertyInfo)_memberExpression.Member;
        }

        public static PropertyInfo GetPropertyInfo<T, P>(this Expression<Func<T, P>> fun)
        {
            Expression exp = fun.Body;
            _memberExpression = null;
            GetMemberExpression(exp);
            if (_memberExpression == null) return null;
            return (PropertyInfo)_memberExpression.Member;
        }

        [ThreadStatic]
        private static MemberExpression _memberExpression;

        private static void GetMemberExpression(Expression exp)
        {
            switch (exp.NodeType)
            {
                case ExpressionType.Convert:
                    GetMemberExpression(((UnaryExpression)exp).Operand);
                    break;
                case ExpressionType.MemberAccess:
                    _memberExpression = (MemberExpression)exp;
                    break;
                case ExpressionType.Call:
                    var obj = ((MethodCallExpression)exp).Object;
                    if (obj == null || obj.NodeType == ExpressionType.Constant)
                    {
                        foreach (var arg in ((MethodCallExpression)exp).Arguments)
                        {
                            GetMemberExpression(arg);
                        }
                    }
                    else
                    {
                        GetMemberExpression(obj);
                    }
                    break;
            }
        }



        /// <summary>
        /// 把一个对象转化成为WebForm
        /// </summary>
        /// <param name="filter">过滤条件</param>
        /// <returns></returns>
        [Obsolete("采用SP.Studio.Web命名空间下")]
        public static string ToFrom<T>(this T t, string className = "class=\"post\"", Expression<Func<Object, bool>> filter = null)
        {
            if (filter == null) filter = obj => true;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<table {0}>", className);
            //foreach (var property in t.GetType().GetProperties().Where(p => p.CanWrite))
            //{
            //    FormAttribute form = property.GetAttribute<FormAttribute>() ?? new FormAttribute();
            //    string value = string.IsNullOrEmpty(form.Format) ? property.GetValue(t, null).ToString() : ((dynamic)property.GetValue(t, null)).ToString(form.Format);
            //    if (string.IsNullOrEmpty(form.Name)) form.Name = property.Name;
            //    if (property.PropertyType.IsEnum)
            //    {
            //        form.Input = property.PropertyType.ToDropDownList(EnumDropDownType.Text, "name=\"${name}\" class=\"txt\"", value);
            //    }
            //    sb.Append("<tr>");
            //    sb.AppendFormat("<th>{0}:</th>", form.Name);
            //    sb.AppendFormat("<td>{0}</td>", form.Input.Replace("${name}", property.Name).Replace("${value}", value));
            //    sb.Append("</tr>");
            //}
            sb.Append("</table>");
            return sb.ToString();
        }

    }


}
