/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 走势图数据
    /// </summary>
    [Table(Name = "lot_Trend")]
    public partial class LotteryTrend
    {

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Lottery.LotteryType Type { get; set; }


        [Column(Name = "Index", IsPrimaryKey = true)]
        public string Index { get; set; }

        /// <summary>
        /// 系统彩所属的站点，否则为0
        /// </summary>
        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "Number")]
        public string Number { get; set; }

        /// <summary>
        /// 遗漏数据
        /// </summary>
        [Column(Name = "Result")]
        public String Result { get; set; }

    }
}
