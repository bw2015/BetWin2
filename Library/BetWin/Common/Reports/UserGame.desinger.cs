using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Reports
{
    /// <summary>
    /// 第三方游戏的用户报表
    /// </summary>
    [Table(Name = "rpt_UserGame")]
    public class UserDateGame
    {
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        [Column(Name = "UserID")]
        public int UserID { get; set; }

        [Column(Name = "Type")]
        public Games.GameType Type { get; set; }

        [Column(Name = "Date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// 盈亏金额
        /// </summary>
        [Column(Name = "Money")]
        public decimal Money { get; set; }

        /// <summary>
        /// 有效流水
        /// </summary>
        [Column(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}
