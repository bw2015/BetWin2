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
    /// 五星 组选60
    /// </summary>
    public class Player4 : IX5
    {
        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start5_Group;
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 3 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();

            string[] num1 = inputNumber[0];
            string[] num2 = inputNumber[1];

            int bet = 0;
            foreach (string n1 in num1)
            {
                string[] n2 = num2.Where(t => t != n1).ToArray();
                bet += MathExtend.Combinations(3, n2.Length);
            }
            return bet;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            Dictionary<int, string[]> dic = number.GetRepeaterNumber();
            if (dic.GetNumberLength(2) != 1 || dic.GetNumberLength(1) != 3) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();
            if (MathExtend.Intersect(dic[2], inputNumber[0]) == 1 && MathExtend.Intersect(dic[1], inputNumber[1]) == 3)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            string[][] inputNumber = input.GetInputNumber();
            List<string> list = new List<string>();
            foreach (string n1 in inputNumber[0])
            {
                string[] n2 = inputNumber[1].Where(t => t != n1).ToArray();
                if (n2.Length < 3) continue;

                foreach (string t in n2.ToGroupList(3))
                {
                    yield return string.Concat(string.Join(",", n1.RepeaterPadding(2)), ",", t);
                }
            }

        }

        public override decimal RewardMoney
        {
            get
            {
                return 3330M;
            }
        }
    }
}
