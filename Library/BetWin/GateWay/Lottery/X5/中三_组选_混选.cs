using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 中三 组选 混选
    /// </summary>
    public class Player36 : Player26
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star32;
            }
        }
    }
}
