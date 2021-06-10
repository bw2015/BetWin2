/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 会员备注信息
    /// </summary>
    [Table(Name = "usr_Remark")]
    public partial class UserRemark
    {

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "MarkID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 管理员
        /// </summary>
        [Column(Name = "AdminID")]
        public int AdminID { get; set; }

        /// <summary>
        /// 备注时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 备注内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

    }
}
