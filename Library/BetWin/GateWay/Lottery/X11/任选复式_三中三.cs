using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 三中三
    /// </summary>
    public class Player53 : Player51
    {
        /// <summary>
        /// 任选的数量
        /// </summary>
        protected override int Length
        {
            get
            {
                return 3;
            }
        }

        public override decimal RewardMoney
        {
            get
            {
                return 33.00M;
            }
        }
    }
}
