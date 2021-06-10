using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Cache
{
    /// <summary>
    /// 缓存操作基类
    /// </summary>
    public interface ICacheBase
    {
        /// <summary>
        /// 更新缓存值
        /// </summary>
        /// <returns></returns>
        bool UpdateCache<T>(params Expression<Func<T, object>>[] fields) where T : ICacheBase, new();
    }
}
