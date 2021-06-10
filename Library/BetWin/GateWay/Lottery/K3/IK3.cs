using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.K3
{
    /// <summary>
    /// 快三类型
    /// </summary>
    public abstract class IK3 : IPlayer
    {
        public override LotteryCategory Type
        {
            get { return LotteryCategory.K3; }
        }
    }
}
