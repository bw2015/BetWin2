using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
namespace BW.GateWay.SMS
{
    /// <summary>
    /// 短信提供商
    /// </summary>
    public enum SMSProvider : byte
    {
        /// <summary>
        /// 无供应商
        /// </summary>
        None,
        /// <summary>
        /// http://www.ihuyi.com/product.php
        /// 互亿无线
        /// </summary>
        [Description("互亿无线")]
        IHuYi
    }
}
