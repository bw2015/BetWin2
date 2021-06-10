/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Admins
{
    /// <summary>
    /// 管理员在线状态
    /// </summary>
    [Table(Name = "site_AdminSession")]
    public partial class AdminSession
    {

        /// <summary>
        /// 当前的随机Key
        /// </summary>
        [Column(Name = "Session", IsPrimaryKey = true)]
        public Guid Session { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "AdminID")]
        public int AdminID { get; set; }


        [Column(Name = "UpdateAt")]
        public DateTime UpdateAt { get; set; }

    }
}
