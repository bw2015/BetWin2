using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;

using SP.Studio.Array;
using SP.Studio.Web;

namespace SP.Studio.Core
{
    /// <summary>
    /// 对象扩展
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 获取时间的秒部分
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetSecond(this DateTime date)
        {
            return date.Hour * 3600 + date.Minute * 60 + date.Second;
        }

        /// <summary>
        /// 把值转化成为安全类型
        /// </summary>
        public static T GetValue<T>(this object value)
        {
            return (T)value.GetValue(typeof(T));
        }

        /// <summary>
        /// 获取把值转化成为安全类型
        /// </summary>
        public static object GetValue(this object value, Type type = null)
        {
            if (type == null) type = value.GetType();
            if (value == null || value == DBNull.Value) return type.GetDefaultValue();
            object obj = null;
            Regex regex;
            Match match;
            switch (value.GetType().Name)
            {
                case "Object[]":
                    obj = ((string)value).ToObject(typeof(object[]));
                    break;
                case "DateTime":
                    obj = (DateTime)value == DateTime.MinValue ? DateTime.Parse("1900-1-1") : value;
                    break;
                case "String":
                    switch (type.Name)
                    {
                        case "Guid":
                            Guid guid;
                            Guid.TryParse((string)value, out guid);
                            obj = guid;
                            break;
                        case "Boolean":
                            obj = value.ToString().Equals("1") || value.ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase) || value.ToString().Equals("on", StringComparison.CurrentCultureIgnoreCase);
                            break;
                        case "Decimal":
                            decimal decimalValue;
                            obj = Decimal.TryParse((string)value, out decimalValue) ? decimalValue : 0.00M;
                            break;
                        case "Double":
                            double doubleValue;
                            obj = Double.TryParse((string)value, out doubleValue) ? doubleValue : 0.00D;
                            break;
                        case "Int64":
                            long longValue;
                            obj = long.TryParse((string)value, out longValue) ? longValue : (long)0;
                            break;
                        case "Int32":
                            int intValue;
                            obj = int.TryParse((string)value, out intValue) ? intValue : 0;
                            break;
                        case "Int16":
                        case "Short":
                            short shortValue;
                            obj = short.TryParse((string)value, out shortValue) ? shortValue : (short)0;
                            break;
                        case "Byte":
                            byte byteValue;
                            obj = byte.TryParse((string)value, out byteValue) ? byteValue : (byte)0;
                            break;
                        case "DateTime":
                            regex = new Regex(@"^(?<yyyy>\d{4})(?<MM>\d{2})(?<dd>\d{2})(?<HH>\d{2})(?<mm>\d{2})(?<ss>\d{2})$");
                            if (regex.IsMatch((string)value))
                            {
                                match = regex.Match((string)value);
                                value = string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                                    match.Groups["yyyy"].Value,
                                    match.Groups["MM"].Value,
                                    match.Groups["dd"].Value,
                                    match.Groups["HH"].Value,
                                    match.Groups["mm"].Value,
                                    match.Groups["ss"].Value);
                            }
                            DateTime dateTime;
                            obj = DateTime.TryParse((string)value, out dateTime) ? dateTime : new DateTime(1900, 1, 1);
                            break;
                        case "Int32[]":
                        case "System.Int32[]":
                            obj = WebAgent.GetArray<int>((string)value);
                            break;
                        case "String[]":
                            obj = ((string)value).Split(',');
                            break;
                        default:
                            if (type.IsBaseType(typeof(ISetting)))
                            {
                                obj = Activator.CreateInstance(type, (string)value);
                            }
                            else if (type.IsBaseType(typeof(JsonBase)))
                            {
                                obj = Activator.CreateInstance(type, value.ToString());
                            }
                            else if (type.IsBaseType(typeof(Enum)))
                            {

                            }
                            break;
                    }

