using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;
using SP.Studio.Cache.Redis;

namespace SP.Studio.Cache.Redis
{
    public class RedisCacheBase<T> where T : class, new()
    {
        /// <summary>
        /// Redis缓存操作数据库对象
        /// </summary>
        private RedisManager db { get; }

        /// <summary>
        /// 默认的数据库
        /// </summary>
        protected virtual int DB_INDEX { get { return -1; } }

        protected IDatabase NewExecutor()
        {
            return this.db.NewExecutor(this.DB_INDEX);
        }

        protected ConnectionMultiplexer Connection()
        {
            return this.db.Instance();
        }

        public RedisCacheBase(string redisConnection)
        {
            this.db = new RedisManager(redisConnection);
        }

        /// <summary>
        /// 为了兼容阿里云Redis4.0不支持UNLINK命令的问题
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        protected virtual RedisResult KeyDelete(params RedisKey[] keys)
        {
            string[] args = keys.Select(t => t.ToString()).ToArray();
            return this.NewExecutor().Execute("DEL", args);
        }

        private static T _instance;
        /// <summary>
        /// 返回单例对象
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }
}
