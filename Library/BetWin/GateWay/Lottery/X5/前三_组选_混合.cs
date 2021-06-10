using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 前三 组选 混合组选
    /// </summary>
    public class Player26 : IX5
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star31;
            }
        }
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 3, true, true);
            return inputNumber != null;
        }

        public override int Bet(string input)
        {
            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 3, true, true);
            foreach (string[] number in inputNumber)
            {
                if (number.Distinct().Count() == 1) return 0;
            }
            return inputNumber.Length;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.NumberType != NumberRange.Star5 && this.Bet(input) == 0) return decimal.Zero;
            if (this.NumberType != NumberRange.Star5 && !this.IsResult(number)) return decimal.Zero;

            string[][] inputNumber = input.GetSingleInputNumber(this.InputBall, 3, true, true);
            
            string[] resultNumber = this.GetNumber(number).OrderBy(t => t).ToArray();
            string result = string.Join(string.Empty, resultNumber);

            // 计算是组三还是组六
            int numberLength = resultNumber.Distinct().Count();
            if (numberLength == 1) return decimal.Zero;

            decimal reward = decimal.Zero;
            foreach (string[] num in inputNumber)
            {
                if (result == string.Join(string.Empty, num.OrderBy(t => t)))
                {
                    if (numberLength == 2)
                    {
                        reward += this.GetRewardMoney(rewardMoney);
                    }
                    else if (numberLength == 3)
                    {
                        reward += this.GetRewardMoney(rewardMoney) * 0.5M;
                    }
                    break;
                }
            }
            return reward;
        }

        /// <summary>
        /// 组三的奖金（组六为50%）
        /// </summary>
        public override decimal RewardMoney
        {
            get
            {
                return 666M;
            }
        }
    }
}
