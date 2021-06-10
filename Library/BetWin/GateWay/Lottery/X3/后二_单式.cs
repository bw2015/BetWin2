using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X3
{
    /// <summary>
    /// 后二 单式
    /// </summary>
    public class Player62 : BW.GateWay.Lottery.X5.Player52
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
                return NumberRange.Star22;
            }
        }

        public override LimitedType Limited
        {
            get
            {
                return LimitedType.X5_Start2;
            }
        }
    }
}
