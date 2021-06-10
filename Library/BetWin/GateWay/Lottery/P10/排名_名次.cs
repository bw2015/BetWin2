using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// Player101 Category="排名竞猜" Label="排名" Name="名次"
    /// </summary>
    /// 12345/2/2
    [BetChat(@"^(?<Index>(1|2|3|4|5|6|7|8|9|10){2,})/(?<Number>(1|2|3|4|5|6|7|8|9|10))/(?<Money>\d+)$")]
    public class Player101 : IP10
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
            }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();

            return inputNumber.Sum(t => t.Length);
        }

        /// <summary>
        /// 1,2,3,4,5|||||||||
        /// 出现在第一道上面的是 1、2、3、4、5几号
        /// 
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <param name="rewardMoney"></param>
        /// <returns></returns>
        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();
            List<string> resultNumber = number.Split(',').ToList();

            decimal reward = decimal.Zero;
            int count = 0;
            foreach (string[] index in inputNumber)
            {
                count++;
                // 当前车号所在的名次
                string result = (resultNumber.IndexOf(count.ToString().PadLeft(2, '0')) + 1).ToString();
                if (index.Contains(result))
                {
                    reward += this.GetRewardMoney(rewardMoney);
                }
            }

            return reward;
        }

        /// <summary>
        /// 微信投注
        /// </summary>
        /// <param name="content"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = BetChatAttribute.Get(this);
            if (betchat == null) return null;

            times = betchat.GetTimes(content);
            // 车号
            int car = betchat.GetValue(content, "Number", 0);
            // 名次
            string index = betchat.GetValue(content, "Index", string.Empty);

            if (car == 0 || string.IsNullOrEmpty(index)) return null;

            List<string> num = new List<string>();

            Regex regex = new Regex(@"(10)|(1)|(2)|(3)|(4)|(5)|(6)|(7)|(8)|(9)");
            regex.Replace(index, m =>
             {
                 num.Add(m.Value);
                 return string.Empty;
             });

            string[] number = new string[10];
            number[car - 1] = string.Join(",", num);
            return string.Join("|", number);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 20M;
            }
        }
    }
}
