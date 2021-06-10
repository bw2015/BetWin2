using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

using SP.Studio.Core;

namespace BW.Common.Users
{
    partial class ChatTalk
    {
        /// <summary>
        /// 用户与管理员、游客与管理员、用户与用户、管理员与管理员
        /// </summary>
        public enum TalkType : byte
        {
            [Description("未指定类型")]
            None,
            [Description("用户与管理员")]
            AdminUser,
            [Description("游客与管理员")]
            AdminGuest,
            [Description("用户与用户")]
            User2User,
            [Description("管理员与管理员")]
            Admin2Admin,
            /// <summary>
            /// 群聊
            /// </summary>
            [Description("群聊")]
            Group
        }

        /// <summary>
        /// 群类型
        /// </summary>
        public enum GroupType : byte
        {
            [Description("北京赛车")]
            PK10 = BW.Common.Lottery.LotteryType.PK10,
            [Description("香港赛马")]
            HKSM = BW.Common.Lottery.LotteryType.HKSM,
            [Description("重庆时时彩")]
            ChungKing = BW.Common.Lottery.LotteryType.ChungKing,
            [Description("幸运飞艇")]
            Boat = BW.Common.Lottery.LotteryType.Boat,
            [Description("新疆时时彩")]
            Sinkiang = BW.Common.Lottery.LotteryType.Sinkiang
        }
    }
}