                    switch (type.BaseType.Name)
                    {
                        case "Enum":
                            int enumValue;
                            if (type.HasAttribute(typeof(FlagsAttribute)))
                            {
                                int[] enumValues = WebAgent.GetArray<int>(value.ToString());
                                if (enumValues.Length > 0)
                                {
                                    enumValue = WebAgent.GetArray<int>(value.ToString()).Sum();
                                }
                                else
                                {
                                    enumValue = 0;
                                    value.ToString().Split(',').ToList().ForEach(t =>
                                    {
                                        enumValue += (int)Enum.Parse(type, t);
                                    });

                                }
                                obj = Enum.ToObject(type, enumValue);
                            }
                            else
                            {
                                obj = int.TryParse(value.ToString(), out enumValue) ? Enum.ToObject(type, enumValue) : Enum.Parse(type, value.ToString());
                            }
                            break;
                    }

                    break;
                default:
                    if (type.IsBaseType(typeof(ISetting))) obj = Activator.CreateInstance(type, value.ToString());
                    break;
            }

            return obj == null ? value : obj;
        }

        /// <summary>
        /// 获取类型的默认值
        /// </summary>
        internal static object GetDefaultValue(this Type type)
        {
            if (type == typeof(string)) return string.Empty;
            if (type == typeof(Guid)) return Guid.Empty;
            if (type == typeof(object[]) || type == typeof(int[])) return null;
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0}\nType:{1}", ex.Message, type.FullName));
            }
        }

        /// <summary>
        /// 把实体对象转化成为另一类型
        /// 通过属性名相同的方式进行转换
        /// </summary>
        /// <param name="t">要转换的对象</param>
        /// <param name="type">要转换到的类型</param>
        public static object ConvertType<T>(this T t, Type type) where T : class
        {
            if (t == null) return null;
            object obj = Activator.CreateInstance(type);
            List<PropertyInfo> propertys = type.GetProperties().ToList();
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                PropertyInfo pro = propertys.Find(p => p.Name.Equals(property.Name));
                if (pro == null) continue;
                object value = property.GetValue(t, null);
                if (property.PropertyType.IsEnum) value = Enum.Parse(pro.PropertyType, value.ToString());
                pro.SetValue(obj, value, null);
            }
            return obj;
        }

        /// <summary>
        /// 判断当前类型是否是派生类
        /// </summary>
        public static bool IsBaseType(this Type type, Type type2)
        {
            if (type2.IsInterface)
            {
                return type.GetInterface(type2.Name) != null;
            }
            while (type != null)
            {
                if (type == type2) return true;
                type = type.BaseType;
            }
            return false;
        }

        /// <summary>
        /// 判断对象是否拥有指定的属性
        /// </summary>
        public static bool HasAttribute(this object obj, Type type)
        {
            ICustomAttributeProvider custom = obj is ICustomAttributeProvider ? (ICustomAttributeProvider)obj : (ICustomAttributeProvider)obj.GetType();
            foreach (var t in custom.GetCustomAttributes(false))
            {
                if (t.GetType().Equals(type)) return true;
            }
            return false;
        }

        public static bool HasAttribute<T>(this object obj) where T : Attribute
        {
            return obj.HasAttribute(typeof(T));
        }

        /// <summary>
        /// 获取对象的属性值
        /// </summary>
        /// <returns>如果没有该属性则返回null</returns>
        public static T GetAttribute<T>(this object obj)
        {
            if (obj == null) return default(T);
            ICustomAttributeProvider custom = obj is ICustomAttributeProvider ? (ICustomAttributeProvider)obj : (ICustomAttributeProvider)obj.GetType();
            foreach (var t in custom.GetCustomAttributes(true))
            {
                if (t.GetType().Equals(typeof(T))) return (T)t;
            }
            return default(T);
        }

        /// <summary>
        /// 获取方法的注释
        /// </summary>
        public static string GetDescription(this MethodInfo method)
        {
            object[] objs = method.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs == null || objs.Length == 0) return method.Name;
            return ((DescriptionAttribute)objs[0]).Description;
        }

        /// <summary>
        /// 获取自定义的字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetString(this object obj, Type type = null)
        {
            if (obj == null) return null;
            string value = string.Empty;
            if (type == null) type = obj.GetType();
            switch (type.ToString())
            {
                case "Int32[]":
                case "System.Int32[]":
                    value = string.Join(",", (int[])obj);
                    break;
                default:
                    value = obj.ToString();
                    break;
            }
            return value;
        }

    }
}
