using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// 排名竞猜    定位胆  前五
    /// </summary>
    [BetChat(@"^(?<Type>(6|7|8|9|10))/(?<Number>(1|2|3|4|5|6|7|8|9|10)+)/(?<Money>\d+)$")]
    public class Player5 : Player4
    {
        protected override string[] GetNumber(string number)
        {
            return number.Split(',').Skip(5).ToArray();
        }
    }
}
