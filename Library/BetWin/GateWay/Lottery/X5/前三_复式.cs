using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前三 复式
    /// </summary>
    public class Player21 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star31;
            }
        }

        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start3;
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1, 1 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            int bet = 1;
            foreach (string[] num in inputNumber) bet *= num.Length;
            return bet;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();
            string[] resultNumber = this.GetNumber(number);

            if (PlayerUtils.IsDuplex(inputNumber, resultNumber))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToDuplexList(true, true, true, false, false);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 2000M;
            }
        }
    }
}
