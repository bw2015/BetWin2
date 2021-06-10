using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.Common.Lottery.Limited
{
    /// <summary>
    /// 限号的数据模型
    /// </summary>
    public class LimitedNumber
    {
        public LimitedNumber(decimal bet, decimal reward)
        {
            this.Bet = bet;
            this.Reward = reward;
        }

        /// <summary>
        /// 投注金额
        /// </summary>
        public decimal Bet;

        /// <summary>
        /// 可得奖奖金
        /// </summary>
        public decimal Reward;
    }
}
