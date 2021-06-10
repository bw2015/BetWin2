using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Lottery;


namespace BW.Common
{
    /// <summary>
    /// 彩票对象
    /// </summary>
    partial class BetDataContext : DataContext, IDisposable
    {
        /// <summary>
        /// 彩票的配置表
        /// </summary>
        public Table<LotterySetting> LotterySetting
        {
            get { return this.GetTable<LotterySetting>(); }
        }

        /// <summary>
        /// 彩种玩法
        /// </summary>
        public Table<LotteryPlayer> LotteryPlayer
        {
            get
            {
                return this.GetTable<LotteryPlayer>();
            }
        }

        /// <summary>
        /// 开奖时间模板
        /// </summary>
        public Table<TimeTemplate> TimeTemplate
        {
            get
            {
                return this.GetTable<TimeTemplate>();
            }
        }

        /// <summary>
        /// 特殊彩种的自定义开奖时间
        /// </summary>
        public Table<StartTime> StartTime
        {
            get
            {
                return this.GetTable<StartTime>();
            }
        }

        /// <summary>
        /// 彩票订单
        /// </summary>
        public Table<LotteryOrder> LotteryOrder
        {
            get
            {
                return this.GetTable<LotteryOrder>();
            }
        }

        /// <summary>
        /// 按小时分布的投注派奖输赢
        /// </summary>
        public Table<LotteryOrderReward> LotteryOrderReward
        {
            get
            {
                return this.GetTable<LotteryOrderReward>();
            }
        }

        /// <summary>
        /// 彩票订单（用于站点查询的冗余表）
        /// </summary>
        public Table<SiteOrder> SiteOrder
        {
            get
            {
                return this.GetTable<SiteOrder>();
            }
        }

        /// <summary>
        /// 彩票历史订单
        /// </summary>
        public Table<LotteryOrderHistory> LotteryOrderHistory
        {
            get
            {
                return this.GetTable<LotteryOrderHistory>();
            }
        }

        /// <summary>
        /// 开奖号码
        /// </summary>
        public Table<ResultNumber> ResultNumber
        {
            get
            {
                return this.GetTable<ResultNumber>();
            }
        }

        /// <summary>
        /// 站点自定义开奖的官方号码
        /// </summary>
        public Table<SiteResultNumber> SiteResultNumber
        {
            get
            {
                return this.GetTable<SiteResultNumber>();
            }
        }

        /// <summary>
        /// 限号封锁值设定
        /// </summary>
        public Table<LimitedSetting> LimitedSetting
        {
            get
            {
                return this.GetTable<LimitedSetting>();
            }
        }

        /// <summary>
        /// 自定义彩种开奖号码
        /// </summary>
        public Table<SiteNumber> SiteNumber
        {
            get
            {
                return this.GetTable<SiteNumber>();
            }
        }

        /// <summary>
        /// 追号订单
        /// </summary>
        public Table<LotteryChase> LotteryChase
        {
            get
            {
                return this.GetTable<LotteryChase>();
            }
        }

        /// <summary>
        /// 追号详情
        /// </summary>
        public Table<LotteryChaseItem> LotteryChaseItem
        {
            get
            {
                return this.GetTable<LotteryChaseItem>();
            }
        }

        /// <summary>
        /// 合买订单
        /// </summary>
        public Table<United> United
        {
            get
            {
                return this.GetTable<United>();
            }
        }

        /// <summary>
        /// 合买订单跟单
        /// </summary>
        public Table<UnitedItem> UnitedItem
        {
            get
            {
                return this.GetTable<UnitedItem>();
            }
        }

        /// <summary>
        /// 走势图
        /// </summary>
        public Table<LotteryTrend> LotteryTrend
        {
            get
            {
                return this.GetTable<LotteryTrend>();
            }
        }

        /// <summary>
        /// 彩种分类
        /// </summary>
        public Table<LotteryCate> LotteryCate
        {
            get
            {
                return this.GetTable<LotteryCate>();
            }
        }

    }
}
