using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后三 复式
    /// </summary>
    public class Player41 : Player21
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
            return input.ToDuplexList(false, false, true, true, true);
        }
    }
}
