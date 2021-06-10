using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 龙虎 千十
    /// </summary>
    public class Player193 : Player183
    {
        protected override int[] NumberIndex
        {
            get
            {
                return new int[] { 1, 3 };
            }
        }
    }
}
