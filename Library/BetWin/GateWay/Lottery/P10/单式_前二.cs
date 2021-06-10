using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Text.RegularExpressions;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 排名竞猜  单式  前二
    /// </summary>
    [BetChat(@"^12/(?<N1>(1|2|3|4|5|6|7|8|9|10))(?<N2>(1|2|3|4|5|6|7|8|9|10))/(?<Money>\d+)$")]
    public class Player6 : IP10
    {
        /// <summary>
        /// 单式的长度
        /// </summary>
        protected virtual int SingleLength
        {
            get
            {
                return 2;
            }
        }


        /// <summary>
        /// 前
        /// </summary>
        protected virtual bool Before
        {
            get
            {
                return true;
            }
        }

        public override bool IsMatch(string input)
        {
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, this.SingleLength, false);
            return inputNumber == null ? 0 : inputNumber.Length;
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
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, this.SingleLength, false);

            if (this.Before)
            {
                resultNumber = resultNumber.Take(this.SingleLength).ToArray();
            }
            else
            {
                resultNumber = resultNumber.Skip(10 - this.SingleLength).ToArray();
            }
            foreach (string[] num in inputNumber)
            {
                bool isReward = true;
                for (int index = 0; index < this.SingleLength; index++)
                {
                    if (num[index] != resultNumber[index])
                    {
                        isReward = false;
                        break;
                    }
                }
                if (isReward) return this.RewardMoney;
            }

            return decimal.Zero;
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betChat = BetChatAttribute.Get(this);
            if (betChat == null) return null;
            times = betChat.GetTimes(content);
            List<string> number = new List<string>();
            for (int i = 1; i <= this.SingleLength; i++)
            {
                number.Add(betChat.GetValue(content, "N" + i, string.Empty).PadLeft(2, '0'));
            }
            return string.Join(",", number);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 180M;
            }
        }
    }
}
