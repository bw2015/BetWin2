using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 组选6
    /// </summary>
    public class Player115 : Player15
    {
        protected override NumberRange NumberType => NumberRange.Star4;
        
    }
}
