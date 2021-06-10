using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.BA
{
    public abstract class IBA : IPlayer
    {
        public override LotteryCategory Type
        {
            get
            {
                return LotteryCategory.BA;
            }
        }

        protected int[] GetResultNumber(string number)
        {
            if (!this.IsResult(number)) return null;
            return number.Split(',').Select(t => int.Parse(t)).ToArray();
        }
    }
}
