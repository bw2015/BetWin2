using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 二同号 单选
    /// </summary>
    public class Player3 : IK3
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            int bet = 0;
            foreach (string n1 in inputNumber[0])
            {
                bet += inputNumber[1].Count(t => t != n1);
            }
            return bet;
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            Dictionary<int, string[]> dic = number.GetRepeaterNumber();
            if (!dic.ContainsKey(2) || !dic.ContainsKey(1)) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();

            if (inputNumber[0].Contains(dic[2].First()) && inputNumber[1].Contains(dic[1].First()))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 144M;
            }
        }
    }
}
