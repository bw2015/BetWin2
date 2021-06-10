/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 彩期的时间模板
    /// </summary>
    [Table(Name = "lot_TimeTemplate")]
    public partial class TimeTemplate
    {

        /// <summary>
        /// 彩期
        /// </summary>
        [Column(Name = "Index", IsPrimaryKey = true)]
        public int Index { get; set; }

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Lottery.LotteryType Type { get; set; }

        /// <summary>
        /// 开奖时间（分钟）
        /// </summary>
        [Column(Name = "Time")]
        public int Time { get; set; }

    }
}
