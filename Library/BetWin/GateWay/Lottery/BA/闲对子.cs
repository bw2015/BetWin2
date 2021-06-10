using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.BA
{
    /// <summary>
    /// 闲 对子
    /// </summary>
    public class Player5 : IBA
    {
        public override int Bet(string input)
        {
            return this.IsMatch(input) ? 1 : 0;
        }

        public override bool IsMatch(string input)
        {
            return input == "闲对";
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            int[] result = this.GetResultNumber(number);
            if (result[0] == result[1]) return this.GetRewardMoney(rewardMoney);
            return decimal.Zero;
        }

        /// <summary>
        /// 奖金
        /// </summary>
        public override decimal RewardMoney => 16.00M;
    }
}
