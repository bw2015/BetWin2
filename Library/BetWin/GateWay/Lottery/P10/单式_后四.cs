using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 单式 后四
    /// </summary>
    [BetChat(@"^78910/(?<N1>(1|2|3|4|5|6|7|8|9|10))(?<N2>(1|2|3|4|5|6|7|8|9|10))(?<N3>(1|2|3|4|5|6|7|8|9|10))(?<N4>(1|2|3|4|5|6|7|8|9|10))/(?<Money>\d+)$")]
    public class Player114 : Player6
    {
        protected override int SingleLength
        {
            get
            {
                return 4;
            }
        }

        protected override bool Before
        {
            get
            {
                return false;
            }
        }

        public override decimal RewardMoney
        {
            get
            {
                return 10080M;
            }
        }
    }
}
