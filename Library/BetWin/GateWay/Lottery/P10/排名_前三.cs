using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 排名竞猜    排名  前三
    /// </summary>
    public class Player3 : IP10
    {
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1, 1 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            int bet = 0;
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1, 1 });
            foreach (string n1 in inputNumber[0])
            {
                foreach (string n2 in inputNumber[1].Where(t => t != n1))
                {
                    bet += inputNumber[2].Count(t => t != n1 && t != n2);
                }
            }
            return bet;
        }

        /// <summary>
        /// 奖金
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string[] resultNumber = this.GetNumber(number);
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1, 1 });

            if (inputNumber[0].Contains(resultNumber[0]) && inputNumber[1].Contains(resultNumber[1]) && inputNumber[2].Contains(resultNumber[2]))
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 1440M;
            }
        }
    }
}
