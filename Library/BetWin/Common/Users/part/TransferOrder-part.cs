using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BW.Common.Users
{
    partial class TransferOrder
    {
        /// <summary>
        /// 转账状态
        /// </summary>
        public enum TransferStatus : byte
        {
            [Description("待审核")]
            None = 0,
            [Description("审核通过")]
            Success = 1,
            [Description("审核失败")]
            Faild = 2
        }
    }
}
