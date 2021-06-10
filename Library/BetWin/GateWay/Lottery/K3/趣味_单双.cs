using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 趣味 单双
    /// </summary>
    public class Player22 : IK3
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "单", "双" };
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            return !this.IsMatch(input) ? 0 : 1;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string result = number.Split(',').Select(t => int.Parse(t)).Sum() % 2 == 0 ? "双" : "单";
            if (input == result)
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
