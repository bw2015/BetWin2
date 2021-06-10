using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SP.Studio.Core;

namespace BW.Common.Users
{
    /// <summary>
    /// 资金枚举
    /// </summary>
    partial class MoneyLock
    {
        /// <summary>
        /// 锁定类型
        /// </summary>
        public enum LockType : byte
        {
            [Description("提现")]
            Withdraw = 1,
            /// <summary>
            /// 彩票投注订单
            /// 投注的时候锁定金额，开奖或者撤单的时候解锁
            /// </summary>
            [Description("彩票投注")]
            LotteryOrder = 2,
            /// <summary>
            /// 与第三方游戏的账户资金转账
            /// </summary>
            [Description("账户转账")]
            Transfer = 3,
            /// <summary>
            /// 提现手续费
            /// </summary>
            [Description("提现手续费")]
            WithdrawFee = 4,
            /// <summary>
            /// 追号
            /// </summary>
            [Description("彩票追号")]
            LotteryChase = 5,
            /// <summary>
            /// 参与彩票合买
            /// </summary>
            [Description("彩票合买")]
            LotteryUnited = 6,
            /// <summary>
            /// 保底金额
            /// </summary>
            [Description("彩票合买保底")]
            LotteryUnitedPackage = 7,
            /// <summary>
            /// 扩展类库的定义方法
            /// </summary>
            [Description("其他类型")]
            Other = 100,
            /// <summary>
            /// 理财排队
            /// </summary>
            [Description("理财排队")]
            LendingQueue = 101

        }

        public string TypeValue
        {
            get
            {
                return this.Type.GetDescription();
            }
        }
    }
}
