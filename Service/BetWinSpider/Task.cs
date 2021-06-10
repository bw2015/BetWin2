using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Common.Lottery;

namespace BetWinSpider
{
    /// <summary>
    /// 开奖任务
    /// </summary>
    public struct OpenTask
    {
        public OpenTask(LotteryType type, int siteId)
        {
            this.Type = type;
            this.SiteID = siteId;
        }

        public LotteryType Type;

        public int SiteID;
    }
}
