using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Common.Lottery;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点的缓存对象
    /// </summary>
    partial class Site
    {
        /// <summary>
        /// 当前站点开放的彩种
        /// </summary>
        public LotterySetting[] LotteryList { get; internal set; }

        /// <summary>
        /// 当前站点开放的彩种玩法
        /// </summary>
        public Dictionary<LotteryType, LotteryPlayer[]> LotteryPlayer { get; internal set; }

        private Dictionary<int, LotteryPlayer> _lotteryPlayerInfo;
        /// <summary>
        /// 当前站点的玩法，使用ID作为索引
        /// </summary>
        public Dictionary<int, LotteryPlayer> LotteryPlayerInfo
        {
            get
            {
                if (_lotteryPlayerInfo == null)
                {
                    _lotteryPlayerInfo = new Dictionary<int, LotteryPlayer>();
                    foreach (KeyValuePair<LotteryType, LotteryPlayer[]> obj in this.LotteryPlayer)
                    {
                        foreach (LotteryPlayer player in obj.Value)
                        {
                            _lotteryPlayerInfo.Add(player.ID, player);
                        }
                    }
                }
                return _lotteryPlayerInfo;
            }
        }

        /// <summary>
        /// 当前站点的限号策略
        /// </summary>
        public List<LimitedSetting> LimitedList { get; internal set; }

        /// <summary>
        /// 获取限号的详细设置，如果为null则不限号
        /// </summary>
        /// <param name="game"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public LimitedSetting GetLimitedInfo(LotteryType game, LimitedType type)
        {
            return this.LimitedList.Find(t => t.Game == game && t.Type == type);
        }

    }
}
