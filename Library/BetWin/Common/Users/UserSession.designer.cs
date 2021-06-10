/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户的登录Key以及在线信息
    /// </summary>
    [Table(Name = "usr_Session")]
    public partial class UserSession
    {


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 登录类型 PC、Mobile
        /// </summary>
        [Column(Name = "Platform", IsPrimaryKey = true)]
        public SP.Studio.PageBase.PlatformType Platform { get; set; }

        /// <summary>
        /// 用户登录产生的随机Key
        /// </summary>
        [Column(Name = "Session")]
        public Guid Session { get; set; }

        /// <summary>
        /// 上次活动的时间
        /// </summary>
        [Column(Name = "UpdateAt")]
        public DateTime UpdateAt { get; set; }

    }
}
