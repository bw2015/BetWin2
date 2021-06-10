using SP.Studio.Core;
using SP.Studio.Data;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Cache.Redis
{
    public static class RedisUtils
    {
        /// <summary>
        /// 把缓存实体类转化成为Redis参数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> ToHashEntry<T>(this T obj) where T : IRedisBase
        {
            IEnumerable<PropertyInfo> fields = obj.GetType().GetProperties();
            if (typeof(T).HasAttribute<ICacheAttribute>()) fields = fields.Where(t => t.HasAttribute<ICacheAttribute>());
            foreach (PropertyInfo property in fields)
            {
                object value = null;
                string name = null;
                RedisValue redisValue;
                try
                {
                    name = property.Name;
                    value = property.GetValue(obj);
                    redisValue = value.GetRedisValue();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + "\n" + string.Format("{0}.{1}={2}({3})转换成为RedisValue无效", typeof(T).Name, name, value, value.GetType().FullName));
                }
                yield return new HashEntry(name, redisValue);
            }
        }

        public static IEnumerable<HashEntry> ToHashEntry<T>(this T t, params Expression<Func<T, object>>[] fields)
        {
            bool isHasCache = typeof(T).HasAttribute<ICacheAttribute>();
            foreach (Expression<Func<T, object>> field in fields)
            {
                PropertyInfo property = field.ToPropertyInfo();
                if (isHasCache && !property.HasAttribute<ICacheAttribute>()) continue;
                object value = property.GetValue(t);
                if (value != null) yield return new HashEntry(property.Name, value.GetRedisValue());
            }
        }

        /// <summary>
        /// 获取hash表的key-value对象
        /// 不存在的键值自动过滤
        /// </summary>
        /// <param name="db"></param>
        /// <param name="key"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> GetHashValue(this IDatabase db, RedisKey key, params RedisValue[] fields)
        {
            RedisValue[] values = db.HashGet(key, fields);
            for (int i = 0; i < fields.Length; i++)
            {
                if (!values[i].IsNull) yield return new HashEntry(fields[i], values[i]);
            }
        }

        /// <summary>
        /// 把object转化成为Redis支持的格式
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static RedisValue GetRedisValue(this object obj)
        {
            Type type = obj.GetType();
            RedisValue value;

            switch (type.Name)
            {
                case "String":
                    value = (string)obj;
                    break;
                case "Int32":
                    value = (int)obj;
                    break;
                case "Int16":
                    value = (int)((short)obj);
                    break;
                case "Byte":
                    value = (byte)obj;
                    break;
                case "Int64":
                    value = (long)obj;
                    break;
                case "Double":
                    value = (double)obj;
                    break;
                case "Decimal":
                    value = ((decimal)obj).ToRedisValue();
                    break;
                case "Byte[]":
                    value = (byte[])obj;
                    break;
                case "DateTime":
                    value = ((DateTime)obj).Ticks;
                    break;
                case "Boolean":
                    value = (bool)obj;
                    break;
                case "Guid":
                    value = ((Guid)obj).ToString("N");
                    break;
                default:
                    if (type.IsEnum)
                    {
                        value = (byte)obj;
                    }
                    else
                    {
                        value = obj.ToString();
                    }
                    break;
            }
            return value;
        }

        /// <summary>
        /// 把Redis转成系统值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T GetRedisValue<T>(this RedisValue value)
        {
            if (value.IsNull) return default(T);
            return (T)value.GetRedisValue(typeof(T));
        }

        /// <summary>
        /// Redis值转化成为系统值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Object GetRedisValue(this RedisValue value, Type type)
        {
            Object obj;
            switch (type.Name)
            {
                case "Decimal":
                    obj = ((long)value).ToRedisValue();
                    break;
                case "Boolean":
                    obj = (bool)value;
                    break;
                case "DateTime":
                    obj = new DateTime((long)value);
                    break;
                case "Int32":
                    obj = (int)value;
                    break;
                case "Int16":
                    obj = (short)value;
                    break;
                case "Byte":
                    obj = (byte)value;
                    break;
                case "String":
                    obj = (string)value;
                    break;
                case "Guid":
                    obj = (Guid)Guid.Parse((string)value);
                    break;
                default:
                    if (type.IsEnum)
                    {
                        obj = (byte)value;
                    }
                    else
                    {
                        obj = value;
                    }
                    break;
            }
            return obj;
        }

        /// <summary>
        /// 整形与MONEY型互转（保留四位小数）
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static long ToRedisValue(this decimal money)
        {
            return (long)(money * 10000M);
        }

        /// <summary>
        /// 整形转为MONEY（保留四位小数）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ToRedisValue(this long value)
        {
            return (decimal)value / 10000M;
        }

        /// <summary>
        /// 整个对象赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static T GetRedisValue<T>(this IEnumerable<HashEntry> fields) where T : class, new()
        {
            T t = new T();
            foreach (HashEntry item in fields)
            {
                string name = (string)item.Name;
                if (name.Contains('.')) continue;
                PropertyInfo property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite) continue;
                property.SetValue(t, item.Value.GetRedisValue(property.PropertyType));
            }
            return t;
        }

        /// <summary>
        /// 把对象转化成Redis的Hash对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<HashEntry> GetRedisEntry<T>(this T obj) where T : class, new()
        {
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                string name = property.Name;
                object value = property.GetValue(obj);
                yield return new HashEntry(name, value.GetRedisValue());
            }
        }

        /// <summary>
        /// 把缓存值取出直接赋值给实体对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        public static T Fill<T>(this IEnumerable<HashEntry> fields) where T : IRedisBase, new()
        {
            T t = new T();
            t.Fill(fields);
            return t;
        }

        /// <summary>
        /// 从缓存读取出的对象赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="fields"></param>
        /// <param name="prefix">前缀名</param>
        public static void Fill<T>(this T t, IEnumerable<HashEntry> fields, string prefix = null) where T : IRedisBase, new()
        {
            foreach (HashEntry item in fields)
            {
                string name = (string)item.Name;
                if (!string.IsNullOrEmpty(prefix) && name.StartsWith(prefix + "."))
                {
                    name = name.Substring(prefix.Length + 1);
                }
                if (name.Contains('.')) continue;
                PropertyInfo property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite) continue;
                property.SetValue(t, item.Value.GetRedisValue(property.PropertyType));
            }
        }
    }
}
