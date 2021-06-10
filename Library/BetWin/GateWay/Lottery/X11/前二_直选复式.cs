using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 前二 直选 复式
    /// </summary>
    public class Player11 : IX11
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            int bet = 0;
            foreach (string number in inputNumber[0])
            {
                bet += inputNumber[1].Count(t => t != number);
            }
            return bet;

        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = number.Split(',');

            string[][] inputNumber = input.GetInputNumber();
            if (inputNumber[0].Contains(resultNumber[0]) && inputNumber[1].Contains(resultNumber[1]))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 220;
            }
        }
    }
}
