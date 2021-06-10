using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    public class Player13 : IX11
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 2 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;

            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(2, inputNumber[0].Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();
            string[] resultNumber = number.Split(',').Take(2).ToArray();

            if (MathExtend.Intersect(inputNumber[0], resultNumber) == 2)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 110;
            }
        }
    }
}
