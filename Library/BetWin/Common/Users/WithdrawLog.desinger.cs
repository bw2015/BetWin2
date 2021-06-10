/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    ///  提现额度变化日志
    /// </summary>
    [Table(Name = "usr_WithdrawLog")]
    public partial class WithdrawLog
    {

        /// <summary>
        /// 日志编号
        /// </summary>
        [Column(Name = "LogID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 提现额
        /// </summary>
        [Column(Name = "Withdraw")]
        public Decimal Withdraw { get; set; }

        /// <summary>
        /// 操作之后剩余的提现额
        /// </summary>
        [Column(Name = "Balance")]
        public Decimal Balance { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        [Column(Name = "LogDesc")]
        public string Description { get; set; }

    }
}
