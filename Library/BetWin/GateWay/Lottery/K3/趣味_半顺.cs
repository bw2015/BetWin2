using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.K3
{
    public class Player25 : Player2
    {
        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int[] num = number.Split(',').Select(t => int.Parse(t)).OrderBy(t => t).ToArray();

            if ((num[1] - num[0] == 1 || num[2] - num[1] == 1) && (num[2] - num[0] != 2) && num.Distinct().Count() == 3)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney => 6M;
    }
}
