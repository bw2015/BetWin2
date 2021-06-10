using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前二 复式
    /// </summary>
    public class Player51 : IX5
    {
        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start2;
            }
        }
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star21;
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 });
            return inputNumber != null;

        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length * inputNumber[1].Length;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            if (PlayerUtils.IsDuplex(input.GetInputNumber(), this.GetNumber(number)))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToDuplexList(true, true, false, false, false);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 200M;
            }
        }
    }
}
