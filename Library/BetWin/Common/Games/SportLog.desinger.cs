/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
    /// 体育游戏日志
    /// </summary>
    [Table(Name = "game_SportLog")]
    public partial class SportLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Games.GameType Type { get; set; }

        /// <summary>
        /// 流水
        /// </summary>
        [Column(Name = "WagersID")]
        public string WagersID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 下注的时间
        /// </summary>
        [Column(Name = "PlayAt")]
        public DateTime PlayAt { get; set; }

        /// <summary>
        /// 游戏种类（足球、篮球或者其他）
        /// </summary>
        [Column(Name = "GameType")]
        public string GameType { get; set; }

        /// <summary>
        /// 下注金额
        /// </summary>
        [Column(Name = "BetAmount")]
        public Decimal BetAmount { get; set; }

        /// <summary>
        /// 有效投注金额
        /// </summary>
        [Column(Name = "BetMoney")]
        public Decimal BetMoney { get; set; }

        /// <summary>
        /// 输赢
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 结果采集时间
        /// </summary>
        [Column(Name = "ResultAt")]
        public DateTime ResultAt { get; set; }

        /// <summary>
        /// 投注结果
        /// </summary>
        [Column(Name = "Result")]
        public string Result { get; set; }

        /// <summary>
        ///  比赛状态		0：None 未结算		1：Finish 已结算		2：Error 其他错误
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
