using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMService.Common
{
    /// <summary>
    /// 需要发送的数据
    /// </summary>
    public interface ISend
    {
    }

    public enum SendType
    {
        /// <summary>
        /// 信息
        /// </summary>
        Message,
        /// <summary>
        /// 上线
        /// </summary>
        Online,
        /// <summary>
        /// 下线
        /// </summary>
        Offline,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 信息提醒
        /// </summary>
        Tip,
        /// <summary>
        /// 提示
        /// </summary>
        Notify
    }
}
