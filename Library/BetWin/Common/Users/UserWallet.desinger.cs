/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 用户钱包
	/// </summary>
    [Table(Name = "usr_Wallet")]
    public partial class UserWallet
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        /// 钱包类型		0：现金钱包（充值、提现均使用该钱包）		1：红利钱包		
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public WalletType Type { get; set; }

        /// <summary>
        /// 钱包余额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

    }
}
