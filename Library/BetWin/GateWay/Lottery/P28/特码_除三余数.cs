using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P28
{
    /// <summary>
    /// 特码 除三余数
    /// </summary>
    public class Player12 : IP28
    {
        public override string[] InputBall => new string[]{ "0", "1", "2" };

        public override int Bet(string input)
        {
            return input.Split(',').Distinct().Count();
        }

        public override bool IsMatch(string input)
        {
            return !input.Split(',').Any(t => !this.InputBall.Contains(t));
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0 || this.IsResult(number)) return decimal.Zero;

            int result = WebAgent.GetArray<int>(number).LastOrDefault();
            if (WebAgent.GetArray<int>(input).Contains(result)) return this.GetRewardMoney(rewardMoney);
            return decimal.Zero;
        }

        public override decimal RewardMoney => 6M;
    }
}
