using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 排名竞猜    定位胆  单码
    /// </summary>
    [BetChat(@"^(?<Type>(1|2|3|4|5|6|7|8|9|10))/(?<Number>(1|2|3|4|5|6|7|8|9|10))/(?<Money>\d)$")]
    public class Player105 : Player4
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            return inputNumber.Sum(t => t.Length);
        }

        protected override string[] GetNumber(string number)
        {
            return number.Split(',').ToArray();
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

            string[] inputNumber = new string[10];
            int index = 1;
            

            switch (type)
            {
                case "亚":
                    index = 2;
                    break;
                case "冠":
                    index = 1;
                    break;
                default:
                    break;
            }

            return string.Join("|", inputNumber);
        }
    }
}
