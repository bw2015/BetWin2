/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{

    [Table(Name = "lot_BetAgent")]
    public partial class LotteryBetAgent
    {

        /// <summary>
        /// 投注订单
        /// </summary>
        [Column(Name = "OrderID", IsPrimaryKey = true)]
        public int OrderID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 投注的用户
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 是否已经发放
        /// </summary>
        [Column(Name = "IsBet")]
        public bool IsBet { get; set; }

        /// <summary>
        /// 投注时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 投注订单的返点值
        /// </summary>
        [Column(Name = "Rebate")]
        public int Rebate { get; set; }

    }
}
