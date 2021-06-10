using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

using SP.Studio.Core;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前三 组六
    /// </summary>
    [BetChat(@"^前组六/(?<Number>[0-9]{3,10})/(?<Money>\d+)$")]
    public class Player25 : IX5
    {

        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star31;
            }
        }

        /// <summary>
        /// 用竖线分组，逗号隔开号码。
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 3 }, new int[] { 10 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(3, inputNumber[0].Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[] resultNumber = this.GetNumber(number);
            Dictionary<int, string[]> dic = resultNumber.GetRepeaterNumber();
            if (dic.GetNumberLength(1) != 3) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();

            if (MathExtend.Intersect(inputNumber[0], dic[1]) == 3)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 333M;
            }
        }

        public override string GetBetChat(string content, out int times)
        {
            BetChatAttribute betchat = this.GetType().GetAttribute<BetChatAttribute>();
            times = betchat.GetTimes(content);

            string number = betchat.GetValue(content, "Number", string.Empty);
            char[] num = number.Distinct().OrderBy(t => t).ToArray();
            if (num.Length < 3) return null;

            return string.Join(",", num);
        }
    }
}
