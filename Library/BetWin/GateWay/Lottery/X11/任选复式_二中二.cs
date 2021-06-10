using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X11
{
    /// <summary>
    /// 任选复式 二中二
    /// </summary>
    public class Player52 : Player51
    {
        /// <summary>
        /// 任选的数量
        /// </summary>
        protected override int Length
        {
            get
            {
                return 2;
            }
        }


        public override decimal RewardMoney
        {
            get
            {
                return 11.00M;
            }
        }
    }
}
