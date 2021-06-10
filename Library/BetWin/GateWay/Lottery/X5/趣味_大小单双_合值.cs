using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 合值大小
    /// </summary>
    [BetChat(@"^和(?<Type>[大小单双])(?<Money>\d+)$")]
    public class Player181 : IX5
    {

        public override bool IsMatch(string input)
        {
            string[] ball = new string[] { "大", "小", "单", "双" };
            string[][] inputNumber = input.GetInputNumber(ball, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            if (!this.IsResult(number)) return decimal.Zero;

            int value = this.GetNumber(number).Select(t => int.Parse(t)).Sum();
            string[] inputNumber = input.GetInputNumber()[0];

            string[] result = new string[] { value >= 23 ? "大" : "小", value % 2 == 0 ? "双" : "单" };

            return result.Intersect(inputNumber).Count() * this.GetRewardMoney(rewardMoney);
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
