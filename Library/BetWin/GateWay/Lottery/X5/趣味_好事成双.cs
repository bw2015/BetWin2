using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 趣味 好事成双
    /// </summary>
    public class Player84 : Player83
    {
       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (!this.IsResult(number)) return decimal.MinusOne;
            if (this.Bet(input) == 0) return decimal.Zero;

            Dictionary<int, string[]> dic = number.GetRepeaterNumber();
            if (dic.GetNumberLength(1) == 5) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber();

            IEnumerable<string> resultNumber = this.GetNumber(number, NumberRange.Star5).Where(p => dic.Where(t => t.Key > 1 && t.Value.Contains(p)).Count() > 0);

            return MathExtend.Intersect(inputNumber[0], resultNumber) * this.GetRewardMoney(rewardMoney);
        }


        public override decimal RewardMoney
        {
            get
            {
                return 24.1M;
            }
        }
    }
}
