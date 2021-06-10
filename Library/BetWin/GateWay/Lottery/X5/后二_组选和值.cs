using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后二 组选和值
    /// </summary>
    public class Player66 : Player56
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
