using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Array
{
    /// <summary>
    /// 数组管理的工具类
    /// </summary>
    public class ArrayAgent
    {
        /// <summary>
        /// 数组的笛卡尔乘积合并算法
        /// </summary>
        /// <param name="list">要合并的数组</param>
        /// <returns></returns>
        public static List<T>[] Descartes<T>(params List<T>[][] list)
        {
            var result = new List<List<T>>();
            list = list.Where(t => t != null).ToArray();
            if (list.Length == 1) return list[0];
            for (var i = 0; i < list.Length; i++)
            {
                foreach (var item in list[i])
                {
                    for (var i3 = i + 1; i3 < list.Length; i3++)
                    {
                        foreach (var obj in list[i3])
                        {
                            result.Add(item.Merge(t => t, obj));
                        }
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 多个字典合并
        /// 如果存在多个值则使用少数服从多数的算法
        /// </summary>
        /// <param name="dics"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Merger<TKey, TValue>(params Dictionary<TKey, TValue>[] dics)
        {
            List<TKey> list = new List<TKey>();
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();

            foreach (Dictionary<TKey, TValue> obj in dics)
            {
                foreach (TKey key in obj.Keys)
                {
                    if (!list.Contains(key)) list.Add(key);
                }
            }

            foreach (TKey key in list)
            {
                TValue value = dics.Where(t => t.ContainsKey(key)).Select(t => t[key]).GroupBy(t => t).Select(t => new { Value = t.Key, Count = t.Count() }).OrderByDescending(t => t.Count).Select(t => t.Value).FirstOrDefault();
                dic.Add(key, value);
            }

            return dic;
        }
    }
}
