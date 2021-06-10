using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BW.Common.Users
{
    partial class UserWallet
    {
        /// <summary>
        /// 钱包类型
        /// </summary>
        public enum WalletType : byte
        {
            /// <summary>
            /// 现金钱包
            /// </summary>
            [Description("现金钱包")]
            Cash = 0,
            /// <summary>
            /// 红利钱包
            /// </summary>
            [Description("红利钱包")]
            Bonus = 1
        }
    }
}
