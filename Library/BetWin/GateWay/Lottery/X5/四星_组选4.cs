using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 组选4
    /// </summary>
    public class Player16 : IX5
    {

        protected override NumberRange NumberType => NumberRange.Star41;

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;

            string[][] inputNumber = input.GetInputNumber();
            int value = 0;
            foreach (string n1 in inputNumber[0])
            {
                value += MathExtend.Combinations(1, inputNumber[1].Count(t => !t.Contains(n1)));
            }
            return value;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 });
            string[] resultNumber = this.GetNumber(number, NumberType);

            Dictionary<int, string[]> dic = resultNumber.GetRepeaterNumber();

            if (dic.GetNumberLength(3) != 1 || dic.GetNumberLength(1) != 1) return decimal.Zero;

            if (MathExtend.Intersect(dic[3], inputNumber[0]) == 1 && MathExtend.Intersect(dic[1], inputNumber[1]) == 1)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;

        }

        public override decimal RewardMoney
        {
            get
            {
                return 5000M;
            }
        }
    }
}
