/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Games
{
    /// <summary>
    /// 第三方游戏日志
    /// </summary>
    [Table(Name = "game_Log")]
    public class GameLog
    {
        /// <summary>
        /// 所属站点
        /// </summary>
         [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
         [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
         [Column(Name = "Type")]
        public GameType Type { get; set; }

        /// <summary>
        /// 盈亏金额
        /// </summary>
         [Column(Name = "Money")]
        public decimal Money { get; set; }

        /// <summary>
        /// 投注金额
        /// </summary>
         [Column(Name = "BetAmount")]
        public decimal BetAmount { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
         [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }
    }
}
