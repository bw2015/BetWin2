using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    public class Player13 : IP28
    {
        public override string[] InputBall => new string[] { "大", "小", "单", "双", "大单", "小单", "大双", "小双" };

     


        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int num = int.Parse(number.Split(',').LastOrDefault());
            string[] inputNumber = input.Split(',');

            string result1 = num > 13 ? "大" : "小";
            string result2 = num % 2 == 0 ? "双" : "单";

            decimal reward = decimal.Zero;

            decimal rate = this.GetRewardMoney(rewardMoney) / this.RewardMoney;
            if (inputNumber.Contains(result1)) reward += this.rewardMoney[result1] * rate;
            if (inputNumber.Contains(result2)) reward += this.rewardMoney[result2] * rate;
            if (inputNumber.Contains(result1 + result2)) reward += this.rewardMoney[result1 + result2] * rate;
            return reward;
        }

        public override decimal RewardMoney => 8.6M;

        private Dictionary<string, decimal> rewardMoney = new Dictionary<string, decimal>()
        {
            {"大",4M },
            {"小",4M },
            {"单",4M },
            {"双",4M },
            {"大单",8.6M },
            {"小单",7.4M },
            {"大双",7.4M },
            {"小双",8.6M }
        };
    }
}
