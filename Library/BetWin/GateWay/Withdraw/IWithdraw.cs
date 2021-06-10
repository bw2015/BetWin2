using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using BW.Common.Sites;
using SP.Studio.Core;
using BW.Framework;

namespace BW.GateWay.Withdraw
{
    public abstract class IWithdraw : SettingBase
    {
        public IWithdraw() : base() { }

        public IWithdraw(string setting) : base(setting) { }

        protected virtual Site SiteInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return (Site)HttpContext.Current.Items[BetModule.SITEINFO];
            }
        }

        /// <summary>
        /// 本地提现订单号
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// 打款金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 卡号
        /// </summary>
        public string CardNo { get; set; }

        /// <summary>
        /// 开卡人姓名
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 银行编号
        /// </summary>
        public BankType BankCode { get; set; }

        /// <summary>
        /// 银行代码转换
        /// </summary>
        protected abstract Dictionary<BankType, string> InterfaceCode { get; }

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="msg">如果出错传递出来的错误信息</param>
        /// <returns></returns>
        public abstract bool Remit(out string msg);

        /// <summary>
        /// 异步请求出款
        /// </summary>
        /// <param name="callback"></param>
        public abstract void Remit(Action<bool, string> callback);

        /// <summary>
        /// 查询订单状态
        /// </summary>
        /// <param name="orderId">订单号</param>
        /// <param name="msg">接口返回的信息</param>
        /// <returns>状态</returns>
        public abstract WithdrawStatus Query(string orderId, out string msg);

        /// <summary>
        /// 获取当前接口支持的提现银行代码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public virtual string GetBankCode(BankType code)
        {
            if (!this.InterfaceCode.ContainsKey(code)) return null;
            return this.InterfaceCode[code];
        }
    }

    /// <summary>
    /// 付款结果
    /// </summary>
    public enum WithdrawStatus
    {
        /// <summary>
        /// 订单号不存在或者其他错误
        /// </summary>
        Error = 0,
        /// <summary>
        /// 正在付款中
        /// </summary>
        Paymenting = 1,
        /// <summary>
        /// 返回
        /// </summary>
        Return = 2,
        /// <summary>
        /// 成功打款
        /// </summary>
        Success = 3

    }
}
