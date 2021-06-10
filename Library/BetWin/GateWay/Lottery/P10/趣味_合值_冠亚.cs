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
    /// 趣味 合值 冠亚合值
    /// </summary>
    [BetChat(@"^和(?<Number>((3/)|(4/)|(5/)|(6/)|(7/)|(8/)|(9/)|(10/)|(11/)|(12/)|(13/)|(14/)|(15/)|(16/)|(17/)|(18/)|(19/))+)(?<Money>\d+)$")]
    public class Player51 : IP10
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19" };
            }
        }
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { InputBall.Length }, false);
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return input.Split(',').Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { InputBall.Length }, false);
            int result = number.Split(',').Take(2).Select(t => int.Parse(t)).Sum();

            if (!inputNumber[0].Contains(result.ToString())) return decimal.Zero;
            Dictionary<int, int> dic = new Dictionary<int, int>();
            for (int n1 = 1; n1 <= 10; n1++)
            {
                for (int n2 = n1 + 1; n2 <= 10; n2++)
                {
                    int sum = n1 + n2;
                    if (dic.ContainsKey(sum))
                    {
                        dic[sum]++;
                    }
                    else
                    {
                        dic.Add(sum, 1);
                    }
                }
            }

            return this.GetRewardMoney(rewardMoney) / (decimal)dic[result];
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betChat = this.GetType().GetAttribute<BetChatAttribute>();
            if (betChat == null || !betChat.IsMatch(content)) return null;
            Regex regex = new Regex(betChat.Pattern);
            Match match = regex.Match(content);
            times = betChat.GetTimes(content);
            string number = match.Groups["Number"].Value;
            return string.Join(",", number.Split('/').Where(t => this.InputBall.Contains(t)));
        }

        /// <summary>
        /// 默认奖金 10*9
        /// </summary>
        public override decimal RewardMoney
        {
            get
            {
                return 90M;
            }
        }
    }
}
