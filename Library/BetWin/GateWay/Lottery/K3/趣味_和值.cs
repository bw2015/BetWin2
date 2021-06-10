using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 趣味 单号 和值
    /// 至少选择1个和值（3个号码之和）进行投注，所选和值与开奖的3个号码的和值相同即中奖。
    /// 如：选择3，开奖号码为 1,1,1（顺序不限），即为中奖。
    /// </summary>
    public class Player24 : IK3
    {
        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length;
        }

        public override bool IsMatch(string input)
        {
            string[] ball = new string[18];
            for (int i = 0; i < 18; i++) ball[i] = i.ToString();
            string[][] inputNumber = input.GetInputNumber(ball, new int[] { 1 }, new int[] { 18 });
            return inputNumber != null;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = 0)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;


            Dictionary<int, int> total = new Dictionary<int, int>();
            int count = 0;
            for (int i1 = 1; i1 <= 6; i1++)
            {
                for (int i2 = 1; i2 <= 6; i2++)
                {
                    for (int i3 = 1; i3 <= 6; i3++)
                    {
                        int sum = i1 + i2 + i3;
                        if (total.ContainsKey(sum))
                        {
                            total[sum]++;
                        }
                        else
                        {
                            total.Add(sum, 1);
                        }
                        count++;
                    }
                }
            }

            Dictionary<int, decimal> reward = total.ToDictionary(t => t.Key, t => (decimal)count / (decimal)t.Value * 2M);

            int resultNumber = number.Split(',').Select(t => int.Parse(t)).Sum();

            if (input.GetInputNumber()[0].Contains(resultNumber.ToString()))
            {
                return this.GetRewardMoney(reward[resultNumber]);
            }
            return decimal.Zero;
        }




        public override decimal RewardMoney
        {
            get
            {
                return 432M;
            }
        }
    }
}
