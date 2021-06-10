/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Logs
{
    /// <summary>
    /// IM系统的错误日志
    /// </summary>
    [Table(Name = "log_ChatError")]
    public partial class ChatErrorLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "Content")]
        public string Content { get; set; }

    }
}
