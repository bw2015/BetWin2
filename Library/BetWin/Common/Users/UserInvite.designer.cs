/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 用户邀请注册
	/// </summary>
    [Table(Name = "usr_Invite")]
    public partial class UserInvite
    {

        /// <summary>
        /// 注册码
        /// </summary>
        [Column(Name = "InviteID", IsPrimaryKey = true)]
        public string ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 通过此注册链接注册的人数
        /// </summary>
        [Column(Name = "Member")]
        public int Member { get; set; }

        /// <summary>
        /// 此链接的返点类型
        /// </summary>
        [Column(Name = "Rebate")]
        public int Rebate { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  注册的用户类型
        /// </summary>
        [Column(Name = "Type")]
        public User.UserType Type { get; set; }

        /// <summary>
        /// 绑定的独立域名
        /// </summary>
        [Column(Name = "InviteDomain")]
        public string InviteDomain { get; set; }

    }
}
