using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BW.Common.Users
{
    /// <summary>
    /// 充值对单
    /// </summary>
    public struct RechargeCheck
    {
        public RechargeCheck(string id, decimal money, decimal amount, RechargeCheckStatus status)
        {
            this.ID = id;
            this.Money = money;
            this.Amount = amount;
            this.Status = status;
        }

        /// <summary>
        /// 订单号
        /// </summary>
        public string ID;

        /// <summary>
        /// 本地入账金额
        /// </summary>
        public decimal Money;

        /// <summary>
        /// 充值网关的实际金额
        /// </summary>
        public decimal Amount;

        /// <summary>
        /// 对单状态
        /// </summary>
        public RechargeCheckStatus Status;
    }

    public enum RechargeCheckStatus
    {
        [Description("对单成功")]
        Success,
        [Description("金额不符")]
        Money,
        [Description("本地订单不存在")]
        NoLocal,
        [Description("远程订单不存在")]
        NoGateway
    }
}
