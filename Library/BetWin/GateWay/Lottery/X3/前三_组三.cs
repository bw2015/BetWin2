using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X3
{
    /// <summary>
    /// 三码 组三
    /// </summary>
    public class Player24 : BW.GateWay.Lottery.X5.Player24
    {
        public override LotteryCategory Type
        {
            get
            {
                return LotteryCategory.X3;
            }
        }

        protected override BW.GateWay.Lottery.X5.IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star31;
            }
        }
    }
}
