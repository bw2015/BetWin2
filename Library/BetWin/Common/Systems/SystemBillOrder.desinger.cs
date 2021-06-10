/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    ///  账单支付
    /// </summary>
    [Table(Name = "sys_BillOrder")]
    public partial class SystemBillOrder
    {

        /// <summary>
        ///  订单编号
        /// </summary>
        [Column(Name = "OrderID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int ID { get; set; }


        [Column(Name = "SiteID")]
        public int SiteID { get; set; }


        [Column(Name = "Money")]
        public Decimal Money { get; set; }


        [Column(Name = "BillID")]
        public Guid BillID { get; set; }


        [Column(Name = "CreateAt")]
        public DateTime CreateAt { get; set; }


        [Column(Name = "PayAt")]
        public DateTime PayAt { get; set; }

        /// <summary>
        /// 是否已支付
        /// </summary>
        [Column(Name = "IsPayment")]
        public bool IsPayment { get; set; }

    }
}
