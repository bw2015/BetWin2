/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 用户操作日志
    /// </summary>
    [Table(Name = "usr_Log")]
    public partial class UserLog
    {

        /// <summary>
        /// 日志编号
        /// </summary>
        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }


        [Column(Name = "IP")]
        public string IP { get; set; }

        /// <summary>
        /// 操作内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 会员的浏览器标识
        /// </summary>
        [Column(Name = "BowserID")]
        public Guid BowserID { get; set; }

        /// <summary>
        /// 如果是管理员操作，对应的管理员ID
        /// </summary>
        [Column(Name = "AdminID")]
        public int AdminID { get; set; }

    }
}
