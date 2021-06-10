using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SP.Studio.Core;
using SP.Studio.Web;

namespace SP.Studio.Array
{
    /// <summary>
    /// 哈希表的扩展类
    /// </summary>
    public static class HashtableExtendsions
    {
        public static T GetValue<T>(this Hashtable ht, object key, T defaultValue)
        {
            if (!ht.ContainsKey(key)) return defaultValue;
            if (typeof(T) == typeof(decimal))
            {
                if (ht[key].ToString().StartsWith(".")) ht[key] = "0" + ht[key];
            }
            string value = ht[key].ToString();
            if (!WebAgent.IsType<T>(value)) return defaultValue;
            return value.GetValue<T>();
        }
    }
}
