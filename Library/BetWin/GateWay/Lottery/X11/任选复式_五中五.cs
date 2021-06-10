using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 五中五
    /// </summary>
    public class Player55 : Player51
    {
        /// <summary>
        /// 任选的数量
        /// </summary>
        protected override int Length
        {
            get
            {
                return 5;
            }
        }   

        public override decimal RewardMoney
        {
            get
            {
                return 923.33M;
            }
        }
    }
}
