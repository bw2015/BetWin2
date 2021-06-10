using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web;
using System.Reflection;
using System.Runtime.Serialization;
using System.ComponentModel;

using SP.Studio.Array;
using SP.Studio.Web;

namespace SP.Studio.Core
{
    /// <summary>
    /// 键值对参数设置对象 
    /// </summary>
    [Serializable, DataContract]
    public abstract class SettingBase : ISetting
    {
        /// <summary>
        /// 默认构造
        /// </summary>
        public SettingBase() { }

        /// <summary>
        /// 赋值构造
        /// </summary>
        /// <param name="queryString"></param>
        public SettingBase(string queryString)
        {
            NameValueCollection request = HttpUtility.ParseQueryString(queryString ?? string.Empty);

            foreach (PropertyInfo property in this.GetType().GetProperties().Where(t => t.CanWrite))
            {
                if (request.AllKeys.Contains(property.Name))
                {
                    object value = request[property.Name];
                    switch (property.PropertyType.Name)
                    {
                        case "Boolean":
                            value = value.Equals("1") || value.ToString().Equals("true", StringComparison.CurrentCultureIgnoreCase);
                            break;
                        case "Int32[]":
                            value = WebAgent.GetArray<int>((string)value);
                            break;
                        case "Byte[]":
                            value = WebAgent.GetArray<byte>((string)value);
                            break;
                        case "String[]":
                            value = ((string)value).Split(',');
                            break;
                        default:
                            if (property.PropertyType.IsArray)
                            {
                                Type arrayType = property.PropertyType.GetElementType();
                                string[] values = ((string)value).Split(',');
                                System.Array array = System.Array.CreateInstance(arrayType, values.Length);
                                for (int i = 0; i < array.Length; i++)
                                {
                                    array.SetValue(values[i].ToEnum(arrayType), i);
                                }
                                value = array;
                            }
                            else if (property.PropertyType.IsEnum)
                            {
                                value = value.ToString().ToEnum(property.PropertyType);
                            }
                            break;
                    }
                    property.SetValue(this, Convert.ChangeType(value, property.PropertyType), null);
                }
            }
        }

        /// <summary>
        /// 转化成为QueryString字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            List<string> list = new List<string>();
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (!property.CanWrite) continue;
                object value = property.GetValue(this, null);
                if (value != null)
                {
                    switch (property.PropertyType.Name)
                    {
                        case "Int32[]":
                            value = string.Join(",", (int[])value);
                            break;
                        case "String[]":
                            value = string.Join(",", (string[])value);
                            break;
                        default:
                            if (property.PropertyType.IsArray)
                            {
                                System.Array array = (System.Array)value;
                                string[] arrayValue = new string[array.Length];
                                for (int i = 0; i < array.Length; i++)
                                {
                                    arrayValue[i] = array.GetValue(i).ToString();
                                }
                                value = string.Join(",", arrayValue);
                            }
                            break;
                    }
                }
                list.Add(string.Format("{0}={1}", property.Name, value == null ? "" : HttpUtility.UrlEncode(value.ToString())));
            }
            return list.Join('&');
        }

        /// <summary>
        /// 转化成为json 一个list对象
        /// </summary>
        /// <returns></returns>
        public virtual string ToSetting()
        {
            return this.GetType().GetProperties().Where(t => t.HasAttribute<DescriptionAttribute>()).Select(t => new
            {
                t.Name,
                Value = t.GetValue(this, null),
                Description = t.GetAttribute<DescriptionAttribute>().Description
            }).ToJson();
        }

        /// <summary>
        /// 默认转化成为字符串
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static implicit operator string(SettingBase setting)
        {
            return setting.ToString();
        }

    }
}
