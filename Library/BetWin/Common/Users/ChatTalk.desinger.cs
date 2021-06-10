/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{

    [Table(Name = "usr_ChatTalk")]
    public partial class ChatTalk
    {

        /// <summary>
        /// 对话Key（对话双方的ID 从小大排列逗号隔开，使用MD5加密）
        /// </summary>
        [Column(Name = "TalkKey", IsPrimaryKey = true)]
        public string Key { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 对话总数
        /// </summary>
        [Column(Name = "Count")]
        public int Count { get; set; }

        /// <summary>
        /// 最后一条信息的时间
        /// </summary>
        [Column(Name = "LastAt")]
        public DateTime LastAt { get; set; }

        /// <summary>
        /// 交谈的双方A
        /// </summary>
        [Column(Name = "User1")]
        public string User1 { get; set; }

        /// <summary>
        /// 交谈的双方B
        /// </summary>
        [Column(Name = "User2")]
        public string User2 { get; set; }

        /// <summary>
        ///  对话类型（用户与管理员、游客与管理员、用户与用户、管理员与管理员）
        /// </summary>
        [Column(Name = "Type")]
        public TalkType Type { get; set; }

    }
}
