using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选单式 五中五
    /// </summary>
    public class Player65 : Player62
    {
        /// <summary>
        /// 任选的长度
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
