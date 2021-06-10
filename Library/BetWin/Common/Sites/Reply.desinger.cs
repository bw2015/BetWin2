/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 常用语
    /// </summary>
    [Table(Name = "site_Reply")]
    public partial class Reply
    {


        [Column(Name = "ReplyID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "CateID")]
        public int CateID { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 最后的内容编辑人
        /// </summary>
        [Column(Name = "AdminID")]
        public int AdminID { get; set; }

        /// <summary>
        /// 内容的添加/修改时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
