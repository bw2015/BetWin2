/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.IM.Common
{
    /// <summary>
    /// 聊天记录
    /// </summary>
    [Table(Name = "usr_ChatLog")]
    public partial class ChatLog
    {

        /// <summary>
        /// 记录编号
        /// </summary>
        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 接收者ID
        /// </summary>
        [Column(Name = "UserID")]
        public string UserID { get; set; }

        /// <summary>
        /// 发送者ID
        /// </summary>
        [Column(Name = "SendID")]
        public string SendID { get; set; }

        /// <summary>
        /// 信息发送者的头像（绝对路径）
        /// </summary>
        [Column(Name = "SendAvatar")]
        public string SendAvatar { get; set; }

        /// <summary>
        /// 信息发送者的名字
        /// </summary>
        [Column(Name = "SendName")]
        public string SendName { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        [Column(Name = "IsRead")]
        public bool IsRead { get; set; }

        /// <summary>
        /// 信息内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 会话KEY
        /// </summary>
        [Column(Name = "TalkKey")]
        public string Key { get; set; }

    }
}
