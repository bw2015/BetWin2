using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X3
{
    /// <summary>
    /// 胆码 定位胆
    /// </summary>
    public class Player71 : IPlayer
    {
        public override LotteryCategory Type
        {
            get { return LotteryCategory.X3; }
        }

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 0, 0, 0 });
            if (inputNumber == null) return false;
            if (inputNumber.Sum(t => t.Length) == 0) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            return inputNumber.Sum(t => t.Length);
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();
            string[] resultNumber = number.Split(',');

            decimal reward = decimal.Zero;
            for (int i = 0; i < resultNumber.Length; i++)
            {
                if (inputNumber[i].Contains(resultNumber[i])) reward += this.RewardMoney;
            }
            return reward;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 20.00M;
            }
        }
    }
}
