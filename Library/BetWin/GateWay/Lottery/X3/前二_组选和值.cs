using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X3
{
    /// <summary>
    /// 前二 组选和值
    /// </summary>
    public class Player56 : BW.GateWay.Lottery.X5.Player56
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
                return NumberRange.Star21;
            }
        }
    }
}
