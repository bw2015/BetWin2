using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 趣味 一帆风顺
    /// </summary>
    public class Player83 : IX5
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.MinusOne;
            if (this.Bet(input) == 0) return decimal.Zero;

            IEnumerable<string> resultNumber = this.GetNumber(number, NumberRange.Star5).Distinct();
            string[][] inputNumber = input.GetInputNumber();

            return MathExtend.Intersect(inputNumber[0], resultNumber) * this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4.8M;
            }
        }
    }
}
