using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Data
{
    /// <summary>
    /// 查询时候的锁定级别
    /// </summary>
    public enum LockType
    {
        /// <summary>
        /// 会把被锁住的行不显示出来
        /// </summary>
        READPAST,
        /// <summary>
        /// 可能把没有提交事务的数据也显示出来
        /// </summary>
        NOLOCK
    }
}
