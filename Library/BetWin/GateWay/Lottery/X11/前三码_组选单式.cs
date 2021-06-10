using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 前三码 组选单式
    /// </summary>
    public class Player34 : IX11
    {

        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 3, false, true);
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            string[][] inputNumber = input.GetInputNumber();
            if (inputNumber.Distinct().Count() != inputNumber.Length) return 0;
            return inputNumber.Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.Zero;
            if (this.Bet(input) == 0) return decimal.Zero;
            string[][] inputNumber = input.GetInputNumber();
            string resultNumber = string.Join(",", number.Split(',').Take(3).OrderBy(t => t));

            if (inputNumber.Where(t => string.Join(",", t.OrderBy(p => p)) == resultNumber).Count() != 0)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 330M;
            }
        }
    }
}
