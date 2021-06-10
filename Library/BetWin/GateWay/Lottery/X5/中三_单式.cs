using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 中三 单式
    /// </summary>
    public class Player32 : Player22
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star32;
            }
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToSingleList(false, true, true, true, false);
        }
    }
}
