using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 趣味 大小单双 后二
    /// </summary>
    public class Player82 : Player81
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star22;
            }
        }
    }
}
