using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选单式 三中三
    /// </summary>
    public class Player63 : Player62
    {
        /// <summary>
        /// 任选的长度
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
                return 33;
            }
        }
    }
}
