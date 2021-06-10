using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 趣味 大小
    /// </summary>
    public class Player21 : IK3
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "大", "小" };
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            return !this.IsMatch(input)  ? 0 : 1;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            int resultNumber = number.Split(',').Select(t => int.Parse(t)).Sum();
            if ((input == "大" && resultNumber > 10) || (input == "小" && resultNumber <= 10))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4;
            }
        }
    }
}
