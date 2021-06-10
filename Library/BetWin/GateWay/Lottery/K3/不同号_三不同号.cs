using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 不同号 三不同号
    /// </summary>
    public class Player11 : IK3
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 3 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(3, inputNumber[0].Length);
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            Dictionary<int, string[]> dic = number.GetRepeaterNumber();
            if (!dic.ContainsKey(1) || dic[1].Length != 3) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();

            if (MathExtend.Intersect(inputNumber[0],dic[1]) == 3)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 72M;
            }
        }
    }
}
