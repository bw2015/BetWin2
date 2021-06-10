/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{

    [Table(Name = "lot_LimitedSetting")]
    public partial class LimitedSetting
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 彩种
        /// </summary>
        [Column(Name = "Game", IsPrimaryKey = true)]
        public LotteryType Game { get; set; }

        /// <summary>
        /// 所属的限号组
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public LimitedType Type { get; set; }

        /// <summary>
        /// 封锁值
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

    }
}
