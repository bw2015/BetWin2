using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    public class Player16 : IP28
    {
        public override string[] InputBall => new string[] { "大边", "中边", "小边" };
        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            int num = WebAgent.GetArray<int>(number).Last();

            string result = null;
            if (num <= 9)
            {
                result = this.InputBall[0];
            }
            else if (num <= 17)
            {
                result = this.InputBall[1];
            }
            else
            {
                result = this.InputBall[2];
            }
            decimal reward = 0;
            if (input.Split(',').Contains(result)) reward = this.rewardMoney[result] * this.GetRewardMoney(rewardMoney) / this.RewardMoney;
            return reward;
        }

        public override decimal RewardMoney => 9M;

        private Dictionary<string, decimal> rewardMoney = new Dictionary<string, decimal>()
        {
            {"大边",9M },
            {"中边",3.4M },
            {"小边",9M }
        };
    }
}
