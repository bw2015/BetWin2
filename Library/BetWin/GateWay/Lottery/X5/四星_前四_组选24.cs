using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 前四 组选24
    /// </summary>
    public class Player113 : Player13
    {
        protected override NumberRange NumberType => NumberRange.Star41;

    }
}
