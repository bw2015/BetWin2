using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.Common.Games
{
    /// <summary>
    /// 第三方游戏类型
    /// </summary>
    public enum GameType : byte
    {
        [Description("PT"), Game(GameCategory.Slot)]
        PT = 1,
        [Description("AG"), Game(GameCategory.Live, GameCategory.Slot)]
        AG = 2,
        [Description("BBIN"), Game(GameCategory.Live, GameCategory.Slot, GameCategory.Sport)]
        BBIN = 3,
        [Description("三昇体育"), Game(GameCategory.Sport)]
        Sing3 = 4,
        [Description("MG"), Game(GameCategory.Slot)]
        MG = 5,
        [Description("线下赌场"), Game(GameCategory.Scene)]
        Casino = 6,
        [Description("申博"), Game(GameCategory.Live)]
        SunBet = 7,
        [Description("MW"), Game(GameCategory.Slot)]
        MW = 8,
        [Description("泛亚电竞"), Game(GameCategory.Sport)]
        BWGaming = 9,
        /// <summary>
        /// 东方OG
        /// </summary>
        [Description("OG"),Game(GameCategory.Live)]
        OG = 10
    }

    public enum GameCategory
    {
        /// <summary>
        /// 真人视讯
        /// </summary>
        Live = 0,
        /// <summary>
        /// 老虎机
        /// </summary>
        Slot = 1,
        /// <summary>
        /// 体育
        /// </summary>
        Sport = 2,
        /// <summary>
        /// 电子竞技
        /// </summary>
        ESport = 3,
        /// <summary>
        /// 线下游戏
        /// </summary>
        Scene = 4
    }

    public class GameAttribute : Attribute
    {
        public GameAttribute(params GameCategory[] categories)
        {
            this.Category = categories;
        }

        /// <summary>
        /// 包含的游戏类型
        /// </summary>
        public GameCategory[] Category { get; set; }
    }
}
