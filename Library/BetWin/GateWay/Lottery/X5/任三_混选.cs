using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 任三 混选
    /// </summary>
    public class Player100 : Player26
    {
        protected override NumberRange NumberType => NumberRange.Star5;

        public override int Bet(string input)
        {
            string flags = input.Split('*')[0];
            input = input.Split('*')[1];

            if (!Regex.IsMatch(flags, "^[万千百十个]{3,5}$")) return 0;
            if (flags.Distinct().Count() != flags.Length) return 0;

            int count = base.Bet(input);

            int flag = MathExtend.Combinations(3, flags.Length);
            return flag * count;
        }

        public override bool IsMatch(string input)
        {
            if (!input.Contains('*')) return false;
            input = input.Substring(input.IndexOf("*") + 1);
            return base.IsMatch(input);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            string flags = input.Split('*')[0];
            input = input.Split('*')[1];

            string[] resultNumber = this.GetNumber(number);
            string flag = "万千百十个";
            decimal reward = decimal.Zero;

            for (int i = 0; i < flags.Length; i++)
            {
                for (int n = i + 1; n < flags.Length; n++)
                {
                    for (int m = n + 1; m < flags.Length; m++)
                    {
                        string n1 = resultNumber[flag.IndexOf(flags[i])];
                        string n2 = resultNumber[flag.IndexOf(flags[n])];
                        string n3 = resultNumber[flag.IndexOf(flags[m])];

                        reward += base.Reward(input, string.Join(",", n1, n2, n3), rewardMoney);
                    }
                }
            }
            return reward;
        }
    }
}
