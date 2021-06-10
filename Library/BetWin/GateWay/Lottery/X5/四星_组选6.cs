using BW.Common.Lottery;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 组选6
    /// </summary>
    public class Player15 : IX5
    {
        protected override NumberRange NumberType => NumberRange.Star4;

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 2 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(2, inputNumber[0].Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 2 });
            string[] resultNumber = this.GetNumber(number, NumberType);

            Dictionary<int, string[]> dic = resultNumber.GetRepeaterNumber();

            if (dic.GetNumberLength(2) != 2) return decimal.Zero;

            if (MathExtend.Intersect(dic[2], inputNumber[0]) == 2)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;

        }

        public override decimal RewardMoney
        {
            get
            {
                return 3333M;
            }
        }
    }
}
