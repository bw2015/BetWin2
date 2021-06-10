/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 契约转账日志
    /// </summary>
    [Table(Name = "usr_ContractLog")]
    public partial class ContractLog
    {

        /// <summary>
        /// 日志编号
        /// </summary>
        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 契约编号
        /// </summary>
        [Column(Name = "ContractID")]
        public int ContractID { get; set; }

        /// <summary>
        /// 契约类型
        /// </summary>
        [Column(Name = "Type")]
        public Contract.ContractType Type { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 契约甲方
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 契约乙方
        /// </summary>
        [Column(Name = "User2")]
        public int User2 { get; set; }

        /// <summary>
        /// 唯一来源
        /// </summary>
        [Column(Name = "SourceID")]
        public int SourceID { get; set; }

        /// <summary>
        /// 契约转账的金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 转账状态
        /// </summary>
        [Column(Name = "Status")]
        public TransferStatus Status { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "LogDesc")]
        public string Description { get; set; }

        /// <summary>
        /// 需要计算的业绩
        /// </summary>
        [Column(Name = "Amount")]
        public Decimal Amount { get; set; }

    }
}
