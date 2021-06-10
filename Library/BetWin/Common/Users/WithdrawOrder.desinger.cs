/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
	/// 提现订单
	/// </summary>
    [Table(Name = "usr_WithdrawOrder")]
    public partial class WithdrawOrder
    {

        /// <summary>
        ///  提现订单号
        /// </summary>
        [Column(Name = "WithdrawID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 提现手续费
        /// </summary>
        [Column(Name = "Fee")]
        public Decimal Fee { get; set; }

        /// <summary>
        /// 提现时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///  提现状态
        /// </summary>
        [Column(Name = "Status")]
        public WithdrawStatus Status { get; set; }

        /// <summary>
        ///  备注信息
        /// </summary>
        [Column(Name = "OrderDesc")]
        public string Description { get; set; }

        /// <summary>
        /// 所使用的出款接口
        /// </summary>
        [Column(Name = "WithdrawSettingID")]
        public int WithdrawSettingID { get; set; }

        /// <summary>
        /// 银行类型
        /// </summary>
        [Column(Name = "Bank")]
        public BW.Common.Sites.BankType Bank { get; set; }

        /// <summary>
        /// 银行名字
        /// </summary>
        [Column(Name = "BankName")]
        public string BankName { get; set; }

        /// <summary>
        /// 提现账户名
        /// </summary>
        [Column(Name = "AccountName")]
        public string AccountName { get; set; }

        /// <summary>
        /// 提现账号
        /// </summary>
        [Column(Name = "AccountNumber")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// 需手工处理
        /// </summary>
        [Column(Name = "IsManual")]
        public bool IsManual { get; set; }

        /// <summary>
        /// 预约提现时间
        /// </summary>
        [Column(Name = "Appointment")]
        public DateTime Appointment { get; set; }

        /// <summary>
        /// 提现来源
        /// </summary>
        [Column(Name = "Source")]
        public string Source { get; set; }

        /// <summary>
        /// 处理资金返回的存储过程
        /// </summary>
        [Column(Name = "SourceProc")]
        public string SourceProc { get; set; }

        /// <summary>
        /// 远程网关的订单号
        /// </summary>
        [Column(Name = "SystemID")]
        public string SystemID { get; set; }

    }
}
