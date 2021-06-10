using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace BW.Common.Users
{
    /// <summary>
    /// 契约日志
    /// </summary>
    partial class ContractLog
    {
        public enum TransferStatus : byte
        {
            [Description("待支付")]
            None = 0,
            [Description("支付成功")]
            Success = 1
        }
    }
}
