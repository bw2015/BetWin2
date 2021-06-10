using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Web;
using System.Web.Caching;

namespace SP.Studio.Cache
{
    /// <summary>
    /// 缓存代理类
    /// </summary>
    public class CacheAgent
    {
        /// <summary>
        /// 获取缓存（如果缓存不存在则执行委托方法生成
        /// </summary>
        /// <param name="key">关键词KEY</param>
        /// <param name="func">用于生成内容的KEY</param>
        /// <param name="time">缓存有效时间 默认为10分钟</param>
        /// <param name="dependencies">缓存依赖条件</param>
        /// <returns></returns>
        public static T GetCache<T>(string key, Func<object[], T> func, object[] args, int time = 600, bool isAbsolute = true, CacheDependency dependencies = null)
        {
            if (HttpRuntime.Cache[key] != null)
                return (T)HttpRuntime.Cache[key];
            if (func == null) return default(T);
            T t = func.Invoke(args);
            HttpRuntime.Cache.Insert(key, t, dependencies,
                isAbsolute ? DateTime.UtcNow.AddSeconds(time) : System.Web.Caching.Cache.NoAbsoluteExpiration,
                isAbsolute ? System.Web.Caching.Cache.NoSlidingExpiration : TimeSpan.FromSeconds(time));
            return t;
        }
    }
}
