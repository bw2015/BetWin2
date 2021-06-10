/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
    ///  真人视频日志
    /// </summary>
    [Table(Name = "game_VideoLog")]
    public partial class VideoLog
    {


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
        /// 注单流水号
        /// </summary>
        [Column(Name = "BillNo")]
        public string BillNo { get; set; }

        /// <summary>
        /// 所玩游戏名称
        /// </summary>
        [Column(Name = "GameName")]
        public string GameName { get; set; }

        /// <summary>
        /// 游戏局号
        /// </summary>
        [Column(Name = "GameCode")]
        public string GameCode { get; set; }

        /// <summary>
        /// 输赢金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 游戏的开始时间
        /// </summary>
        [Column(Name = "StartAt")]
        public DateTime StartAt { get; set; }

        /// <summary>
        /// 游戏结束时间
        /// </summary>
        [Column(Name = "EndAt")]
        public DateTime EndAt { get; set; }

        /// <summary>
        /// 导入记录的时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 游戏玩法
        /// </summary>
        [Column(Name = "PlayType")]
        public string PlayType { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
        [Column(Name = "BetAmount")]
        public Decimal BetAmount { get; set; }

        /// <summary>
        /// 本局结束之后的余额
        /// </summary>
        [Column(Name = "Balance")]
        public Decimal Balance { get; set; }

        /// <summary>
        ///   结算状态
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
