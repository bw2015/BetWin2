using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选单式 四中四
    /// </summary>
    public class Player64 : Player62
    {
        /// <summary>
        /// 任选的长度
        /// </summary>
        protected override int Length
        {
            get
            {
                return 4;
            }
        }


        public override decimal RewardMoney
        {
            get
            {
                return 133.00M;
            }
        }
    }
}
