using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    public class Player31 : IX11
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1, 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            int bet = 0;
            foreach (string num1 in inputNumber[0])
            {
                foreach (string num2 in inputNumber[1].Where(t => t != num1))
                {
                    bet += inputNumber[2].Count(t => t != num1 && t != num2);
                }
            }
            return bet;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();
            if (PlayerUtils.IsDuplex(inputNumber, number.Split(',').Take(3).ToArray()))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 1980;
            }
        }
    }
}
