using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 两面  冠亚季军
    /// </summary>
    [BetChat(@"^123(?<Type1>[大小])(?<Type2>[单双])(?<Money>\d+)$")]
    public class Player32 : Player21
    {
        public override int[] Index
        {
            get
            {
                return new int[] { 0, 1, 2 };
            }
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number) || this.Bet(input) == 0) return decimal.Zero;

            int num = number.Split(',').Select(t => int.Parse(t)).Take(3).Sum();

            string result1 = num > 16 ? "大" : "小";
            string result2 = num % 2 == 0 ? "双" : "单";

            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 1 }, new int[] { 2, 2 });

            if (inputNumber[0].Contains(result1) && inputNumber[1].Contains(result2))
            {
                decimal zoom = decimal.One;
                string result = result1 + result2;
                if (result == "小双" || result == "大单") zoom = 0.81M;
                return this.RewardMoney * zoom;
            }
            return decimal.Zero;
        }

        public override decimal RewardMoney
        {
            get
            {
                return 8.88M;
            }
        }
    }
}
