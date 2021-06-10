using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 单式
    /// </summary>
    public class Player12 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star4;
            }
        }
        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start4;
            }
        }

        /// <summary>
        /// 号码用逗号隔开，整个注数用|隔开
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 4);
            if (inputNumber == null) return false;
            if (inputNumber.Distinct().Count() != inputNumber.Length) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return input.Split('|').Length;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;

            if (this.IsSingleReward(input, number))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToSingleList(false, true, true, true);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 20000;
            }
        }
    }
}
