/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 第三方游戏的提现额变化日志
    /// </summary>
    [Table(Name = "usr_GameWithdraw")]
    public partial class GameWithdraw
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type")]
        public BW.Common.Games.GameType Type { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 当前的提现额度
        /// </summary>
        [Column(Name = "Withdraw")]
        public Decimal Withdraw { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  变化说明
        /// </summary>
        [Column(Name = "LogDesc")]
        public string Description { get; set; }

    }
}
