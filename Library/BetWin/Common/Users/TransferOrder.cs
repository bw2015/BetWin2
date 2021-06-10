/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Users
{
    /// <summary>
    /// 线下转账审核列表
    /// </summary>
    [Table(Name = "usr_TransferOrder")]
    public partial class TransferOrder
    {

        /// <summary>
        ///  转账ID
        /// </summary>
        [Column(Name = "TransferID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }

        /// <summary>
        /// 所属站点
        /// </summary>
        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 转账用户
        /// </summary>
        [Column(Name = "UserID")]
        public int UserID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 提交的充值时间
        /// </summary>
        [Column(Name = "PaymentAt")]
        public DateTime PaymentAt { get; set; }

        /// <summary>
        /// 所使用的充值方式
        /// </summary>
        [Column(Name = "PayID")]
        public int PayID { get; set; }


        [Column(Name = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// 提交的充值金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 实际到帐金额
        /// </summary>
        [Column(Name = "Amount")]
        public Decimal Amount { get; set; }

        /// <summary>
        /// 流水编号
        /// </summary>
        [Column(Name = "SerialID")]
        public string SerialID { get; set; }

        /// <summary>
        /// 转账备注
        /// </summary>
        [Column(Name = "Description")]
        public string Description { get; set; }

        /// <summary>
        ///  转账状态		0、待审核 1、审核入账 2、审核失败
        /// </summary>
        [Column(Name = "Status")]
        public TransferStatus Status { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        [Column(Name = "CheckAt")]
        public DateTime CheckAt { get; set; }

        /// <summary>
        /// 充值订单编号
        /// </summary>
        [Column(Name = "RechargeID")]
        public long RechargeID { get; set; }

    }
}
