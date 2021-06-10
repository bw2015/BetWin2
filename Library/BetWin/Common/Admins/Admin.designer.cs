/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Admins
{
    /// <summary>
    /// 管理员
    /// </summary>
    [Table(Name = "site_Admin")]
    public partial class Admin
    {

        /// <summary>
        /// 管理员编号
        /// </summary>
        [Column(Name = "AdminID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 管理员用户名
        /// </summary>
        [Column(Name = "AdminName")]
        public string AdminName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column(Name = "NickName")]
        public string NickName { get; set; }

        /// <summary>
        /// 自定义头像
        /// </summary>
        [Column(Name = "Face")]
        public string Face { get; set; }


        [Column(Name = "Password")]
        public string Password { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "LoginIP")]
        public string LoginIP { get; set; }


        [Column(Name = "LoginAt")]
        public DateTime LoginAt { get; set; }

        /// <summary>
        ///  管理员状态
        /// </summary>
        [Column(Name = "Status")]
        public AdminStatus Status { get; set; }


        [Column(Name = "GroupID")]
        public int GroupID { get; set; }

        /// <summary>
        /// 当前管理员是否在线
        /// </summary>
        [Column(Name = "IsOnline")]
        public bool IsOnline { get; set; }

        /// <summary>
        /// 谷歌验证码
        /// </summary>
        [Column(Name = "SecretKey")]
        public Guid SecretKey { get; set; }

    }
}
