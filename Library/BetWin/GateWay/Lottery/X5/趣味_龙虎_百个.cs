using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Core;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 龙虎 百个
    /// </summary>
    public class Player196 : Player183
    {
        protected override int[] NumberIndex
        {
            get
            {
                return new int[] { 2, 4 };
            }
        }
    }
}
