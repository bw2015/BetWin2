using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.GateWay.Lottery.X11
{
    public abstract class IX11 : IPlayer
    {
        public override LotteryCategory Type
        {
            get { return LotteryCategory.X11; }
        }
    }
}
