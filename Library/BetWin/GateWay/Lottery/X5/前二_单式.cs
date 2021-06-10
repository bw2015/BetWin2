using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前二 单式
    /// </summary>
    public class Player52 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star21;
            }
        }

        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start2;
            }
        }

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
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            if (this.IsSingleReward(input, number))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToSingleList(true, true, false, false, false);
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
