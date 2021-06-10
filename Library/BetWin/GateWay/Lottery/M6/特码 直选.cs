using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.M6
{
    /// <summary>
    /// 特码 直选
    /// </summary>
    public class Player1 : IM6
    {
       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = this.GetNumber(number);
            if (resultNumber.LastOrDefault() == input)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 90M;
            }
        }
    }
}
