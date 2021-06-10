using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 任二 单式
    /// 万千百十个*1,2,3,4,5,6,7,8,9
    /// </summary>
    public class Player93 : IX5
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

            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 2);
            if (inputNumber == null) return 0;

            int flag = MathExtend.Combinations(2, flags.Length);
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
                    string n1 = resultNumber[flag.IndexOf(flags[i])];
                    string n2 = resultNumber[flag.IndexOf(flags[n])];
                    
                    if (inputNumber.Where(t => t[0] == n1 && t[1] == n2).Count() != 0)
                    {
                        reward += this.GetRewardMoney(rewardMoney);
                    }
                }
            }
            return reward;
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
