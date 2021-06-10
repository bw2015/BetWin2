using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    public class Player21 : IP28
    {
        public override string[] InputBall => new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int[] result = WebAgent.GetArray<int>(number);
            int[] inputNumber = WebAgent.GetArray<int>(input).Take(3).ToArray();
            decimal reward = decimal.Zero;
            foreach (int inputNum in inputNumber)
            {
                int count = result.Count(t => t == inputNum);
                if (this.rewardMoney.ContainsKey(count))
                {
                    reward += this.rewardMoney[count] * this.GetRewardMoney(rewardMoney) / this.RewardMoney;
                }
            }
            return reward;
        }

        public override decimal RewardMoney => 200M;

        private Dictionary<int, decimal> rewardMoney = new Dictionary<int, decimal>()
        {
            {1,4M },
            {2,20M },
            {3,200M }
        };
    }
}
