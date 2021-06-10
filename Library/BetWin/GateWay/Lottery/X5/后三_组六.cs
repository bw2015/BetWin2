using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后三 组六
    /// </summary>
    [BetChat(@"^后组六/(?<Number>[0-9]{3,10})/(?<Money>\d+)$")]
    public class Player45 : Player25
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
