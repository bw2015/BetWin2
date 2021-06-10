using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 出款类型
    /// </summary>
    public enum WithdrawType : byte
    {
        /// <summary>
        /// 手动出款
        /// </summary>
        [Description("手动出款")]
        Manually,
        /// <summary>
        /// 通汇卡自动出款
        /// </summary>
        [Description("通汇卡")]
        THCard,
        /// <summary>
        /// 金海蜇
        /// </summary>
        [Description("金海蜇")]
        JHZ,
        /// <summary>
        /// 乐付
        /// </summary>
        [Description("乐付")]
        LeFu,
        [Description("易势代付")]
        IEPLM,
        /// <summary>
        /// 傲视支付
        /// </summary>
        [Description("傲视支付")]
        ASO,
        [Description("泽圣支付")]
        ZSAGE,
        [Description("智付")]
        DinPay,
        [Description("国付宝")]
        GoPay,
        /// <summary>
        /// 摩宝的下发接口
        /// </summary>
        [Description("摩宝")]
        MOPay,
        /// <summary>
        /// 喜付下发接口
        /// </summary>
        [Description("喜付")]
        XIFPay,
        [Description("海付盛通")]
        HaiFuPay,
        [Description("随意付")]
        EasyiPay,
        /// <summary>
        /// 金贝支付
        /// </summary>
        [Description("金贝支付")]
        JBPay,
        [Description("汇畅代付")]
        HuiChang,
        [Description("启航通")]
        QHT,
        [Description("威力代付")]
        WLPay,
        [Description("汇天代付")]
        HTPay
    }
}
