using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前二 和值
    /// </summary>
    public class Player53 : IX5
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
            string[] ball = new string[19];
            for (int i = 0; i <= 18; i++) ball[i] = i.ToString();
            string[][] inputNumber = input.GetInputNumber(ball, new int[] { 1 }, new int[] { 19 });
            return inputNumber != null;

        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            int[] num = new int[] { 0, 0 };
            int value = 0;
            for (num[0] = 0; num[0] < 10; num[0]++)
            {
                for (num[1] = 0; num[1] < 10; num[1]++)
                {
                    if (inputNumber[0].Contains(num.Sum().ToString())) value++;
                }
            }
            return value;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string resultNumber = this.GetNumber(number).Select(t => int.Parse(t)).Sum().ToString();

            if (input.GetInputNumber()[0].Contains(resultNumber))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 200M;
            }
        }
    }
}
