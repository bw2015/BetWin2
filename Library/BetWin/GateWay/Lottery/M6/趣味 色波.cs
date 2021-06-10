using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.M6
{
    /// <summary>
    /// 趣味 色波
    /// </summary>
    public class Player4 : IM6
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "红", "蓝", "绿" };
            }
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;

            string num = this.GetNumber(number).LastOrDefault();
            string result = string.Empty;
            int index = 0;
            foreach (string[] ball in new string[][] { this.Ball_Red, this.Ball_Blue, this.Ball_Green })
            {
                if (ball.Contains(num))
                {
                    result = this.InputBall[index];
                    continue;
                }
                index++;
            }

            if (result == input)
            {
                return result == "红" ? Math.Round(this.GetRewardMoney(rewardMoney) * 0.9411M, 2) : this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }


        public override decimal RewardMoney
        {
            get
            {
                return 5.625M;
            }
        }
    }
}
