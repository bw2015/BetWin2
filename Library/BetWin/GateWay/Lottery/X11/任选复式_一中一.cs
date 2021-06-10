using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 一中一
    /// </summary>
    public class Player51 : IX11
    {
        /// <summary>
        /// 任选的数量
        /// </summary>
        protected virtual int Length
        {
            get
            {
                return 1;
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { Length });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(Length, inputNumber[0].Length);
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();
            string[] resultNumber = number.Split(',');

            int count = MathExtend.Intersect(inputNumber[0], resultNumber);

            if (this.Length < 6)
            {
                return MathExtend.Combinations(this.Length, count) * this.GetRewardMoney(rewardMoney);
            }
            else if (count == 5)
            {
                return this.GetRewardMoney(rewardMoney);
            }

            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4.33M;
            }
        }
    }
}
