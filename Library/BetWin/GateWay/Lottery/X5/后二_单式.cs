using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后二 单式
    /// </summary>
    public class Player62 : Player52
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
            return input.ToSingleList(false, false, false, true, true);
        }

    }
}
