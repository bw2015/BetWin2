using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 任二 组三
    /// 万千百十个*1,2,3,4,5,6,7,8,9
    /// 开奖号码中有两个号码相同一个不相同
    /// </summary>
    public class Player95 : IX5
    {
        public override bool IsMatch(string input)
        {
            return input.Contains('*');
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;

            string flags = input.Split('*')[0];
            input = input.Split('*')[1];

            if (!Regex.IsMatch(flags, "^[万千百十个]{3,5}$")) return 0;
            if (flags.Distinct().Count() != flags.Length) return 0;

            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 10 });
            if (inputNumber == null) return 0;

            int flag = MathExtend.Combinations(3, flags.Length);
            int bet = MathExtend.Combinations(2, inputNumber[0].Length) * 2;
            return flag * bet;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;

            string flags = input.Split('*')[0];
            string[][] inputNumber = input.Split('*')[1].GetInputNumber();

            string[] resultNumber = number.Split(',');
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

                        IEnumerable<string> result = new string[] { n1, n2, n3 }.Distinct();
                        if (result.Count() != 2) continue;

                        if (inputNumber[0].Intersect(result).Count() == 2)
                        {
                            reward += this.GetRewardMoney(rewardMoney);
                        }
                    }

                }
            }
            return reward;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 666M;
            }
        }
    }
}
