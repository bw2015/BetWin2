using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    public class Player14 : IP28
    {
        public override string[] InputBall => new string[] { "极大", "极小" };


        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;

            int num = int.Parse(number.Split(',').LastOrDefault());
            string result = null;
            if (num >= 22) result = "极大";
            if (num <= 5) result = "极小";
            if (string.IsNullOrEmpty(result)) return decimal.Zero;
            return this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney => 35.6M;
    }
}
