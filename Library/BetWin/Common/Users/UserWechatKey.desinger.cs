/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 用户绑定微信、登录时候需要用到的KEY值
	/// </summary>
    [Table(Name = "usr_WechatKey")]
    public partial class UserWechatKey
    {


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        /// 随机值
        /// </summary>
        [Column(Name = "Key")]
        public Guid Key { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
