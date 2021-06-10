using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 中三 和值
    /// </summary>
    public class Player33 : Player23
    {

        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star32;
            }
        }
    }
}
