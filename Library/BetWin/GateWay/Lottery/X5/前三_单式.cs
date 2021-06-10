using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前三 单式
    /// </summary>
    public class Player22 : IX5
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

        /// <summary>
        /// 用竖线分组，逗号隔开号码。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 3);
            if (inputNumber == null) return false;
            if (inputNumber.Distinct().Count() != inputNumber.Length) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return input.Count(t => t == '|') + 1;
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
            return input.ToSingleList(true, true, true, false, false);
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
