using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.BA
{
    /// <summary>
    /// 闲赢
    /// </summary>
    public class Player2 : IBA
    {
        public override int Bet(string input)
        {
            return this.IsMatch(input) ? 1 : 0;
        }

        public override bool IsMatch(string input)
        {
            return input == "闲赢";
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            int[] result = this.GetResultNumber(number);
            int 闲 = (result[0] + result[1]) % 10;
            int 庄 = (result[2] + result[3]) % 10;
            if (庄 == 闲) return RETURNMONEY;
            if (庄 < 闲) return this.GetRewardMoney(rewardMoney);
            return decimal.Zero;
        }

        /// <summary>
        /// 奖金
        /// </summary>
        public override decimal RewardMoney => 4.00M;
    }
}
