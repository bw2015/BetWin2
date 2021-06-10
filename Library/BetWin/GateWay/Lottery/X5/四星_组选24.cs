using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 组选24
    /// </summary>
    public class Player13 : IX5
    {

        protected override NumberRange NumberType => NumberRange.Star4;

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return MathExtend.Combinations(4, input.Split(',').Length);
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });

            string[] resultNumber = this.GetNumber(number, NumberType);

            if (resultNumber.Distinct().Count() != 4) return decimal.Zero;
            if (MathExtend.Intersect(resultNumber, inputNumber[0]) == 4)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;

        }

        public override decimal RewardMoney
        {
            get
            {
                return 833M;
            }
        }
    }
}
