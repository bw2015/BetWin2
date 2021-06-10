using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 后三 组三 混选
    /// </summary>
    public class Player46 : Player26
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star33;
            }
        }
    }
}
