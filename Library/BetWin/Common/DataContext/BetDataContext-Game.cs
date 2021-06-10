using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Games;
namespace BW.Common
{
    partial class BetDataContext
    {
        /// <summary>
        /// 游戏接口参数设定
        /// </summary>
        public Table<GameSetting> GameSetting
        {
            get
            {
                return this.GetTable<GameSetting>();
            }
        }

        /// <summary>
        /// 电子游戏日志
        /// </summary>
        public Table<SlotLog> SlotLog
        {
            get
            {
                return this.GetTable<SlotLog>();
            }
        }

        /// <summary>
        /// 真人视频日志
        /// </summary>
        public Table<VideoLog> VideoLog
        {
            get
            {
                return this.GetTable<VideoLog>();
            }
        }

        /// <summary>
        /// 体育日志
        /// </summary>
        public Table<SportLog> SportLog
        {
            get
            {
                return this.GetTable<SportLog>();
            }
        }

        /// <summary>
        /// 游戏的玩法
        /// </summary>
        public Table<GamePlayer> GamePlayer
        {
            get
            {
                return this.GetTable<GamePlayer>();
            }
        }

        /// <summary>
        /// 与第三方平台之间的转入转出记录
        /// </summary>
        public Table<TransferLog> TransferLog
        {
            get
            {
                return this.GetTable<TransferLog>();
            }
        }

        /// <summary>
        /// 第三方游戏日志的合并（视图）
        /// </summary>
        public Table<GameLog> GameLog
        {
            get
            {
                return this.GetTable<GameLog>();
            }
        }

        /// <summary>
        /// 上分额度变化
        /// </summary>
        public Table<GameMoneyLog> GameMoneyLog
        {
            get
            {
                return this.GetTable<GameMoneyLog>();
            }
        }
    }
}
