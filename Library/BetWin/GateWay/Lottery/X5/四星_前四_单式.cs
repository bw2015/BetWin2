using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 四星 前四 单式
    /// </summary>
    public class Player112 : Player12
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star41;
            }
        }
    }
}
