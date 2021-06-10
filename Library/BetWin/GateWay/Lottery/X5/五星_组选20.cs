using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using SP.Studio.Array;
using SP.Studio.Text;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 五星 组选20
    /// </summary>
    public class Player6 : IX5
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 2 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();

            string[] num1 = inputNumber[0];
            string[] num2 = inputNumber[1];

            var value = 0;
            foreach (string n1 in num1)
            {
                string[] n2 = num2.Where(t => t != n1).ToArray();
                value += MathExtend.Combinations(2, n2.Length);
            }
            return value;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            Dictionary<int, string[]> dic = number.GetRepeaterNumber();

            if (dic.GetNumberLength(3) != 1 || dic.GetNumberLength(1) != 2) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();
            if (MathExtend.Intersect(dic[3], inputNumber[0]) == 1 && MathExtend.Intersect(dic[1], inputNumber[1]) == 2)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            string[][] inputNumber = input.GetInputNumber();
            foreach (string n1 in inputNumber[0])
            {
                string[] n2 = inputNumber[1].Where(t => t != n1).ToArray();
                if (n2.Length < 2) continue;
                foreach (string t in n2.ToGroupList(2))
                {
                    yield return string.Concat(string.Join(",", n1.RepeaterPadding(3)), ",", t);
                }
            }
        }

        public override decimal RewardMoney
        {
            get
            {
                return 9990;
            }
        }
    }
}
