/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 短信发送日志
    /// </summary>
    [Table(Name = "site_SMSLog")]
    public partial class SMSLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 短信网关
        /// </summary>
        [Column(Name = "Provider")]
        public BW.GateWay.SMS.SMSProvider Provider { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [Column(Name = "Mobile")]
        public string Mobile { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 发送内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 发送状态
        /// </summary>
        [Column(Name = "Status")]
        public BW.GateWay.SMS.SMSStatus Status { get; set; }

        /// <summary>
        /// 远程网关返回的内容
        /// </summary>
        [Column(Name = "Result")]
        public string Result { get; set; }

    }
}
