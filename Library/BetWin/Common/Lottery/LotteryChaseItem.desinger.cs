/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Lottery
{
    /// <summary>
    /// 彩票追号子项
    /// </summary>
    [Table(Name = "lot_ChaseItem")]
    public partial class LotteryChaseItem
    {

        /// <summary>
        /// 追号内容
        /// </summary>
        [Column(Name = "ItemID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }


        [Column(Name = "ChaseID")]
        public int ChaseID { get; set; }

        /// <summary>
        /// 所属彩种
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Lottery.LotteryType Type { get; set; }

        /// <summary>
        /// 彩期
        /// </summary>
        [Column(Name = "Index")]
        public string Index { get; set; }

        /// <summary>
        /// 倍数
        /// </summary>
        [Column(Name = "Times")]
        public int Times { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 奖金
        /// </summary>
        [Column(Name = "Reward")]
        public Decimal Reward { get; set; }

        /// <summary>
        ///  当前状态（正常、中奖后停止、手工退出、完成投注）
        /// </summary>
        [Column(Name = "Status")]
        public LotteryChase.ChaseStatus Status { get; set; }

        /// <summary>
        /// 追号投注时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 允许投注的时间
        /// </summary>
        [Column(Name = "StartAt")]
        public DateTime StartAt { get; set; }

    }
}
