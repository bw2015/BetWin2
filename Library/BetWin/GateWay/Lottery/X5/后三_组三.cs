using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后三 组三
    /// </summary>
    [BetChat(@"^后组三/(?<Number>[0-9]{2,10})/(?<Money>\d+)$")]
    public class Player44 : Player24
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star33;
            }
        }
    }
}
