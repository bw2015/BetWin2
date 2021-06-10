using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 趣味 大小
    /// </summary>
    [BetChat(@"^(?<Number>(1|2|3|4|5|6|7|8|9|10))(?<Type>[大小])(?<Money>\d+)$")]
    public class Player11 : IP10
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "大", "小" };
            }
        }

        protected virtual string GetResult(int num)
        {
            return num > 5 ? "大" : "小";
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            return inputNumber.Sum(t => t.Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();
            string[] resultNumber = this.GetNumber(number);
            decimal reward = decimal.Zero;
            for (int index = 0; index < inputNumber.Length; index++)
            {
                int num = int.Parse(resultNumber[index]);
                string result = this.GetResult(num);
                if (inputNumber[index].Contains(result))
                {
                    reward += this.GetRewardMoney(rewardMoney);
                }
            }
            return reward;
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = BetChatAttribute.Get(this);
            if (betchat == null) return null;

            times = betchat.GetTimes(content);
            int index = betchat.GetValue(content, "Number", -1);
            string type = betchat.GetValue(content, "Type", string.Empty);
            if (index == -1 || string.IsNullOrEmpty(type)) return null;

            string[] number = new string[10];
            number[index - 1] = type;
            return string.Join("|", number);
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
