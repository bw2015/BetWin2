using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 两面  九名
    /// </summary>
    [BetChat(@"^9(?<Type1>[大小])(?<Type2>[单双])(?<Money>\d+)$")]
    public class Player29 : Player21
    {
        public override int[] Index
        {
            get
            {
                return new int[] { 8 };
            }
        }
    }
}
