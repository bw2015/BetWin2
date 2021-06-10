using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后二 复式
    /// </summary>
    public class Player61 : Player51
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star22;
            }
        }


        public override IEnumerable<string> ToLimited(string input)
        {
            return input.ToDuplexList(false, false, false, true, true);
        }
    }
}
