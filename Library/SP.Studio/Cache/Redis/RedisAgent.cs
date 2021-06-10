using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace SP.Studio.Cache.Redis
{
    /// <summary>
    /// Redis的缓存代理操作方法
    /// </summary>
    public class RedisManager
    {
        private static ConnectionMultiplexer instance;
        private static readonly object locker = new object();

        public RedisManager(string connection)
        {
            lock (locker)
            {
                if (instance == null)
                {
                    if (instance == null)
                        instance = ConnectionMultiplexer.Connect(connection);
                }
            }
        }

        public ConnectionMultiplexer Instance()
        {
            return instance;
        }

        public virtual IDatabase NewExecutor(int db = -1)
        {
            return this.Instance().GetDatabase(db);
        }
    }
}
