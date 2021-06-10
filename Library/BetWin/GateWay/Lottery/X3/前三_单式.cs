using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X3
{
    /// <summary>
    /// 前三 单式
    /// </summary>
    public class Player22 : BW.GateWay.Lottery.X5.Player22
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

        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start3;
            }
        }
    }
}
