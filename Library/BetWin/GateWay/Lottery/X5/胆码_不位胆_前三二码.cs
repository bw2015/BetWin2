using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 胆码 不位胆 前三二码
    /// </summary>
    public class Player75 : Player73
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star31;
            }
        }
    }
}
