/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 特殊彩种的非固定开奖时间
    /// </summary>
    [Table(Name = "lot_StartTime")]
    public partial class StartTime
    {

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Lottery.LotteryType Type { get; set; }

        /// <summary>
        /// 期号
        /// </summary>
        [Column(Name = "Index", IsPrimaryKey = true)]
        public string Index { get; set; }

        /// <summary>
        /// 开奖时间
        /// </summary>
        [Column(Name = "StartAt")]
        public DateTime StartAt { get; set; }

    }
}
