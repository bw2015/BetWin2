using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BW.Common.Lottery.Limited
{
    /// <summary>
    /// 站点的限号数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct SiteLimited
    {
        public SiteLimited(int siteId, LotteryType game, string index, LimitedType type)
        {
            this.SiteID = siteId;
            this.Game = game;
            this.Index = index;
            this.Type = type;
            this.Number = new Dictionary<string, decimal>();
        }

        /// <summary>
        /// 所属站点
        /// </summary>
        public int SiteID;

        /// <summary>
        /// 所属彩种
        /// </summary>
        public LotteryType Game;

        /// <summary>
        /// 所属期号
        /// </summary>
        public string Index;

        /// <summary>
        /// 封锁类型
        /// </summary>
        public LimitedType Type;

        /// <summary>
        /// 已投注的号码
        /// </summary>
        public Dictionary<string, decimal> Number;
    }
}
