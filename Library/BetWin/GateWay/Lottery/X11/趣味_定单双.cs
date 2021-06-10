using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 趣味 定单双
    /// </summary>
    public class Player71 : IX11
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "5单0双", "4单1双", "3单2双", "2单3双", "1单4双", "0单5双" };
            }
        }
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            int[] resultNumber = number.Split(',').Select(t => int.Parse(t)).ToArray();
            Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
            dic.Add("0单5双", 1M);
            dic.Add("1单4双", 0.0333M);
            dic.Add("2单3双", 0.0066M);
            dic.Add("3单2双", 0.005M);
            dic.Add("4单1双", 0.0133M);
            dic.Add("5单0双", 0.1666M);

            string result = string.Format("{0}单{1}双", resultNumber.Count(t => t % 2 == 1), resultNumber.Count(t => t % 2 == 0));
            string[][] inputNumber = input.GetInputNumber();

            if (inputNumber[0].Contains(result) && dic.ContainsKey(result))
            {
                return this.GetRewardMoney(rewardMoney) * dic[result];
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 924M;
            }
        }
    }
}
