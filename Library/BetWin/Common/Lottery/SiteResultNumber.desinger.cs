/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 站点的自定义开奖号码（在单独站内可覆盖官方开奖号，如官方已经开奖则不能添加）
    /// </summary>
    [Table(Name = "lot_SiteResultNumber")]
    public partial class SiteResultNumber
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Lottery.LotteryType Type { get; set; }

        /// <summary>
        /// 彩期
        /// </summary>
        [Column(Name = "Index", IsPrimaryKey = true)]
        public string Index { get; set; }

        /// <summary>
        /// 开奖的时间
        /// </summary>
        [Column(Name = "ResultAt")]
        public DateTime ResultAt { get; set; }

        /// <summary>
        /// 开奖号码
        /// </summary>
        [Column(Name = "Number")]
        public string Number { get; set; }

    }
}
