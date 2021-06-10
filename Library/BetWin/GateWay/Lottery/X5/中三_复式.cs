using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 中三 复式
    /// </summary>
    public class Player31 : Player21
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
            return input.ToDuplexList(false, true, true, true, false);
        }
    }
}
