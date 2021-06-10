using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 二同号 通选
    /// </summary>
    public class Player5 : Player2
    {

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;

            string[] result = number.Split(',');
            if (result.Distinct().Count() == 2)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney => 4.8M;
    }
}
