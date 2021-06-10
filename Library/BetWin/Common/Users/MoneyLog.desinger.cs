/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 会员帐变记录
    /// </summary>
    [Table(Name = "MoneyLog")]
    public partial class MoneyLog
    {


        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 帐变金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 本次帐变后的余额
        /// </summary>
        [Column(Name = "Balance")]
        public Decimal Balance { get; set; }

        /// <summary>
        /// 帐变IP
        /// </summary>
        [Column(Name = "IP")]
        public string IP { get; set; }

        /// <summary>
        /// 帐变时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  帐变类型
        /// </summary>
        [Column(Name = "Type")]
        public MoneyType Type { get; set; }

        /// <summary>
        /// 来源标注
        /// </summary>
        [Column(Name = "SourceID")]
        public int SourceID { get; set; }

        /// <summary>
        /// 帐变备注信息
        /// </summary>
        [Column(Name = "LogDesc")]
        public string Description { get; set; }

        /// <summary>
        /// 所属的分表
        /// </summary>
        [Column(Name = "TableID")]
        public int TableID { get; set; }
    }
}
