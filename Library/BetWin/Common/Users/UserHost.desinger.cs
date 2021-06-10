/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户的授权设备
    /// </summary>
    [Table(Name = "usr_Host")]
    public partial class UserHost
    {


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }


        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }

        /// <summary>
        /// 平台类型
        /// </summary>
        [Column(Name = "Platform", IsPrimaryKey = true)]
        public string Platform { get; set; }

        /// <summary>
        /// 授权Key值
        /// </summary>
        [Column(Name = "Host")]
        public Guid Host { get; set; }

        /// <summary>
        /// 授权日志
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
