using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SP.Studio.Core;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 大小单双 定位胆
    /// </summary>
    [BetChat(@"^(?<Index>[12345]{1,5})(?<Type>[大小单双]{1,4})(?<Money>\d+)$")]
    public class Player182 : IX5
    {
        public override bool IsMatch(string input)
        {
            return
            input.GetInputNumber(new string[] { "大", "小", "单", "双" }, new int[] { 0, 0, 0, 0, 0 }, new int[] { 4, 4, 4, 4, 4 }) != null;
        }

        public override int Bet(string input)
        {
            string[][] inputNumber = input.GetInputNumber(new string[] { "大", "小", "单", "双" }, new int[] { 0, 0, 0, 0, 0 }, new int[] { 4, 4, 4, 4, 4 });
            return inputNumber.Sum(t => t.Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            if (!this.IsResult(number)) return decimal.Zero;

            string[][] result =
            number.Split(',').Select(t => int.Parse(t)).Select(t => new string[]{
                t>=5 ? "大" : "小",
                t%2==1? "单" : "双"
            }).ToArray();
            string[][] inputNumber = input.GetInputNumber(new string[] { "大", "小", "单", "双" }, new int[] { 0, 0, 0, 0, 0 }, new int[] { 4, 4, 4, 4, 4 });

            decimal reward = decimal.Zero;
            for (int index = 0; index < result.Length; index++)
            {
                reward += this.GetRewardMoney(rewardMoney) * MathExtend.Intersect(result[index], inputNumber[index]);
            }
            return reward;
        }

        public override string GetBetChat(string content, out int times)
        {
            BetChatAttribute betchat = this.GetType().GetAttribute<BetChatAttribute>();
            times = betchat.GetTimes(content);

            string index = betchat.GetValue(content, "Index", string.Empty);
            string type = betchat.GetValue(content, "Type", string.Empty);
            if (index.Distinct().Count() != index.Length || type.Distinct().Count() != type.Length) return null;

            string[] result = new string[] { "", "", "", "", "" };

            foreach (char i in index)
            {
                result[int.Parse(i.ToString()) - 1] = string.Join(",", type.Select(t => t));
            }

            return string.Join("|", result);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 4.0M;
            }
        }
    }
}
