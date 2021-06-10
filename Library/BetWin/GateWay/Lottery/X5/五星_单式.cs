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
    /// 五星 单式
    /// </summary>
    public class Player2 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star5;
            }
        }
        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start5;
            }
        }

        /// <summary>
        /// 号码用逗号隔开，整个注数用|隔开
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 5);
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return input.GetInputNumber().Length;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.IsSingleReward(input, number))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToSingleList();
        }

        public override decimal RewardMoney
        {
            get
            {
                return 200000;
            }
        }

    }
}
