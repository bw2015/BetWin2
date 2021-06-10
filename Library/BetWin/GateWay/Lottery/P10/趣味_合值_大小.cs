using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 趣味 合值 冠亚大小
    /// </summary>
    [BetChat(@"^和(?<Type>[大小])(?<Money>\d+)$")]
    public class Player53 : Player11
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return input.Split(',').Length;
        }

        protected override string GetResult(int num)
        {
            return num > 11 ? "大" : "小";
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            string[] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 })[0];
            string result = this.GetResult(number.Split(',').Take(2).Select(t => int.Parse(t)).Sum());

            if (!inputNumber[0].Contains(result)) return decimal.Zero;
            decimal reward = this.GetRewardMoney(rewardMoney);

            if (result == "小")
            {
                reward *= 0.8M;
            }

            return reward;

        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betChat = this.GetType().GetAttribute<BetChatAttribute>();
            if (betChat == null || !betChat.IsMatch(content)) return null;
            Regex regex = new Regex(betChat.Pattern);
            Match match = regex.Match(content);
            times = betChat.GetTimes(content);
            return match.Groups["Type"].Value;
        }

        /// <summary>
        /// 默认奖金 10*9
        /// </summary>
        public override decimal RewardMoney
        {
            get
            {
                return 4.5M;
            }
        }
    }
}
