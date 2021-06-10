/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 系统通知
    /// </summary>
    [Table(Name = "usr_Notify")]
    public partial class UserNotify
    {


        [Column(Name = "NotifyID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 通知类型
        /// </summary>
        [Column(Name = "Type")]
        public NotifyType Type { get; set; }


        [Column(Name = "Message")]
        public string Message { get; set; }


        [Column(Name = "IsRead")]
        public bool IsRead { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "ReadAt")]
        public DateTime ReadAt { get; set; }

    }
}
