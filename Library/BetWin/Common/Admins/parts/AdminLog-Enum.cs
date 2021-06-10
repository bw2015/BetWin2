using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace BW.Common.Admins
{
    /// <summary>
    /// 日志枚举
    /// </summary>
    partial class AdminLog
    {
        public enum LogType : byte
        {
            [Description("登录")]
            Login = 1,
            [Description("用户操作")]
            User = 2,
            [Description("资金操作")]
            Money = 3,
            [Description("彩票操作")]
            Lottery = 4,
            [Description("站点设定")]
            Site = 5,
            [Description("资料修改")]
            Info = 6,
            [Description("微信设置")]
            Wechat = 7
        }
    }
}
