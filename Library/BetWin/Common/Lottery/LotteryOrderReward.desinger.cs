/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
	/// 派奖情况
	/// </summary>
    [Table(Name = "lot_OrderReward")]
    public partial class LotteryOrderReward
    {

        /// <summary>
        /// 时间
        /// </summary>
        [Column(Name = "Time", IsPrimaryKey = true)]
        public DateTime Time { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public LotteryType Type { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 奖金
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

    }
}
