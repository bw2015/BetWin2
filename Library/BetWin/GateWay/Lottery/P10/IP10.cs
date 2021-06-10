using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Core;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.P10
{
    /// <summary>
    /// PK10玩法的基类
    /// </summary>
    public abstract class IP10 : IPlayer
    {
        public override LotteryCategory Type
        {
            get { return LotteryCategory.P10; }
        }

        /// <summary>
        /// 获取开奖号码的数组
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        protected virtual string[] GetNumber(string number)
        {
            return number.Split(',');
        }
    }
}
