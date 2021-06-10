/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using SP.Studio.Core;
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 站点的自定义区域配置
    /// </summary>
    [Table(Name = "site_Region")]
    public partial class Region
    {


        [Column(Name = "ID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// 区域名字
        /// </summary>
        [Column(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 区域内容
        /// </summary>
        [Column(Name = "Content"), HtmlEncode]
        public string Content { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
