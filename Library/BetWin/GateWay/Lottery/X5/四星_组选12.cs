﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 组选12
    /// 2个重号，2个单号
    /// </summary>
    public class Player14 : IX5
    {
        protected override NumberRange NumberType => NumberRange.Star4;
        public override bool IsMatch(string input)
        {
            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 2 });
            if (inputNumber == null) return false;
            return true;
        }

        public override int Bet(string input)
        {
            if (!this.IsMatch(input)) return 0;
            int value = 0;
            string[][] inputNumber = input.GetInputNumber();

            foreach (string n1 in inputNumber[0])
            {
                value += MathExtend.Combinations(2, inputNumber[1].Count(t => !t.Contains(n1)));
            }
            return value;
        }

        public override decimal Reward(string input, string number, decimal rewardMoney = decimal.Zero)
        {
            if (this.Bet(input) == 0) return decimal.Zero;

            string[][] inputNumber = input.GetInputNumber(this.InputBall, new int[] { 1, 2 });
            string[] resultNumber = this.GetNumber(number, this.NumberType);

            Dictionary<int, string[]> dic = resultNumber.GetRepeaterNumber();

            if (dic.GetNumberLength(2) != 1 || dic.GetNumberLength(1) != 2) return decimal.Zero;

            if (MathExtend.Intersect(dic[2], inputNumber[0]) == 1 && MathExtend.Intersect(dic[1], inputNumber[1]) == 2)
            {
                return this.GetRewardMoney(rewardMoney);
            }
            return decimal.Zero;

        }

        public override decimal RewardMoney
        {
            get
            {
                return 1666M;
            }
        }
    }
}
