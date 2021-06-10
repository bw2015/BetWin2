/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Sites
{
    /// <summary>
    /// 短信验证码
    /// </summary>
    [Table(Name = "site_SMSCode")]
    public partial class SMSCode
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "Mobile")]
        public string Mobile { get; set; }

        /// <summary>
        /// 6位数的数字验证码
        /// </summary>
        [Column(Name = "Code")]
        public string Code { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        [Column(Name = "SendAt")]
        public DateTime SendAt { get; set; }

        /// <summary>
        /// 是否已经验证
        /// </summary>
        [Column(Name = "IsValid")]
        public bool IsValid { get; set; }

        /// <summary>
        /// 验证的时间
        /// </summary>
        [Column(Name = "ValidAt")]
        public DateTime ValidAt { get; set; }

    }
}
