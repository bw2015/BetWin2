using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 五星 组选120
    /// </summary>
    public class Player3 : IX5
    {
        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start5_Group;
            }
        }

        public override bool IsMatch(string input)
        {
            string[] number = input.Split(',');
            int inputLength = number.Length;
            number = number.Distinct().ToArray();
            if (number.Length != inputLength) return false;
            if (number.Where(t => !this.InputBall.Contains(t)).Count() != 0) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            return MathExtend.Combinations(5, input.Split(',').Length);
        }

       public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;
            if (number.Split(',').Distinct().Count() != 5) return decimal.Zero;
            if (MathExtend.Intersect(input.Split(','), number.Split(',')) == 5)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            string[] number = input.Split(',');
            return number.ToGroupList(5);
        }

        public override decimal RewardMoney
        {
            get
            {
                return 1665M;
            }
        }
    }
}
