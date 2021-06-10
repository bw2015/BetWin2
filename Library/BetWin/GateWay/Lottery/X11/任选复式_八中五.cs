using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 八中五
    /// </summary>
    public class Player58 : Player51
    {
        /// <summary>
        /// 任选的数量
        /// </summary>
        protected override int Length
        {
            get
            {
                return 8;
            }
        }   

        public override decimal RewardMoney
        {
            get
            {
                return 16.42M;
            }
        }
    }
}
