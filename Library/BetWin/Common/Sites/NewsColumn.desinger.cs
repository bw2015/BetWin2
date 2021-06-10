/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点的栏目管理
    /// </summary>
    [Table(Name = "site_NewsColumn")]
    public partial class NewsColumn
    {

        /// <summary>
        /// 栏目编号
        /// </summary>
        [Column(Name = "ColID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 内容类型（公告、帮助）
        /// </summary>
        [Column(Name = "Type")]
        public ContentType Type { get; set; }

        /// <summary>
        /// 栏目名称
        /// </summary>
        [Column(Name = "ColName")]
        public string Name { get; set; }

        /// <summary>
        /// 排序，从大到小
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

    }
}
