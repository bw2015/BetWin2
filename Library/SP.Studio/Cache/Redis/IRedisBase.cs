using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using StackExchange.Redis;

namespace SP.Studio.Cache.Redis
{
    /// <summary>
    /// Redis缓存对象的基类
    /// </summary>
    public interface IRedisBase
    {

        /// <summary>
        /// 转化成为Redis实体
        /// </summary>
        /// <returns></returns>
        IEnumerable<HashEntry> ToHashEntry();

        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="fields"></param>
        void FillHashEntry(IEnumerable<HashEntry> fields);
    }
}
