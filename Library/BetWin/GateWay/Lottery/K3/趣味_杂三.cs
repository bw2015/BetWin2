using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    ///  开奖号码中有三个不相连的号码（仅限：135.136.146.246）开出即中奖。
    ///  如：开奖号码：135，即中杂三。
    /// </summary>
    public class Player27 : Player2
    {
        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int[] num = number.Split(',').Select(t => int.Parse(t)).OrderBy(t => t).ToArray();
            bool isZ3 = num[1] - num[0] > 1 && num[2] - num[1] > 1;
            if (!isZ3) return decimal.Zero;

            return this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney => 18M;
    }
}
