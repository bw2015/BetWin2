/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
    /// 电子游戏日志
    /// </summary>
    [Table(Name = "game_SlotLog")]
    public partial class SlotLog
    {

        /// <summary>
        /// 日志编号
        /// </summary>
        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type")]
        public GameType Type { get; set; }

        /// <summary>
        /// 订单号（唯一）
        /// </summary>
        [Column(Name = "BillNo")]
        public string BillNo { get; set; }

        /// <summary>
        /// 游戏名字
        /// </summary>
        [Column(Name = "GameName")]
        public string GameName { get; set; }

        /// <summary>
        /// 游戏时间
        /// </summary>
        [Column(Name = "PlayAt")]
        public DateTime PlayAt { get; set; }

        /// <summary>
        /// 报表的录入时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        [Column(Name = "BetAmount")]
        public Decimal BetAmount { get; set; }

        /// <summary>
        /// 输赢金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 投注之后的额度
        /// </summary>
        [Column(Name = "Balance")]
        public Decimal Balance { get; set; }

        /// <summary>
        /// 结算状态
        /// </summary>
        [Column(Name = "Status")]
        public LogStatus Status { get; set; }

        /// <summary>
        /// 原始的数据
        /// </summary>
        [Column(Name = "ExtendXML")]
        public String ExtendXML { get; set; }

    }
}
