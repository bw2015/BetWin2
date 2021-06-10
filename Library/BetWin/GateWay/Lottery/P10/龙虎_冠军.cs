using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 龙虎 冠军
    /// </summary>
    [BetChat(@"^1(?<Type>[龙虎])(?<Money>\d+)$")]
    public class Player41 : IP10
    {

        protected virtual int[] Index
        {
            get
            {
                return new int[] { 0 };
            }
        }

        public override string[] InputBall
        {
            get
            {
                return new string[] { "龙", "虎" };
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return 1;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 1 });
            string[] resultNumber = this.GetNumber(number);

            int num1 = 0;
            int num2 = 0;
            foreach (int index in this.Index)
            {
                num1 += int.Parse(resultNumber[index]);
                num2 += int.Parse(resultNumber[9 - index]);
            }
            if (num1 == num2) return decimal.Zero;
            string result = num1 > num2 ? "龙" : "虎";
            if (inputNumber[0].Contains(result)) return this.GetRewardMoney(rewardMoney);
            return decimal.Zero;
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = BetChatAttribute.Get(this);
            if (!betchat.IsMatch(content)) return null;

            times = betchat.GetTimes(content);
            return betchat.GetValue(content, "Type", string.Empty);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4M;
            }
        }
    }
}
