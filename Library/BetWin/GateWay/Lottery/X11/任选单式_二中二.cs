using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    public class Player62 : IX11
    {
        /// <summary>
        /// 任选的长度
        /// </summary>
        protected virtual int Length
        {
            get
            {
                return 2;
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, this.Length);
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber().Select(t => t.OrderBy(p => p).ToArray()).ToArray();
            if (inputNumber.Distinct().Count() != inputNumber.Length) return 0;

            return inputNumber.Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = number.Split(',');
            string[][] inputNumber = input.GetInputNumber();
            decimal reward = decimal.Zero;
            foreach (string[] num in inputNumber)
            {
                if (MathExtend.Intersect(num, resultNumber) == Math.Min(this.Length, Math.Max(this.Length, 5)))
                {
                    reward += this.GetRewardMoney(rewardMoney);
                }
            }
            return reward;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 11.00M;
            }
        }
    }
}
