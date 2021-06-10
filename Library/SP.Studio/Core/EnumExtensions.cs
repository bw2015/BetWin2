using SP.Studio.Array;
using SP.Studio.Model;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SP.Studio.Core
{
    /// <summary>
    /// 枚举扩展
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 枚举语言包
        /// </summary>
        private static Dictionary<string, string> _enumLanguageBag = new Dictionary<string, string>();

        public static void AddLanguageBag(string key, string value)
        {
            if (_enumLanguageBag.ContainsKey(key))
                _enumLanguageBag[key] = value;
            else
                _enumLanguageBag.Add(key, value);
        }

        /// <summary>
        /// 锁对象
        /// </summary>
        private static object objLock = new object();

        /// <summary>
        /// 缓存枚举说明的属性值（包括对象以及说明）
        /// </summary>
        private static Dictionary<string, object> enumList = new Dictionary<string, object>();

        /// <summary>
        /// 把字符串转化成为枚举
        /// </summary>
        public static T ToEnum<T>(this string value) where T : IComparable, IFormattable, IConvertible
        {
            if (string.IsNullOrEmpty(value) || !typeof(T).IsEnum) return default(T);
            if (WebAgent.IsType<int>(value))
            {
                return (T)Enum.ToObject(typeof(T), int.Parse(value));
            }
            Type t = typeof(T);
            if (t.HasAttribute(typeof(FlagsAttribute)))
            {
                long v = 0;
                value.Split(',').ToList().ForEach(en =>
                {
                    if (Enum.IsDefined(typeof(T), en.Trim()))
                        v += (long)Convert.ChangeType(Enum.Parse(t, en.Trim()), typeof(long));
                });
                return (T)Enum.Parse(t, v.ToString());
            }
            return Enum.IsDefined(t, value) ? (T)Enum.Parse(t, value) : default(T);
        }


        /// <summary>
        /// 把枚举转化成为同等数值的另一类型
        /// </summary>
        public static T ToEnum<T>(this Enum em) where T : IComparable, IFormattable, IConvertible
        {
            return (T)((object)em);
        }

        /// <summary>
        /// 把字符串转化成为枚举（用反射实现，效率较低）
        /// </summary>
        public static object ToEnum(this string value, Type type)
        {
            MethodInfo mi = typeof(EnumExtensions).GetMethods().FirstOrDefault(t => t.Name == "ToEnum" && t.IsGenericMethod);
            MethodInfo gmi = mi.MakeGenericMethod(type);
            return gmi.Invoke(null, new object[] { value });
        }

        /// <summary>
        /// 位枚举的名字
        /// </summary>
        /// <param name="em"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string GetName(this Enum em, string split = ",")
        {
            if (em.GetType().HasAttribute(typeof(FlagsAttribute)))
            {
                List<string> flags = new List<string>();
                foreach (object e in em.GetType().GetEnumValues())
                {
                    int value = Convert.ToInt32(e);
                    if (value == 0) continue;
                    if (em.HasFlag((Enum)e))
                    {
                        flags.Add(((Enum)e).ToString());
                    }
                }
                return flags.Join(split);
            }
            return em.ToString();
        }

        /// <summary>
        /// 获取位枚举的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<Enum> GetValue(this Enum t)
        {
            Type type = t.GetType();
            if (!type.HasAttribute<FlagsAttribute>())
            {
                yield return t;
                yield break;
            }
            foreach (Object value in type.GetEnumValues())
            {
                if (t.HasFlag((Enum)value)) yield return (Enum)value;
            }
        }

        /// <summary>
        /// 获取枚举的说明
        /// </summary>
        /// <param name="split">位枚举的分割符号（仅对位枚举有作用）</param>
        public static string GetDescription(this Enum em, string split = ",")
        {
            if (em.GetType().GetCustomAttributes(typeof(FlagsAttribute), false).Length != 0 && em.ToString().Count(t => t == ',') > 0)
            {
                List<string> flags = new List<string>();
                foreach (object e in em.GetType().GetEnumValues())
                {
                    int value = Convert.ToInt32(e);
                    if (value == 0) continue;
                    if (em.HasFlag((Enum)e))
                    {
                        flags.Add(((Enum)e).GetDescription());
                    }
                }
                return flags.Join(split);
            }

            string key = string.Format("{0}.{1}", em.GetType().FullName, em);
            if (_enumLanguageBag.ContainsKey(key)) return _enumLanguageBag[key] + "(" + key + ")";
            if (enumList.ContainsKey(key)) return (string)enumList[key];
            lock (objLock)
            {
                string description;
                object[] attrs;
                bool isEnum = false;
                foreach (FieldInfo f in em.GetType().GetFields())
                {
                    if (f.Name.Equals(em.ToString())) isEnum = true;
                    key = string.Format("{0}.{1}", em.GetType().FullName, f.Name);
                    if (f.IsSpecialName) continue;
                    attrs = f.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    description = f.Name;
                    foreach (Attribute att in attrs)
                    {
                        description = ((DescriptionAttribute)att).Description;
                    }
                    if (!enumList.ContainsKey(key))
                    {
                        enumList.Add(key, description);
                    }
                }
                return isEnum ? em.GetDescription() : em.ToString();
            }
        }

        /// <summary>
        /// 获取枚举的指定属性
        /// </summary>
        /// <param name="em"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this Enum em) where T : Attribute
        {
            string key = string.Format("{0}.{1}[{2}]", em.GetType().FullName, em, typeof(T).Name);
            if (enumList.ContainsKey(key)) return (T)enumList[key];
            if (!Enum.IsDefined(em.GetType(), em)) return null;
            lock (objLock)
            {
                object[] attrs;
                foreach (FieldInfo f in em.GetType().GetFields())
                {
                    string fieldKey = string.Format("{0}.{1}[{2}]", em.GetType().FullName, f.Name, typeof(T).Name);
                    if (f.IsSpecialName) continue;
                    attrs = f.GetCustomAttributes(typeof(T), false);
                    if (attrs != null && attrs.Length != 0)
                    {
                        T t = (T)attrs[0];
                        if (!enumList.ContainsKey(fieldKey))
                        {
                            enumList.Add(fieldKey, t);
                        }
                    }
                    else
                    {
                        enumList.Add(fieldKey, default(T));
                    }
                }
            }
            return (T)enumList[key];
        }

        /// <summary>
        /// 把枚举转换成为列表
        /// </summary>
        public static List<EnumObject> ToList(this Type type)
        {
            List<EnumObject> list = new List<EnumObject>();
            foreach (object obj in Enum.GetValues(type))
            {
                list.Add(new EnumObject((Enum)obj));
            }
            return list;
        }

        /// <summary>
        /// 获取字符串格式的枚举
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetEnum(this Assembly[] assembly)
        {
            foreach (Assembly ass in assembly)
            {
                foreach (Type t in ass.GetTypes().Where(t => t.IsEnum && (t.IsPublic || t.IsNestedPublic)))
                {
                    yield return string.Format("\"{0}\":{1}", t.FullName, t.ToJson());
                }
            }
        }

        /// <summary>
        /// 获取字典形式的枚举
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetEnumDictionary(this Assembly[] assembly)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Assembly ass in assembly)
            {
                foreach (Type t in ass.GetTypes().Where(t => t.IsEnum && (t.IsPublic || t.IsNestedPublic)))
                {
                    dic.Add(t.FullName, t.ToJson());
                }
            }
            return dic;
        }
    }
}
