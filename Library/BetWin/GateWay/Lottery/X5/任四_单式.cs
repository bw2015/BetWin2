using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 任四 单式
    /// 万千百十个*1,2,3,4|5,6,7,8
    /// </summary>
    public class Player99 : IX5
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

            if (!Regex.IsMatch(flags, "^[万千百十个]{2,5}$")) return 0;
            if (flags.Distinct().Count() != flags.Length) return 0;

            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 4);
            if (inputNumber == null) return 0;

            int flag = MathExtend.Combinations(4, flags.Length);
            return flag * inputNumber.Length;
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
                        for (int k = m + 1; k < flags.Length; k++)
                        {
                            string n1 = resultNumber[flag.IndexOf(flags[i])];
                            string n2 = resultNumber[flag.IndexOf(flags[n])];
                            string n3 = resultNumber[flag.IndexOf(flags[m])];
                            string n4 = resultNumber[flag.IndexOf(flags[k])];

                            if (inputNumber.Where(t => t[0] == n1 && t[1] == n2 && t[2] == n3 && t[3] == n4).Count() != 0)
                            {
                                reward += this.GetRewardMoney(rewardMoney);
                            }
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
                return 20000M;
            }
        }
    }
}
