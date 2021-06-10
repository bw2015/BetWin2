using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Common.Lottery;

namespace BW.GateWay.Lottery.P28
{
    /// <summary>
    /// PC蛋蛋的基础玩法
    /// </summary>
    public abstract class IP28 : IPlayer
    {
        public override LotteryCategory Type => LotteryCategory.P28;

        public override int Bet(string input)
        {
            return input.Split(',').Distinct().Count();
        }
        public override bool IsMatch(string input)
        {
            return !input.Split(',').Any(t => !this.InputBall.Contains(t));
        }
    }
}
