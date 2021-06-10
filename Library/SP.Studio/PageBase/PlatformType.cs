using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SP.Studio.PageBase
{
    /// <summary>
    /// 平台类型
    /// </summary>
    [Flags]
    public enum PlatformType : byte
    {
        [Description("网页端")]
        PC = 1,
        [Description("微信端")]
        Wechat = 2,
        [Description("WAP")]
        Wap = 4,
        [Description("IOS")]
        IOS = 8,
        [Description("Andoird")]
        Android = 16,
        /// <summary>
        /// 移动端的统称
        /// </summary>
        [Description("移动端")]
        Mobile = 30
    }
}
