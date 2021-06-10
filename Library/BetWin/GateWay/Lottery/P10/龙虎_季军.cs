using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 龙虎 季军
    /// </summary>
    [BetChat(@"^3(?<Type>[龙虎])(?<Money>\d+)$")]
    public class Player43 : Player41
    {
        protected override int[] Index
        {
            get
            {
                return new int[] { 2 };
            }
        }
    }
}
