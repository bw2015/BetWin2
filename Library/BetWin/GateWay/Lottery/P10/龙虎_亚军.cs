using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 龙虎 亚军
    /// </summary>
    [BetChat(@"^2(?<Type>[龙虎])(?<Money>\d+)$")]
    public class Player42 : Player41
    {
        protected override int[] Index
        {
            get
            {
                return new int[] { 1 };
            }
        }
    }
}
