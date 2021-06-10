using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    ///  开奖号码中有三个相连的号码（仅限：123、234、345、456）开出即中奖。
    ///  如：开奖号码：234，即中全顺。
    /// </summary>
    public class Player26 : Player2
    {


        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int[] num = number.Split(',').Select(t => int.Parse(t)).OrderBy(t => t).ToArray();
            bool isQS = num[1] - num[0] == 1 && num[2] - num[1] == 1 && num[2] - num[0] == 2;
            if (!isQS) return decimal.Zero;

            return this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney => 18M;
    }
}
