using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.M6
{
    /// <summary>
    /// 趣味  两面
    /// </summary>
    public class Player3 : IM6
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "大", "小", "和大", "和小", "单", "双", "和单", "和双", "大肖", "小肖", "尾大", "尾小" };
            }
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;
            string resultNumber = this.GetNumber(number).LastOrDefault();
            int num = int.Parse(resultNumber);

            List<string> result = new List<string>();
            if (num != 49)
            {
                result.Add(num >= 25 ? "大" : "小");
                result.Add(num % 2 == 1 ? "单" : "双");
                result.Add(Array.IndexOf(this.Lunar, this.GetLunar(resultNumber)) > 5 ? "大肖" : "小肖");
                result.Add(num / 10 + num % 10 >= 7 ? "和大" : "和小");
                result.Add((num / 10 + num % 10) % 2 == 1 ? "和单" : "和双");
                result.Add(num % 10 >= 5 ? "大" : "小");
            }

            if (result.Contains(input)) return this.GetRewardMoney(rewardMoney);

            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 3.8M;
            }
        }
    }
}
