/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 用户与微信的绑定
	/// </summary>
    [Table(Name = "usr_Wechat")]
    public partial class UserWechat
    {


        [Column(Name = "UserID")]
        public int UserID { get; set; }


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "OpenId", IsPrimaryKey = true)]
        public string OpenId { get; set; }

        /// <summary>
        /// 一个随机KEY，进行用户绑定
        /// </summary>
        [Column(Name = "Guid")]
        public Guid Guid { get; set; }


        [Column(Name = "Token")]
        public string Token { get; set; }

        /// <summary>
        /// Token的失效时间
        /// </summary>
        [Column(Name = "TokenExpire")]
        public DateTime TokenExpire { get; set; }

        /// <summary>
        /// 微信获取过来的资料信息
        /// </summary>
        [Column(Name = "UserInfo")]
        public string UserInfo { get; set; }

    }
}
