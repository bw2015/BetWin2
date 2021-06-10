using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后二 和值
    /// </summary>
    public class Player63 : Player53
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star22;
            }
        }
    }
}
