/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 彩票追号
    /// </summary>
    [Table(Name = "lot_Chase")]
    public partial class LotteryChase
    {

        /// <summary>
        /// 追号编号
        /// </summary>
        [Column(Name = "ChaseID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }


        [Column(Name = "Type")]
        public BW.Common.Lottery.LotteryType Type { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  当前状态（正常、中奖后停止、手工退出、完成投注）
        /// </summary>
        [Column(Name = "Status")]
        public ChaseStatus Status { get; set; }

        /// <summary>
        /// 总追号资金
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 已投注的金额
        /// </summary>
        [Column(Name = "BetMoney")]
        public Decimal BetMoney { get; set; }

        /// <summary>
        /// 总中奖资金
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        /// 投注内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 中奖后停止
        /// </summary>
        [Column(Name = "IsRewardStop")]
        public bool IsRewardStop { get; set; }

        /// <summary>
        /// 当前的追号期数
        /// </summary>
        [Column(Name = "Count")]
        public int Count { get; set; }

        /// <summary>
        /// 总追号期数
        /// </summary>
        [Column(Name = "Total")]
        public int Total { get; set; }

    }
}
