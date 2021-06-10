/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// 系统账单
    /// </summary>
    [Table(Name = "sys_Bill")]
    public partial class SystemBill
    {

        /// <summary>
        /// 账单编号
        /// </summary>
        [Column(Name = "BillID", IsPrimaryKey = true)]
        public Guid ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }

        /// <summary>
        /// 账单金额
        /// </summary>
        [Column(Name = "Money")]
        public Decimal Money { get; set; }

        /// <summary>
        /// 账单时间
        /// </summary>
        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }

        /// <summary>
        /// 最后的支付时间
        /// </summary>
        [Column(Name = "EndAt")]
        public DateTime EndAt { get; set; }

        /// <summary>
        /// 账单状态（未支付，已支付）
        /// </summary>
        [Column(Name = "Status")]
        public BillStatus Status { get; set; }

        /// <summary>
        /// 账单标题
        /// </summary>
        [Column(Name = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// 账单内容
        /// </summary>
        [Column(Name = "Content")]
        public string Content { get; set; }

        /// <summary>
        /// 已支付金额
        /// </summary>
        [Column(Name = "Paid")]
        public Decimal Paid { get; set; }

        /// <summary>
        /// 支付接口
        /// </summary>
        [Column(Name = "PayID")]
        public int PayID { get; set; }

    }
}
