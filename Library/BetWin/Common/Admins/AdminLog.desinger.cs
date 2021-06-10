/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Admins
{
    /// <summary>
    /// 管理员操作日志
    /// </summary>
    [Table(Name = "site_AdminLog")]
    public partial class AdminLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "AdminID")]
        public int AdminID { get; set; }

        /// <summary>
        /// 管理员日志类型
        /// </summary>
        [Column(Name = "Type")]
        public LogType Type { get; set; }

        /// <summary>
        /// 操作IP
        /// </summary>
        [Column(Name = "IP")]
        public string IP { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 操作内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 原始操作信息
        /// </summary>
        [Column(Name = "ExtendXML")]
        public String ExtendXML { get; set; }

    }
}
