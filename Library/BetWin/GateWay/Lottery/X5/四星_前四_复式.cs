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
    /// 四星 前四 复式
    /// </summary>
    public class Player111 : Player11
    {
        protected override NumberRange NumberType => NumberRange.Star41;

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[] resultNumber = this.GetNumber(number, this.NumberType);
            string[][] inputNumber = input.GetInputNumber();

            if (PlayerUtils.IsDuplex(inputNumber, resultNumber))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToDuplexList(false, true, true, true, true);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 20000M;
            }
        }
    }
}
