using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using System.Data;

using SP.Studio.Array;
using SP.Studio.Model;
namespace SP.Studio.Core
{
    /// <summary>
    /// 序列化相关方法
    /// 包含把实体对象序列化和反序列化的扩展方法
    /// </summary>
    public static class SerializationExtensions
    {
        private const string ITEM_BOTTOM = "bottom-";

        /// <summary>
        /// 转化对象为系统所带的JSON格式字符串
        /// </summary>
        public static string ToJsonString(this Object obj, Encoding encoding = null)
        {
            if (obj == null) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            return encoding.GetString(dataBytes);
        }

        /// <summary>
        /// 标记这是一个JSON字符串
        /// </summary>
        public static JsonString ToJsonString(this string json)
        {
            return new JsonString(json);
        }

        /// <summary>
        /// 序列化XML数据 可反序列化成实体类
        /// </summary>
        public static string ToXmlString(this Object obj, Encoding encoding = null)
        {
            if (obj == null) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            serializer.Serialize(stream, obj);
            byte[] dataBytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(dataBytes, 0, (int)stream.Length);
            return encoding.GetString(dataBytes);
        }

        /// <summary>
        /// 转化成为二进制文件
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="filePath">要保存的路径</param>
        /// <returns></returns>
        public static void ToBinaryFile(this Object obj, string filePath)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(stream, obj);
                stream.Close();
            }
        }

        /// <summary>
        /// 字符串反序列化成为实体类的方法
        /// 必须配合 ToJsonString 或者 ToXmlString 方法
        /// </summary>
        public static object ToObject(this string str, Type type = null, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(str)) return null;
            if (encoding == null) encoding = Encoding.UTF8;
            object obj = null;
            if (type == null && Regex.IsMatch(str, @"^[a-z]\:", RegexOptions.IgnoreCase))
            {
                obj = BinaryToObject(str);
            }
            switch (str[0])
            {
                case '<':
                    obj = XmlToObject(str, type, encoding);
                    break;
                case '{':
                case '[':
                    obj = JsonToObjec(str, type, encoding);
                    break;
            }
            return obj;
        }

        public static T ToObject<T>(this string str, Encoding encoding = null)
        {
            T t = default(T);
            if (string.IsNullOrEmpty(str)) return t;
            if (encoding == null) encoding = Encoding.UTF8;
            return (T)str.ToObject(typeof(T), encoding);
        }

        /// <summary>
        /// 对象转为二进制序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToSerializationBinary(this object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        #region ============  反序列化方法  =============
        /// <summary>
        /// Json反序列化
        /// </summary>
        private static object JsonToObjec(string str, Type type, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            object obj;
            DataContractJsonSerializer jsr = new DataContractJsonSerializer(type);
            MemoryStream ms = new MemoryStream(encoding.GetBytes(str));
            try
            {
                obj = jsr.ReadObject(ms);
            }
            catch
            {
                obj = null;
            }
            finally
            {
                ms.Close();
            }
            return obj;
        }

        /// <summary>
        /// xml反序列化
        /// </summary>
        private static object XmlToObject(string str, Type type, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            object obj;
            XmlSerializer xsr = new XmlSerializer(type);
            MemoryStream ms = new MemoryStream(encoding.GetBytes(str));
            try
            {
                obj = xsr.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
            return obj;
        }

        /// <summary>
        /// 二进制文件的反序列化
        /// </summary>
        private static object BinaryToObject(string filePath)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// 二进制数据转化成为对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Unserialize<T>(this byte[] data)
        {
            if (data == null) return default(T);
            using (MemoryStream stream = new MemoryStream(data))
            {
                return (T)new BinaryFormatter().Deserialize(stream);
            }
        }

        #endregion


        /// <summary>
        /// 转化成为自定义的JSON字符串
        /// </summary>
        public static string ToJson<T>(this List<T> list, params Expression<Func<T, object>>[] funs) where T : class
        {
            StringBuilder sb = new StringBuilder("[");
            sb.Append(string.Join(",", list.ConvertAll(t => t.ToJson(funs))));
            sb.Append("]");
            return sb.ToString();
        }


        public static string ToJson<T>(this IEnumerable<T> list, params Expression<Func<T, object>>[] funs) where T : class
        {
            StringBuilder sb = new StringBuilder("[");
            sb.Append(string.Join(",", list.Select(t => t.ToJson(funs))));
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// 把枚举转化成为Json 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToJson(this Type type)
        {
            if (!type.IsEnum) return string.Empty;
            return string.Concat("{", string.Join(",", type.ToList().Select(t => string.Format("\"{0}\":\"{1}\"", t.Name, t.Description))), "}");
        }

        /// <summary>
        /// 把当前实体类转成JSON字符串
        /// </summary>
        /// <param name="fields">要使用的字段。为空表示全部</param>
        public static string ToJson<T>(this T obj, params Expression<Func<T, object>>[] funs) where T : class
        {
            if (obj is JsonString) return obj.ToString();

            StringBuilder sb = new StringBuilder();

            if (typeof(T) is Type)
            {

            }

            sb.Append("{");
            if (obj == null) obj = (T)Activator.CreateInstance(typeof(T));
            if (obj is IDictionary)
            {
                IDictionary dictionary = (IDictionary)obj;
                if (dictionary != null)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Value == null)
                        {
                            sb.AppendFormat("\"{0}\":\"\",", entry.Key);
                        }
                        else if (entry.Value.GetType() == typeof(JsonString))
                        {
                            sb.AppendFormat("\"{0}\":{1},", entry.Key, ((JsonString)entry.Value).ToString());
                        }
                        else if (entry.Value.GetType() == typeof(Boolean))
                        {
                            sb.AppendFormat("\"{0}\":\"{1}\",", entry.Key, (bool)entry.Value ? "true" : "false");
                        }
                        else
                        {
                            sb.AppendFormat("\"{0}\":{1},", entry.Key, SafeValue(entry.Value, true));
                        }
                    }
                }
            }
            else
            {
                string format = null;

                if (funs.Length == 0)
                {
                    foreach (PropertyInfo property in obj.GetType().GetProperties())
                    {
                        if (property.HasAttribute<FormatAttribute>())
                        {
                            format = property.GetAttribute<FormatAttribute>().format;
                        }
                        try
                        {
                            sb.AppendFormat("\"{0}\":{1},", property.Name, SafeValue(property.GetValue(obj, null), true, format));
                        }
                        catch (Exception ex)
                        {
                            sb.AppendFormat("\"{0}\":\"{1}\" ,", property.Name, ex.Message);
                        }
                    }
                }
                else
                {
                    foreach (var fun in funs)
                    {
                        PropertyInfo property = fun.GetPropertyInfo();
                        if (property.HasAttribute<FormatAttribute>())
                        {
                            format = property.GetAttribute<FormatAttribute>().format;
                        }
                        sb.AppendFormat("\"{0}\":{1} ,", property.Name, SafeValue(fun.Compile().Invoke(obj), true, format));
                    }
                }
            }
            string json = sb.ToString();
            if (json.EndsWith(",")) json = json.Substring(0, json.Length - 1);
            return string.Concat(json, "}");
        }

        /// <summary>
        /// 把数据库行转成json对象
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static string ToJson(this DataRow dr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            int count = dr.Table.Columns.Count;
            int index = 0;
            foreach (DataColumn column in dr.Table.Columns)
            {
                index++;
                sb.AppendFormat("\"{0}\":\"{1}\"{2}",
                    column.ColumnName, SafeValue(dr[index - 1]), index == count ? "" : ",");

            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 把数据库行转成json列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToJson(this DataRowCollection list)
        {
            int count = list.Count;
            int index = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            foreach (DataRow dr in list)
            {
                index++;
                sb.AppendFormat("{0}{1}",
                   dr.ToJson(), index == count ? "" : ",");
            }
            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// 把当前实体类转成JSON字符串(值不加引号)
        /// </summary>
        public static string ToJson2<T>(this T obj, params Expression<Func<T, object>>[] funs) where T : class
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 转化成为实体类
        /// </summary>
        /// <param name="coll">Request 提交过来的数据集合</param>
        /// <param name="t">要绑定到的实体类。 如果不为空则在集合中不存在的属性就保留初始值</param>
        /// <param name="name">属性名的前缀值</param>
        /// <param name="isHtmlEncode">是否过滤html字符</param>
        public static T Fill<T>(this NameValueCollection coll, T t = null, string name = null, bool isHtmlEncode = true) where T : class
        {
            bool hasValue = t != null;
            if (!hasValue) t = (T)Activator.CreateInstance(typeof(T));
            List<string> allKey = coll.AllKeys.ToList().FindAll(s => !string.IsNullOrEmpty(s)).ConvertAll(s => s.ToLower());
            foreach (PropertyInfo property in t.GetType().GetProperties().Where(p => p.CanWrite && !p.HasAttribute(typeof(NoFillAttribute))))
            {
                object value = null;
                Type type = property.PropertyType;
                try
                {
                    bool hasCollection = !hasValue; //是否需要更新
                    if ((string.IsNullOrEmpty(name) && allKey.Contains(property.Name.ToLower())) ||
                            (!string.IsNullOrEmpty(name) && allKey.Contains(name.ToLower() + "." + property.Name.ToLower())))
                    {
                        string valueName = string.IsNullOrEmpty(name) ? property.Name : name + "." + property.Name;
                        if (property.PropertyType.HasAttribute<FlagsAttribute>())
                        {
                            value = coll[valueName].ToEnum(property.PropertyType);
                        }
                        else
                        {
                            switch (type.Name)
                            {
                                case "String":
                                    value = (isHtmlEncode && !property.HasAttribute<HtmlEncodeAttribute>()) ? HttpUtility.HtmlEncode(coll[valueName]) : coll[valueName];
                                    break;
                                default:
                                    value = Convert.ChangeType(coll[valueName].GetValue(type), type);
                                    break;
                            }
                        }
                        hasCollection = true;
                    }
                    else if (!hasValue)
                    {
                        value = type.GetDefaultValue();
                    }

                    if (hasCollection)
                    {
                        property.SetValue(t, value, null);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{0}\n参数名:{1}\n值:{2}", ex.Message, property.Name, coll[property.Name]));
                }
            }
            return t;
        }

        /// <summary>
        /// 把Post的值转化成为QueryString字符串
        /// </summary>
        public static string ToQueryString(this NameValueCollection coll)
        {
            List<string> list = new List<string>();
            foreach (string key in coll.AllKeys)
            {
                list.Add(string.Format("{0}={1}", key, HttpUtility.UrlEncode(coll[key])));
            }
            return string.Join("&", list);
        }

        public static T Fill<T>(this T t, string name = null) where T : class
        {
            return HttpContext.Current.Request.Form.Fill(t, name);
        }

        /// <summary>
        /// 把List对象转换成为下拉框对象
        /// </summary>
        /// <param name="items">自定义插入的选项 默认为插入最顶部。 如果需要插入尾部则在Text项以 bottom- 开头</param>
        public static string ToDropDownList<T>(this IEnumerable<T> list, Converter<T, Object> value, Converter<T, Object> text, string property = null, object selectValue = null, params ListItem[] items)
        {
            StringBuilder sb = new StringBuilder();
            var valueList = list.Select(t => value(t)).ToArray();
            var textList = list.Select(t => text(t)).ToArray();
            if (string.IsNullOrEmpty(property)) property = string.Format("Name=\"{0}\"", typeof(T).Name);
            if (selectValue == null) selectValue = string.Empty;
            sb.AppendFormat("<select {0}>", property);
            foreach (ListItem item in items.ToList().FindAll(t => !t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text, selectValue.ToString() == item.Value ? " selected" : "");
            }
            for (int index = 0; index < list.Count(); index++)
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", valueList[index], textList[index],
                    selectValue.ToString() == valueList[index].ToString() ? " selected" : "");
            }
            foreach (ListItem item in items.ToList().FindAll(t => t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text.Substring(ITEM_BOTTOM.Length), selectValue.ToString() == item.Value ? " selected" : "");
            }
            sb.Append("</select>");
            return sb.ToString();
        }

        /// <summary>
        /// 有层级的下拉框
        /// </summary>
        public static string ToDropDownList<T, TKey>(this IEnumerable<T> list, Converter<T, TKey> value, Converter<T, Object> text, Converter<T, TKey> parent, TKey parentValue, string property = null, object selectValue = null, params ListItem[] items)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(property)) property = string.Format("Name=\"{0}\"", typeof(T).Name);
            if (selectValue == null) selectValue = string.Empty;
            sb.AppendFormat("<select {0}>", property);
            foreach (ListItem item in items.ToList().FindAll(t => !t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text, selectValue.ToString() == item.Value ? " selected" : "");
            }
            ToDropDownList(list, value, text, parent, parentValue, ref sb, 0);
            foreach (ListItem item in items.ToList().FindAll(t => t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text.Substring(ITEM_BOTTOM.Length), selectValue.ToString() == item.Value ? " selected" : "");
            }
            sb.Append("</select>");

            return selectValue == null ? sb.ToString() : sb.ToString().Replace("value=\"" + selectValue + "\"", "value=\"" + selectValue + "\" selected");
        }

        private static void ToDropDownList<T, TKey>(IEnumerable<T> list, Converter<T, TKey> value, Converter<T, Object> text, Converter<T, TKey> parent, TKey parentValue, ref StringBuilder sb, int depth = 0)
        {
            foreach (var t in list.Where(obj => parent.Invoke(obj).Equals(parentValue)))
            {
                sb.AppendFormat("<option value=\"{0}\" data-parent=\"{3}\">{2}{1}</option>", value.Invoke(t), text.Invoke(t),
                    depth == 0 ? "" : "".PadLeft(depth, '　'), parentValue);
                ToDropDownList(list, value, text, parent, value.Invoke(t), ref sb, depth + 1);
            }
        }

        /// <summary>
        /// 把列表转化成为有层级的HTML结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">列表</param>
        /// <param name="value">自身ID 等于子集的父ID</param>
        /// <param name="parent">父ID字段</param>
        /// <param name="format">格式化</param>
        /// <param name="parentValue">默认的第一层父级</param>
        /// <param name="depath">当前层次</param>
        /// <returns></returns>
        public static string ToHtml<T>(this List<T> list, Converter<T, Object> value, Converter<T, Object> parent, Func<T, int, string> format, object parentValue, int depath = 0)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var obj in list.FindAll(t => parent.Invoke(t).Equals(parentValue)))
            {
                sb.Append(format.Invoke(obj, depath));
                sb.Append(list.ToHtml(value, parent, format, value.Invoke(obj), depath + 1));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 把枚举转化成为下拉框对象 兼容旧的参数对象
        /// </summary>
        public static string ToDropDownList(this Type type, EnumDropDownType show, string property = null, object selectValue = null, params ListItem[] items)
        {
            return type.ToDropDownList(show, property, selectValue, null, items);
        }

        /// <summary>
        /// 把枚举转化成为下拉框对象(支持布尔类型）
        /// 可自定义显示方式
        /// </summary>
        public static string ToDropDownList(this Type type, EnumDropDownType show, string property, object selectValue, Func<string, bool> filter, Func<Enum, string> textShow, params ListItem[] items)
        {
            if (textShow == null) textShow = t => t.GetDescription();
            StringBuilder sb = new StringBuilder();
            if (property == null) property = string.Format("name=\"{0}\"", type.Name);
            if (selectValue == null) selectValue = string.Empty;
            sb.AppendFormat("<select {0}>", property);
            foreach (ListItem item in items.ToList().FindAll(t => !t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text, selectValue.ToString() == item.Value ? " selected" : "");
            }
            switch (type.Name)
            {
                case "Boolean":
                    bool isSelected = false;
                    bool noSelected = false;
                    if (selectValue.GetType() == typeof(Boolean))
                    {
                        isSelected = (bool)selectValue;
                        noSelected = !isSelected;
                    }
                    else
                    {
                        isSelected = Regex.IsMatch(selectValue.ToString(), "^(1|yes|true)$", RegexOptions.IgnoreCase);
                        noSelected = Regex.IsMatch(selectValue.ToString(), "^(0|no|false)$", RegexOptions.IgnoreCase);
                    }
                    sb.AppendFormat("<option value=\"1\"{0}>是</option>", isSelected ? " selected" : "")
                        .AppendFormat("<option value=\"0\"{0}>否</option>", noSelected ? " selected" : "");
                    break;
                default:

                    foreach (FieldInfo f in type.GetFields())
                    {
                        if (f.IsSpecialName || f.HasAttribute(typeof(ObsoleteAttribute))) continue;

                        Enum _enum = (Enum)Enum.Parse(type, f.Name);
                        string v = null;
                        switch (show)
                        {
                            case EnumDropDownType.Text:
                                v = _enum.ToString();
                                break;
                            case EnumDropDownType.Value:
                                v = Convert.ChangeType(_enum, typeof(int)).ToString();
                                break;
                            case EnumDropDownType.Description:
                                v = _enum.GetDescription();
                                break;
                        }
                        if (filter == null || filter.Invoke(v))
                        {
                            sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", v, textShow.Invoke(_enum),
                                selectValue != null && selectValue.ToString().Equals(v) ? " selected" : "");
                        }
                    }

                    break;
            }


            foreach (ListItem item in items.ToList().FindAll(t => t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text.Substring(ITEM_BOTTOM.Length), selectValue.ToString() == item.Value ? " selected" : "");
            }
            sb.Append("</select>");
            return sb.ToString();

        }

        /// <summary>
        /// 把枚举转化成为下拉框对象
        /// </summary>
        public static string ToDropDownList(this Type type, EnumDropDownType show, string property, object selectValue, Func<string, bool> filter, params ListItem[] items)
        {
            return type.ToDropDownList(show, property, selectValue, filter, null, items);
        }



        /// <summary>
        /// 带分组元素的下拉列表
        /// </summary>
        /// <param name="list">列表对象</param>
        /// <param name="value">列表值</param>
        /// <param name="value">列表显示的文本</param>
        /// <param name="keySelector">分组条件</param>
        /// <param name="label">分组名称</param>
        public static string ToDropDownList<T, TKey>(this IEnumerable<T> list, Converter<T, Object> value, Converter<T, Object> text, List<TKey> keySelector, Converter<TKey, string> label, Func<T, TKey, bool> match, string property, object selectValue = null, params ListItem[] items)
        {
            if (selectValue == null) selectValue = string.Empty;
            StringBuilder sb = new StringBuilder();
            if (property == null) property = string.Format("name=\"{0}\"", typeof(T).Name);
            sb.AppendFormat("<select {0}>", property);
            foreach (ListItem item in items.ToList().FindAll(t => !t.Text.StartsWith(ITEM_BOTTOM)))
                sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", item.Value, item.Text, selectValue.ToString() == item.Value ? " selected" : "");

            foreach (TKey key in keySelector)
            {
                sb.AppendFormat("<optgroup label=\"{0}\">", label.Invoke(key));
                list.Where(t => match.Invoke(t, key)).ToList().ForEach(t =>
                {
                    sb.AppendFormat("<option value=\"{0}\"{2}>{1}</option>", value.Invoke(t), text.Invoke(t), selectValue.Equals(value.Invoke(t)) ? " selected" : "");
                });
                sb.Append("</optgroup>");
            }
            sb.Append("</select>");

            return sb.ToString();
        }

        /// <summary>
        /// 根据枚举进行分组显示
        /// </summary>
        public static string ToDropDownList<T, TKey>(this IEnumerable<T> list, Converter<T, Object> value, Converter<T, Object> text, Func<T, TKey, bool> match, string property, object selectValue = null, params ListItem[] items) where TKey : struct, IComparable, IConvertible, IFormattable
        {
            List<TKey> keySelector = Enum.GetNames(typeof(TKey)).ToList().ConvertAll(t => (TKey)Enum.Parse(typeof(TKey), t));

            return list.ToDropDownList(value, text, keySelector, t => (t as Enum).GetDescription(),
                match, property, selectValue, items);
        }


        /// <summary>
        /// 把枚举转化成为多选框（仅适用于位枚举）
        /// </summary>
        public static string ToCheckBoxList(this Type type, string property = null, Enum selectValue = null, params ListItem[] items)
        {
            StringBuilder sb = new StringBuilder();
            if (property == null) property = string.Format("name=\"{0}\"", type.Name);

            foreach (object obj in Enum.GetNames(type))
            {
                Enum _enum = (Enum)Enum.Parse(type, obj.ToString());
                string value = Convert.ChangeType((Enum)Enum.Parse(type, obj.ToString()), typeof(int)).ToString();
                if (value == "0") continue;
                sb.AppendFormat("<input type=\"checkbox\" id=\"{4}\" value=\"{3}\" {0} {2} /><label for=\"{4}\">{1}</label>", property, _enum.GetDescription(),
                    selectValue != null && selectValue.HasFlag(_enum) ? "checked=\"checked\"" : "", value,
                     Guid.NewGuid().ToString("n"));
            }
            return sb.ToString();
        }

        public static string ToRoaidBoxList(this Type type, string property = null, object selectValue = null, Func<string, bool> filter = null, params ListItem[] items)
        {
            return type.ToRoaidBoxList(property, selectValue, filter, null, items);
        }

        /// <summary>
        /// 把枚举转化成为单选列表
        /// </summary>
        /// <param name="filter">string ： 枚举的字符串</param>
        /// <param name="labelShow">对外文字的显示方式</param>
        public static string ToRoaidBoxList(this Type type, string property, object selectValue, Func<string, bool> filter, Func<string, string> labelShow, params ListItem[] items)
        {
            StringBuilder sb = new StringBuilder();
            if (property == null) property = string.Format("name=\"{0}\"", type.Name);
            if (filter == null) filter = t => true;
            if (labelShow == null) labelShow = t => t;
            foreach (object obj in Enum.GetNames(type))
            {
                Enum _enum = (Enum)Enum.Parse(type, obj.ToString());
                string value = Convert.ChangeType((Enum)Enum.Parse(type, obj.ToString()), typeof(int)).ToString();
                if (!filter.Invoke(value)) continue;
                sb.AppendFormat("<input type=\"radio\" id=\"{4}\" value=\"{3}\" {0} {2} /><label for=\"{4}\">{1}</label>", property, labelShow(_enum.GetDescription()),
                    selectValue != null && selectValue.ToString().Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase) ? "checked=\"checked\"" : "", value,
                     Guid.NewGuid().ToString("n"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 把列表转化成为单选框
        /// </summary>
        public static string ToRoaidBoxList<T, TValue>(this IEnumerable<T> list, Converter<T, TValue> value, Converter<T, object> text, string property, TValue selectedValue = default(TValue), params ListItem[] items)
        {
            StringBuilder sb = new StringBuilder();
            if (property == null) property = string.Format("name=\"{0}\"", typeof(T).Name);
            if (selectedValue == null) selectedValue = default(TValue);

            int index = 0;
            string id = null;
            string key = SP.Studio.Security.MD5.Encrypto(property).ToLower().Substring(0, 8);
            foreach (ListItem item in items.ToList().FindAll(t => !t.Text.StartsWith(ITEM_BOTTOM)))
            {
                index++;
                id = string.Concat(typeof(T).Name, "_", key, "_", index);
                sb.AppendFormat("<label for=\"{0}\"><input type=\"radio\" id=\"{0}\" value=\"{1}\" {2} {3} />{4}</label>",
                   id, item.Value, property, item.Value.Equals(selectedValue.ToString()) ? "checked=\"checked\"" : "", item.Text);
            }
            foreach (T t in list)
            {
                index++;
                id = string.Concat(typeof(T).Name, "_", key, "_", index);
                sb.AppendFormat("<label for=\"{0}\"><input type=\"radio\" id=\"{0}\" value=\"{1}\" {2} {3} />{4}</label>",
                    id, value.Invoke(t), property, value.Invoke(t).Equals(selectedValue) ? "checked=\"checked\"" : "", text.Invoke(t));
            }
            foreach (ListItem item in items.ToList().FindAll(t => t.Text.StartsWith(ITEM_BOTTOM)))
            {
                index++;
                id = string.Concat(typeof(T).Name, "_", key, "_", index);
                sb.AppendFormat("<label for=\"{0}\">{<input type=\"radio\" id=\"{0}\" value=\"{1}\" {2} {3} />4}</label>",
                   id, item.Value, property, item.Value.Equals(selectedValue.ToString()) ? "checked=\"checked\"" : "", item.Text);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 兼容旧版的转化
        /// </summary>
        public static string ToCheckBoxList<T>(this List<T> list, Converter<T, Object> value, Converter<T, Object> text, string property, object[] selectValue, params ListItem[] items)
        {
            return ToCheckBoxList(list, value, text, property, t => selectValue != null && selectValue.Contains(t), items);
        }

        /// <summary>
        /// 把列表转化成为多选框对象
        /// </summary>
        public static string ToCheckBoxList<T>(this List<T> list, Converter<T, Object> value, Converter<T, Object> text, string property = null, Func<Object, bool> selected = null, params ListItem[] items)
        {
            StringBuilder sb = new StringBuilder();
            if (property == null) property = string.Format("name=\"{0}\"", typeof(T).Name);
            if (selected == null) selected = t => false;
            var valueList = list.ConvertAll(value);
            var textList = list.ConvertAll(text);

            foreach (ListItem item in items.ToList().FindAll(t => !t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<label for=\"{4}\"><input type=\"checkbox\" id=\"{4}\" value=\"{3}\" {0} {2} />{1}</label>", property, item.Text, selected.Invoke(item.Value) ? "checked=\"checked\"" : "",
                    item.Value, Guid.NewGuid().ToString("n"));
            }
            for (int index = 0; index < list.Count; index++)
            {
                sb.AppendFormat("<label for=\"{4}\"><input type=\"checkbox\" id=\"{4}\" value=\"{3}\" {0} {2} />{1}</label>", property, textList[index], selected.Invoke(valueList[index]) ? "checked=\"checked\"" : "",
                   valueList[index], Guid.NewGuid().ToString("n"));
            }
            foreach (ListItem item in items.ToList().FindAll(t => t.Text.StartsWith(ITEM_BOTTOM)))
            {
                sb.AppendFormat("<label for=\"{4}\"><input type=\"checkbox\" id=\"{4}\" value=\"{3}\" {0} {2} />{1}</label>", property, item.Text, selected.Invoke(item.Value) ? "checked=\"checked\"" : "",
                   item.Value, Guid.NewGuid().ToString("n"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 安全字符串
        /// </summary>
        /// <param name="obj">要转化的对象</param>
        /// <param name="quote">是否自动加上引号</param>
        /// <param name="format">要转换成为的格式</param>
        private static string SafeValue(object obj, bool quote = false, string format = null)
        {
            Func<string, string> ac = t => t;
            if (quote)
            {
                ac = t => string.Concat("\"", t, "\"");
            }
            if (obj == null) return ac("");
            string v = "";
            switch (obj.GetType().Name)
            {
                case "Boolean":
                    v = ac((bool)obj ? "true" : "false");
                    break;
                case "DateTime":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((DateTime)obj).ToString(format));
                    break;
                case "TimeSpan":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((TimeSpan)obj).ToString(format));
                    break;
                case "Int32":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((int)obj).ToString(format));
                    break;
                case "Int16":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((short)obj).ToString(format));
                    break;
                case "Int64":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((long)obj).ToString(format));
                    break;
                case "Byte":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((byte)obj).ToString(format));
                    break;
                case "Decimal":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((decimal)obj).ToString(format));
                    break;
                case "Single":
                    v = ac(string.IsNullOrEmpty(format) ? obj.ToString() : ((Single)obj).ToString(format));
                    break;
                case "String":
                    v = ac(HttpUtility.JavaScriptStringEncode(obj.ToString()));
                    break;
                case "JsonString":
                    v = ((JsonString)obj).ToString();
                    break;
                case "Int32[]":
                    v = ac(string.Join(",", ((int[])obj)));
                    break;
                case "String[]":
                    v = string.Concat("[", string.Join(",", ((string[])obj).Select(t => string.Concat("\"", t, "\""))), "]");
                    break;
                default:
                    if (typeof(Enum).IsAssignableFrom(obj.GetType()))  // 枚举
                    {
                        v = ac(obj.ToString());
                    }
                    else
                    {
                        v = ac(obj.ToString());
                    }
                    break;
            }
            return v;
        }
    }

    public enum EnumDropDownType
    {
        Text, Value, Description
    }

    /// <summary>
    /// 标记一个字符串是JSON格式的字符串
    /// </summary>
    public struct JsonString
    {
        private string _jsonString;

        public JsonString(string jsonString)
        {
            this._jsonString = jsonString;
        }

        public JsonString(params object[] args)
        {
            this._jsonString = string.Concat(args);
        }

        public override string ToString()
        {
            return this._jsonString;
        }
    }
}
