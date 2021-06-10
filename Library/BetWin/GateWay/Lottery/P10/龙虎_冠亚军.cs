using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 龙虎  冠亚军
    /// </summary>
    public class Player46 : Player41
    {
        protected override int[] Index
        {
            get
            {
                return new int[] { 0, 1 };
            }
        }
    }
}
