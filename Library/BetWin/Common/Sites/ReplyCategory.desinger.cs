/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 回复分类
    /// </summary>
    [Table(Name = "site_ReplyCategory")]
    public partial class ReplyCategory
    {


        [Column(Name = "CateID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        ///  分类名
        /// </summary>
        [Column(Name = "CateName")]
        public string Name { get; set; }

    }
}
