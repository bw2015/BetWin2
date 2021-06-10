using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 趣味 大小单双 前二
    /// </summary>
    public class Player81 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star21;
            }
        }

        public override bool IsMatch(string input)
        {
            string[] ball = new string[] { "大", "小", "单", "双" };
            string[][] inputNumber = input.GetInputNumber(ball, new int[] { 1, 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length * inputNumber[1].Length;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] resultNumber = this.GetNumber(number).Select(t => new string[] { int.Parse(t) > 4 ? "大" : "小", int.Parse(t) % 2 == 1 ? "单" : "双" }).ToArray();
            string[][] inputNumber = input.GetInputNumber();

            int[] success = new int[2];
            for (int i = 0; i < success.Length; i++)
            {
                success[i] = MathExtend.Intersect(inputNumber[i], resultNumber[i]);
            }
            return success[0] * success[1] * this.GetRewardMoney(rewardMoney);

        }

        public override decimal RewardMoney
        {
            get
            {
                return 8;
            }
        }
    }
}
