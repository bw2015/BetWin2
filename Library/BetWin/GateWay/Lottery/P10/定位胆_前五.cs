using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 排名竞猜    定位胆  前五
    /// </summary>
    [BetChat(@"^(?<Type>[冠亚季12345])/(?<Number>(1|2|3|4|5|6|7|8|9|10)+)/(?<Money>\d+)$|^(?<Number>(1|2|3|4|5|6|7|8|9|10)+)/(?<Money>\d+)$")]
    public class Player4 : IP10
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0 });
            return inputNumber.Sum(t => t.Length);
        }

        /// <summary>
        /// 奖金
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = this.GetNumber(number);
            string[][] inputNumber = input.GetInputNumber();

            decimal reward = decimal.Zero;
            for (int index = 0; index < inputNumber.Length; index++)
            {
                if (inputNumber[index].Contains(resultNumber[index]))
                {
                    reward += this.RewardMoney;
                }
            }

            return reward;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 20M;
            }
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = this.GetType().GetAttribute<BetChatAttribute>();

            if (!betchat.IsMatch(content)) return null;

            Match match = betchat.Regex.Match(content);
            string type = match.Groups["Type"].Value;
            string number = match.Groups["Number"].Value;
            times = betchat.GetTimes(content);

            List<string> num = new List<string>();

            Regex regex = new Regex(@"(10)|(1)|(2)|(3)|(4)|(5)|(6)|(7)|(8)|(9)");
            regex.Replace(number, m =>
            {
                num.Add(m.Value);
                return string.Empty;
            });

            if (num.GroupBy(t => t).Select(t => t.Count()).Count(t => t > 1) != 0)
            {
                return null;
            }

            number = string.Join(",", num.Select(t => t.PadLeft(2, '0')));

            string[] inputNumber = new string[] { "", "", "", "", "" };
            if (string.IsNullOrEmpty(type)) type = "1";
            switch (type)
            {
                case "冠":
                case "1":
                case "6":
                    inputNumber[0] = number;
                    break;
                case "亚":
                case "2":
                case "7":
                    inputNumber[1] = number;
                    break;
                case "季":
                case "3":
                case "8":
                    inputNumber[2] = number;
                    break;
                case "4":
                case "9":
                    inputNumber[3] = number;
                    break;
                case "5":
                case "10":
                    inputNumber[4] = number;
                    break;
            }

            return string.Join("|", inputNumber);
        }
    }
}
