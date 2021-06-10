using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using SP.Studio.Web;

namespace BW.Common.Users
{
    partial class UserInvite
    {
        public enum InviteStatus : byte
        {
            /// <summary>
            /// 正常
            /// </summary>
            [Description("正常")]
            Normal = 0,
            /// <summary>
            /// 停止
            /// </summary>
            [Description("停止")]
            Stop = 1
        }
    }
}
