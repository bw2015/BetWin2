using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BW.Common.Systems
{
    /// <summary>
    /// 账单
    /// </summary>
    partial class SystemBill
    {
        /// <summary>
        /// 账单状态
        /// </summary>
        public enum BillStatus : byte
        {
            [Description("未支付")]
            Normal = 1,
            [Description("已支付")]
            Paid = 2
        }
    }
}
