using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 胆码 定位胆
    /// </summary>
    [BetChat(@"^(?<Index>[1-5]{1,5})/(?<Number>[0-9]{1,10})/(?<Money>\d+)$")]
    public class Player71 : IX5
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0 });
            if (inputNumber == null) return false;
            if (inputNumber.Sum(t => t.Length) == 0) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber.Sum(t => t.Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();
            string[] resultNumber = this.GetNumber(number, NumberRange.Star5);

            decimal reward = decimal.Zero;
            for (int i = 0; i < resultNumber.Length; i++)
            {
                if (inputNumber[i].Contains(resultNumber[i])) reward += this.GetRewardMoney(rewardMoney);
            }
            return reward;
        }

        /// <summary>
        /// 微信投注
        /// 1/0123456789/100
        /// </summary>
        /// <param name="content"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = BetChatAttribute.Get(this);
            if (!betchat.IsMatch(content)) return null;

            times = betchat.GetTimes(content);
            string[] list = new string[] { "", "", "", "", "" };
            string index = betchat.GetValue(content, "Index", string.Empty);
            string number = string.Join(",", betchat.GetValue(content, "Number", string.Empty).Distinct().Select(t => t));
            foreach (char i in index.Distinct())
            {
                int _index = int.Parse(i.ToString()) - 1;
                list[_index] = number;
            }
            return string.Join("|", list);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 20;
            }
        }
    }
}
