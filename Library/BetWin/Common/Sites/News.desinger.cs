/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using SP.Studio.Core;
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{

    [Table(Name = "site_News")]
    public partial class News
    {


        [Column(Name = "NewsID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "ColID")]
        public int ColID { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        [Column(Name = "Type")]
        public NewsColumn.ContentType Type { get; set; }


        [Column(Name = "AdminID")]
        public int AdminID { get; set; }


        [Column(Name = "Title")]
        public string Title { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "Content"), HtmlEncode]
        public string Content { get; set; }

        /// <summary>
        /// 自定义排序，从大到小
        /// </summary>
        [Column(Name = "Sort")]
        public short Sort { get; set; }

        /// <summary>
        /// 弹窗有效期
        /// </summary>
        [Column(Name = "Tip")]
        public int Tip { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [Column(Name = "Cover")]
        public string Cover { get; set; }

    }
}
