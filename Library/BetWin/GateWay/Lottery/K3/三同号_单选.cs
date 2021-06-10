using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 三同号 单选
    /// </summary>
    public class Player1 : IK3
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1 });
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber[0].Length;
        }


       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            IEnumerable<string>  resultNumber = number.Split(',').Distinct();
            if(resultNumber.Count() != 1) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();
            if (inputNumber[0].Contains(resultNumber.First()))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 432.00M;
            }
        }
    }
}
