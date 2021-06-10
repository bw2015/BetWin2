/*    本实体类自动生成，请勿改动。 如数据库结构有变动请重新生成。 */
using System;
using System.Data.Linq.Mapping;

namespace BW.Common.Systems
{
    /// <summary>
    /// 系统账单的支付接口
    /// </summary>
    [Table(Name = "sys_Payment")]
    public partial class SystemPayment
    {

        /// <summary>
        /// 支付接口编号
        /// </summary>
        [Column(Name = "PayID", IsPrimaryKey = true, IsDbGenerated = true)]
        public int PayID { get; set; }

        /// <summary>
        /// 支付接口
        /// </summary>
        [Column(Name = "Type")]
        public BW.GateWay.Payment.PaymentType Type { get; set; }

        /// <summary>
        /// j接口参数
        /// </summary>
        [Column(Name = "Setting")]
        public string SettingString { get; set; }

    }
}
