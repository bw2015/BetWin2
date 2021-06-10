using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Cache.Redis
{
    /// <summary>
    /// Redis处理之后的状态
    /// </summary>
    public enum RedisResultStatus
    {
        /// <summary>
        /// 数据不存在
        /// </summary>
        NoExists,
        /// <summary>
        /// 额度不足
        /// </summary>
        NoEnough,
        /// <summary>
        /// 处理成功
        /// </summary>
        Success,
        /// <summary>
        /// 处理失败
        /// </summary>
        Faild
    }
}
