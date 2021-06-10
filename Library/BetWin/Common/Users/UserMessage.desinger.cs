/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 站内信
    /// </summary>
    [Table(Name = "usr_Message")]
    public partial class UserMessage
    {

        /// <summary>
        /// 编号
        /// </summary>
        [Column(Name = "MsgID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 接收者
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 信息的发送时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        [Column(Name = "IsRead")]
        public bool IsRead { get; set; }


        [Column(Name = "ReadAt")]
        public DateTime ReadAt { get; set; }

    }
}
