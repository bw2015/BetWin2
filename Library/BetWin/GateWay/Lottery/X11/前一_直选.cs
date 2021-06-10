using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 前一码 直选
    /// </summary>
    public class Player1 : IX11
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 11 });
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
            string[][] inputNumber = input.GetInputNumber();
            string resultNumber = number.Split(',').First();

            if (inputNumber[0].Contains(resultNumber))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 22;
            }
        }


    }
}
