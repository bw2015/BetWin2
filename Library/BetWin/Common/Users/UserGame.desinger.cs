/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户的第三方游戏中的账户信息
    /// </summary>
    [Table(Name = "usr_Game")]
    public partial class GameAccount
    {


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        /// 游戏类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public BW.Common.Games.GameType Type { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 钱包余额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 第三方账户的可转出额度
        /// </summary>
        [Column(Name = "Withdraw")]
        public Decimal Withdraw { get; set; }

        /// <summary>
        /// 上次更新的时间
        /// </summary>
        [Column(Name = "UpdateAt")]
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// 上次转出的时间
        /// </summary>
        [Column(Name = "WithdrawAt")]
        public DateTime WithdrawAt { get; set; }

        /// <summary>
        /// 第三方游戏的用户名
        /// </summary>
        [Column(Name = "PlayerName")]
        public string PlayerName { get; set; }

        /// <summary>
        /// 第三方游戏的账户密码
        /// </summary>
        [Column(Name = "Password")]
        public string Password { get; set; }

    }
}
