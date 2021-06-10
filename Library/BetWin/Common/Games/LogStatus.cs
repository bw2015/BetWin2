using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;

namespace BW.Common.Games
{
    /// <summary>
    /// 游戏的状态
    /// </summary>
    public enum LogStatus : byte
    {
        /// <summary>
        /// 未结算状态（正在游戏中，或者体育游戏尚未开奖）
        /// </summary>
        [Description("未结算")]
        None = 0,
        /// <summary>
        /// 投注完成，已派奖
        /// </summary>
        [Description("已结算")]
        Finish = 1,
        /// <summary>
        /// 重置试玩额度
        /// </summary>
        [Description("重置试玩额度")]
        Reset = 2,
        /// <summary>
        /// 注单被篡改
        /// </summary>
        [Description("注单被篡改")]
        Error = 3,
        /// <summary>
        /// 被系统撤单
        /// </summary>
        [Description("取消指定局注单")]
        Cancel = 8,
        /// <summary>
        /// 玩家取消
        /// </summary>
        [Description("取消注单")]
        Quit = 9
    }
}
