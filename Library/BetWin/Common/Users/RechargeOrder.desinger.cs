/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 充值订单
    /// </summary>
    [Table(Name = "usr_RechargeOrder")]
    public partial class RechargeOrder
    {

        /// <summary>
        /// 充值订单号，格式yyyyHHmmHHmmss(id)
        /// </summary>
        [Column(Name = "OrderID", IsPrimaryKey = true)]
        public long ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 提交的金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 提交充值的时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 充值接口
        /// </summary>
        [Column(Name = "PayID")]
        public int PayID { get; set; }

        /// <summary>
        /// 实际的充值金额
        /// </summary>
        [Column(Name = "Amount")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 手续费
        /// </summary>
        [Column(Name = "Fee")]
        public Decimal Fee { get; set; }

        /// <summary>
        /// 到账时间
        /// </summary>
        [Column(Name = "PayAt")]
        public DateTime PayAt { get; set; }

        /// <summary>
        /// 是否已经到账
        /// </summary>
        [Column(Name = "IsPayment")]
        public bool IsPayment { get; set; }

        /// <summary>
        /// 支付所选择的银行
        /// </summary>
        [Column(Name = "BankType")]
        public BW.Common.Sites.BankType BankType { get; set; }

        /// <summary>
        /// 支付成功之后的远程网关订单号
        /// </summary>
        [Column(Name = "SystemID")]
        public string SystemID { get; set; }

        /// <summary>
        /// 备注信息（管理员手动充值才有）
        /// </summary>
        [Column(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        /// 充值奖励
        /// </summary>
        [Column(Name = "Reward")]
        public decimal Reward { get; set; }

    }
}
