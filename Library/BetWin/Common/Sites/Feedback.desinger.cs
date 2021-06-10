/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
	/// 反馈信息
	/// </summary>
    [Table(Name = "site_Feedback")]
    public partial class Feedback
    {


        [Column(Name = "FedID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "IP")]
        public string IP { get; set; }

        /// <summary>
        /// 留言类型
        /// </summary>
        [Column(Name = "Type")]
        public FeedType Type { get; set; }


        [Column(Name = "Email")]
        public string Email { get; set; }


        [Column(Name = "QQ")]
        public string QQ { get; set; }


        [Column(Name = "Skype")]
        public string Skype { get; set; }

        /// <summary>
        /// 其他联系方式
        /// </summary>
        [Column(Name = "Other")]
        public string Other { get; set; }

        /// <summary>
        /// 留言内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

    }
}
