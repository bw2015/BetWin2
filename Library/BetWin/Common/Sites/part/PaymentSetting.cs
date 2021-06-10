using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.GateWay.Payment;
using SP.Studio.Model;
using SP.Studio.Core;
using System.ComponentModel;

namespace BW.Common.Sites
{
    /// <summary>
    /// 支付接口
    /// </summary>
    partial class PaymentSetting
    {
        public IPayment PaymentObject
        {
            get
            {
                return PaymentFactory.CreatePayment(this.Type, this.SettingString);
            }
        }

        /// <summary>
        /// 适用平台
        /// </summary>
        [Flags]
        public enum PlatformType : byte
        {
            [Description("网页版")]
            PC = 1,
            [Description("微信")]
            Wechat = 2,
            [Description("WAP")]
            WAP = 4,
            [Description("APP")]
            APP = 8
        }

        /// <summary>
        /// 图标类型
        /// </summary>
        public enum IconType : byte
        {
            [Description("无图标")]
            None,
            [Description("银联")]
            UnionPay,
            [Description("微信")]
            WXPay,
            [Description("支付宝")]
            AliPay,
            [Description("QQ支付")]
            QQPay,
            [Description("京东支付")]
            JDPay,
            [Description("百度支付")]
            BaiduPay,
            [Description("苹果支付")]
            ApplePay
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"ID\":{0},", this.ID)
                .AppendFormat("\"Name\":\"{0}\",", this.Name)
                .AppendFormat("\"Icon\":\"{0}\",", this.Icon)
                .AppendFormat("\"MinMoney\":{0},", this.MinMoney)
                .AppendFormat("\"MaxMoney\":{0},", this.MaxMoney)
                .AppendFormat("\"Reward\":{0},", this.Reward)
                .Append("\"Bank\":");
            if (this.PaymentObject != null && this.PaymentObject.Bank != null)
            {
                sb.Append("{")
                .Append(string.Join(",", this.PaymentObject.Bank.Select(t => string.Format("\"{0}\":\"{1}\"", t, t.GetDescription()))))
                .Append("}");
            }
            else
            {
                sb.Append("null");
            }
            sb.Append(",\"MoneyValue\":");
            if (this.PaymentObject != null && this.PaymentObject.GetMoneyValue() != null)
            {
                sb.AppendFormat("[{0}]", string.Join(",", this.PaymentObject.GetMoneyValue()));
            }
            else
            {
                sb.Append("null");
            }
            sb.Append("}");

            return sb.ToString();
        }
    }
}
