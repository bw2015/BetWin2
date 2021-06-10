/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    ///  系统的全局日志
    /// </summary>
    [Table(Name = "log_System")]
    public partial class SystemLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int LogID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "Content")]
        public string Content { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

    }
}
