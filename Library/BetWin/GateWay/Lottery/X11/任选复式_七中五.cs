using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 七中五
    /// </summary>
    public class Player57 : Player51
    {
        /// <summary>
        /// 任选的数量
        /// </summary>
        protected override int Length
        {
            get
            {
                return 7;
            }
        }   

        public override decimal RewardMoney
        {
            get
            {
                return 44.00M;
            }
        }
    }
}
