using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 两面 冠军
    /// </summary>
    [BetChat(@"^1(?<Type1>[大小])(?<Type2>[单双])(?<Money>\d+)$")]
    public class Player21 : IP10
    {
        public virtual int[] Index
        {
            get
            {
                return new int[] { 0 };
            }
        }

        public override string[] InputBall
        {
            get
            {
                return new string[] { "大", "小", "单", "双" };
            }
        }


        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 }, new int[] { 2, 2 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 }, new int[] { 2, 2 });
            return inputNumber[0].Length * inputNumber[1].Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = this.GetNumber(number);
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 }, new int[] { 2, 2 });

            int num = 0;
            foreach (int index in this.Index)
            {
                num += int.Parse(resultNumber[index]);
            }
            string result1 = num > 5 * this.Index.Length ? "大" : "小";
            string result2 = num % 2 == 0 ? "双" : "单";

            if (inputNumber[0].Contains(result1) && inputNumber[1].Contains(result2))
            {
                decimal zoom = decimal.One;
                switch (result1 + result2)
                {
                    case "小单":
                    case "大双":
                        zoom = 0.83M;
                        break;
                    case "小双":
                    case "大单":
                        zoom = 1.25M;
                        break;
                }
                return this.RewardMoney * zoom;
            }
            return decimal.Zero;
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = BetChatAttribute.Get(this);
            if (!betchat.IsMatch(content)) return null;
            times = betchat.GetTimes(content);
            return string.Format("{0}|{1}", betchat.GetValue(content, "Type1", string.Empty), betchat.GetValue(content, "Type2", string.Empty));
        }

        public override decimal RewardMoney
        {
            get
            {
                return 8M;
            }
        }
    }
}
