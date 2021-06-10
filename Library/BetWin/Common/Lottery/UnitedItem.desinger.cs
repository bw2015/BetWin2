/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    ///  合买的参与详情
    /// </summary>
    [Table(Name = "lot_UnitedItem")]
    public partial class UnitedItem
    {

        /// <summary>
        /// 合买的购买订单
        /// </summary>
        [Column(Name = "ItemID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 合买订单
        /// </summary>
        [Column(Name = "UnitedID")]
        public int UnitedID { get; set; }

        /// <summary>
        /// 购买的份数
        /// </summary>
        [Column(Name = "Unit")]
        public int Unit { get; set; }

        /// <summary>
        /// 购买金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        ///  购买状态
        /// </summary>
        [Column(Name = "Status")]
        public United.UnitedStatus Status { get; set; }

        /// <summary>
        /// 中奖金额
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        /// 购买时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 购买者的点位
        /// </summary>
        [Column(Name = "Rebate")]
        public int Rebate { get; set; }

    }
}
