using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 趣味 单双
    /// </summary>
    [BetChat(@"^(?<Number>(1|2|3|4|5|6|7|8|9|10))(?<Type>[单双])(?<Money>\d+)$")]
    public class Player12 : Player11
    {
        public override string[] InputBall
        {
            get
            {
                return new string[] { "单", "双" };
            }
        }

        protected override string GetResult(int num)
        {
            return num % 2 == 0 ? "双" : "单";
        }
    }
}
