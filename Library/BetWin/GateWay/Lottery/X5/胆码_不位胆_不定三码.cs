using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 胆码 不位胆 后三二码
    /// </summary>
    public class Player78 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star5;
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 3 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(3, inputNumber[0].Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[] inputNumber = input.GetInputNumber()[0];
            string[] resultNumber = this.GetNumber(number).Distinct().ToArray();

            int count = MathExtend.Intersect(inputNumber, resultNumber);
            return MathExtend.Combinations(3, count) * this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 45.97M;
            }
        }
    }
}
