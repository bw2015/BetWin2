using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 四中四
    /// </summary>
    public class Player54 : Player51
    {
        /// <summary>
        /// 任选的数量
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
