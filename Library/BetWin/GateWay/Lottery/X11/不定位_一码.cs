using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 一码不定位
    /// </summary>
    public class Player41 : IX11
    {
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

            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Intersect(inputNumber[0], number.Split(',')) * this.GetRewardMoney(rewardMoney);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4.4M;
            }
        }
    }
}
