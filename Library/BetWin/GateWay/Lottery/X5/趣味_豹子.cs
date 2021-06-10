using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 豹子
    /// </summary>
    [BetChat(@"^(?<Type>[前中后])豹(?<Money>\d+)$")]
    public class Player184 : IX5
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "前", "中", "后" };
            }
        }
        public override bool IsMatch(string input)
        {
            return input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 3 }) != null;
        }

        public override int Bet(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 3 });
            if (inputNumber == null) return 0;

            return inputNumber[0].Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0 || !this.IsResult(number)) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 }, new int[] { 3 });
            int[] resultNumber = number.Split(',').Select(t => int.Parse(t)).ToArray();

            List<string> result = new List<string>();
            for (int i = 0; i < this.InputBall.Length; i++)
            {
                if (resultNumber[i] == resultNumber[i + 1] && resultNumber[i + 1] == resultNumber[i + 2]) result.Add(this.InputBall[i]);
            }

            return MathExtend.Intersect(result, inputNumber[0]) * this.GetRewardMoney(rewardMoney);
        }

        public override string GetBetChat(string content, out int times)
        {
            times = 0;
            BetChatAttribute betchat = this.GetType().GetAttribute<BetChatAttribute>();
            times = betchat.GetTimes(content);
            return betchat.GetValue(content, "Type", string.Empty);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 200M;
            }
        }
    }
}
