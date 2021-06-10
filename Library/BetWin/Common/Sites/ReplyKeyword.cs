/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 关键词自动回复
    /// </summary>
    [Table(Name = "site_ReplyKeyword")]
    public partial class ReplyKeyword
    {

        /// <summary>
        /// 关键词编号
        /// </summary>
        [Column(Name = "KeyID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 关键词，多个关键词使用空格
        /// </summary>
        [Column(Name = "Keyword")]
        public string Keyword { get; set; }

        /// <summary>
        /// 回复内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
