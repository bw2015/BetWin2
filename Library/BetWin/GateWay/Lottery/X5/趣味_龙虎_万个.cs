using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 龙虎
    /// </summary>
    [BetChat(@"^(?<Type>[龙虎和])(?<Money>\d+)$")]
    public class Player183 : IX5
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "龙", "虎", "和" };
            }
        }
        public override bool IsMatch(string input)
        {
            return input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 3 }) != null;
        }

        public override int Bet(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 3 });
            if (inputNumber == null) return 0;

            return inputNumber[0].Length;
        }

        protected virtual int[] NumberIndex
        {
            get
            {
                return new int[] { 0, 4 };
            }
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 3 });
            int[] resultNumber = number.Split(',').Select(t => int.Parse(t)).ToArray();
            decimal reward = decimal.Zero;
            string result = "和";
            decimal zoom = 4.5M;
            // 是否设定开和退本金
            bool isReturn = this.GetRewardMoney(rewardMoney) <= 4.00M;
            if (isReturn) zoom = 4.0M;

            if (resultNumber[NumberIndex[0]] > resultNumber[NumberIndex[1]])
            {
                result = "龙";
                zoom = decimal.One;
            }
            else if (resultNumber[NumberIndex[0]] < resultNumber[NumberIndex[1]])
            {
                result = "虎";
                zoom = decimal.One;
            }

            if (inputNumber[0].Contains(result))
            {
                reward = this.GetRewardMoney(rewardMoney) * zoom;
            }
            else if (isReturn && result == "和")
            {
                reward = RETURNMONEY;
            }

            return reward;
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = this.GetType().GetAttribute<BetChatAttribute>();
            times = betchat.GetTimes(content);
            return betchat.GetValue(content, "Type", string.Empty);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4.4M;
            }
        }
    }
}
