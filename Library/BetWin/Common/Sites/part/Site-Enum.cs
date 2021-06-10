using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点的枚举
    /// </summary>
    partial class Site
    {
        /// <summary>
        /// 站点状态
        /// </summary>
        public enum SiteStatus : byte
        {
            [Description("正常")]
            Normal = 0,
            [Description("停止")]
            Stop = 1,
            /// <summary>
            /// 逾期没有缴纳佣金，系统将自动把状态设置成为过期
            /// </summary>
            [Description("过期")]
            Expire = 2
        }
    }
}
