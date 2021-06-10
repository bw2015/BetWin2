using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 三连号
    /// </summary>
    public class Player13 : IK3
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

            Dictionary<int, string[]> dic = number.GetRepeaterNumber();
            if (!dic.ContainsKey(1) || dic[1].Length != 3) return decimal.Zero;

            IEnumerable<int> resultNumber = dic[1].Select(t => int.Parse(t));

            if (resultNumber.Max() - resultNumber.Min() == 2) return this.GetRewardMoney(rewardMoney);
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 18.00M;
            }
        }
    }
}
