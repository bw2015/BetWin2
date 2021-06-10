using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.X5
{
    /// <summary>
    /// 胆码 不位胆 不定二码
    /// </summary>
    public class Player77 : Player73
    {
        protected override IX5.NumberRange NumberType
        {
            get
            {
                return NumberRange.Star5;
            }
        }

        public override decimal RewardMoney => 13.63M;
    }
}
