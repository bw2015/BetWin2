/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Admins
{
    /// <summary>
    /// 管理员分组
    /// </summary>
    [Table(Name = "site_AdminGroup")]
    public partial class AdminGroup
    {


        [Column(Name = "GroupID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 分组名字
        /// </summary>
        [Column(Name = "GroupName")]
        public string Name { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// 管理员权限
        /// </summary>
        [Column(Name = "Permission")]
        public string Permission { get; set; }

    }
}
