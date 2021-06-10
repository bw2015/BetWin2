using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 不同号 三不同号
    /// </summary>
    public class Player12 : IK3
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 2 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return MathExtend.Combinations(2, inputNumber[0].Length);
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[] resultNumber = number.Split(',').Distinct().ToArray();
            if (resultNumber.Length < 2) return decimal.Zero;
            string[] inputNumber = input.GetInputNumber()[0];

            return MathExtend.Combinations(2, resultNumber.Intersect(inputNumber).Count()) * this.GetRewardMoney(rewardMoney);

        }

        public override decimal RewardMoney
        {
            get
            {
                return 14.4M;
            }
        }
    }
}
