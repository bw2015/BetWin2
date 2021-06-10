using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SP.Studio.Web;
using SP.Studio.Core;

namespace BW.Common.Users
{
    partial class WithdrawOrder
    {
        public enum WithdrawStatus : byte
        {
            [Description("待处理")]
            None = 0,
            [Description("审核失败")]
            Faild = 1,
            /// <summary>
            /// 审核成功，银行付款中
            /// </summary>
            [Description("银行付款中")]
            Success = 2,
            /// <summary>
            /// 已到账
            /// </summary>
            [Description("已到账")]
            Finish = 3,
            /// <summary>
            /// 银行退单
            /// </summary>
            [Description("银行退单")]
            Return = 4,
            /// <summary>
            /// 已被拆单
            /// </summary>
            [Description("拆单")]
            Split = 5
        }

        /// <summary>
        /// 总共的金额（包括手续费）
        /// </summary>
        public decimal TotalMoney
        {
            get
            {
                return this.Money + this.Fee;
            }
        }

        /// <summary>
        /// 提现的账户
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1} 尾号：{2}",
                WebAgent.HiddenName(this.AccountName),
                this.Bank.GetDescription(),
                this.AccountNumber.Substring(this.AccountNumber.Length > 4 ? this.AccountNumber.Length - 4 : 0));
        }

        /// <summary>
        /// 获取预约时间的显示
        /// </summary>
        /// <returns></returns>
        public string GetAppointment()
        {
            if (this.Appointment.Year < 2000) return null;
            if (Math.Abs(((TimeSpan)(this.Appointment - this.CreateAt)).TotalMinutes) < 5) return null;
            return this.Appointment.ToString();
        }
    }
}
