using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    public class Player12 : IX11
    {

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 2);
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            if (inputNumber.Distinct().Count() != inputNumber.Length) return 0;
            return inputNumber.Length;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = number.Split(',');
            string[][] inputNumber = input.GetInputNumber();
            foreach (string[] num in inputNumber)
            {
                if (num[0] == resultNumber[0] && num[1] == resultNumber[1]) return this.GetRewardMoney(rewardMoney);
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
