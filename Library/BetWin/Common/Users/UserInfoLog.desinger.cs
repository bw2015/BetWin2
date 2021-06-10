/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户资料的修改时间
    /// </summary>
    [Table(Name = "usr_InfoLog")]
    public partial class UserInfoLog
    {
        
        [Column(Name = "SiteID", IsPrimaryKey = true)]
        public int SiteID { get; set; }


        [Column(Name = "UserID", IsPrimaryKey = true)]
        public int UserID { get; set; }

        /// <summary>
        ///  更改资料的类型
        /// </summary>
        [Column(Name = "Type", IsPrimaryKey = true)]
        public UserInfoLogType Type { get; set; }

        /// <summary>
        /// 更改时间
        /// </summary>
        [Column(Name = "UpdateAt")]
        public DateTime UpdateAt { get; set; }

        /// <summary>
        /// 操作IP
        /// </summary>
        [Column(Name = "IP")]
        public string IP { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// 管理员ID（如果是管理员操作）
        /// </summary>
        [Column(Name = "AdminID")]
        public int AdminID { get; set; }

    }
}
