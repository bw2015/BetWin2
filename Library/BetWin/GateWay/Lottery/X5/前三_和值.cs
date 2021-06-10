using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前三 和值
    /// </summary>
    public class Player23 : IX5
    {

        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star31;
            }
        }

        /// <summary>
        /// 用竖线分组，逗号隔开号码。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool IsMatch(string input)
        {
            string[] inputBall = new string[28];
            for (int i = 0; i <= 27; i++) inputBall[i] = i.ToString();

            string[][] inputNumber = input.GetInputNumber(inputBall, new int[] { 1 }, new int[] { inputBall.Length });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            int value = 0;
            int[] number = new int[3];
            for (number[0] = 0; number[0] < 10; number[0]++)
            {
                for (number[1] = 0; number[1] < 10; number[1]++)
                {
                    for (number[2] = 0; number[2] < 10; number[2]++)
                    {
                        if (inputNumber[0].Contains(number.Sum().ToString())) value++;
                    }
                }
            }
            return value;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string resultNumber = this.GetNumber(number).Sum(t => int.Parse(t)).ToString();
            string[][] inputNumber = input.GetInputNumber();

            if (inputNumber[0].Contains(resultNumber))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 2000.00M;
            }
        }
    }
}
