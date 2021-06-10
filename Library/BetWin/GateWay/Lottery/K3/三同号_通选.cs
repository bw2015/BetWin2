using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 三同号 通选
    /// </summary>
    public class Player2 : IK3
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "通选" };
            }
        }
        public override bool IsMatch(string input)
        {
            return !string.IsNullOrEmpty(input) && InputBall.Contains(input);
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return 1;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            IEnumerable<string> resultNumber = number.Split(',').Distinct();
            if (resultNumber.Count() != 1) return decimal.Zero;

            return this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 72M;
            }
        }
    }
}
