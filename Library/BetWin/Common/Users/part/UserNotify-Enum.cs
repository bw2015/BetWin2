using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace BW.Common.Users
{
    partial class UserNotify
    {
        public UserNotify() { }

        public UserNotify(DataRow dr)
        {
            this.ID = (int)dr["NotifyID"];
            this.UserID = (int)dr["UserID"];
            this.Message = (string)dr["Message"];
        }

        public enum NotifyType : byte
        {
            /// <summary>
            /// 彩票派奖
            /// </summary>
            [Description("彩票")]
            Lottery,
            /// <summary>
            /// 充值到账
            /// </summary>
            [Description("充值")]
            Recharge,
            /// <summary>
            /// 活动
            /// </summary>
            [Description("活动")]
            Plan
        }
    }
}
