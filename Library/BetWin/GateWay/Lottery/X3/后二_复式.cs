using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X3
{
    /// <summary>
    /// 后二 复式
    /// </summary>
    public class Player61 : BW.GateWay.Lottery.X5.Player51
    {
        public override LotteryCategory Type
        {
            get
            {
                return LotteryCategory.X3;
            }
        }

        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start2;
            }
        }
        protected override BW.GateWay.Lottery.X5.IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star22;
            }
        }
    }
}
