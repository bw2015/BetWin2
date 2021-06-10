using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.M6
{
    /// <summary>
    /// 趣味 生肖
    /// </summary>
    public class Player2 : IM6
    {
        public override string[] InputBall
        {
            get
            {
                return this.Lunar;
            }
        }

        /// <summary>
        /// 中奖奖金（如果为当年生肖则打八折）
        /// </summary>
        /// <param name="input"></param>
        /// <param name="number"></param>
        /// <returns></returns>
       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;

            string[] resultNumber = this.GetNumber(number);
            string result = this.GetLunar(resultNumber.LastOrDefault());
            if (result == input)
            {
                if (input == this.Lunar[this.LunarIndex])
                {
                    return this.GetRewardMoney(rewardMoney) * 0.8M;
                }
                else
                {
                    return this.GetRewardMoney(rewardMoney);
                }
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 11.25M;
            }
        }
    }
}
