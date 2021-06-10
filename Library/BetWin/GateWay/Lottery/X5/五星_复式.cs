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
    /// 五星 复式
    /// </summary>
    [Description("五星 五星直选 复式")]
    public class Player1 : IX5
    {
        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start5;
            }
        }

        /// <summary>
        /// 用竖线分组，逗号隔开号码。 要求
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1, 1, 1, 1 });
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
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[] resultNumber = number.Split(',');
            string[][] inputNumber = input.GetInputNumber();

            if (PlayerUtils.IsDuplex(inputNumber, resultNumber))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToDuplexList();
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
