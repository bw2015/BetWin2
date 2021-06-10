using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 组选4
    /// </summary>
    public class Player116 : Player16
    {
        protected override NumberRange NumberType => NumberRange.Star41;
        
    }
}
