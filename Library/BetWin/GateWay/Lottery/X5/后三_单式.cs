using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后三 单式
    /// </summary>
    public class Player42 : Player22
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star33;
            }
        }

        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToSingleList(false, false, true, true, true);
        }
    }
}
