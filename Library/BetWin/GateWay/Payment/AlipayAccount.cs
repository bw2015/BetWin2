using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using BW.Agent;
using BW.Common.Users;
using SP.Studio.Web;
using SP.Studio.Model;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 支付宝普通账户收款
    /// </summary>
    public class AlipayAccount : IPayment
    {
        [Description("收款账号")]
        public string Account { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        private string _url = "/handler/payment/Redirect";
        [Description("网关")]
        public string Url
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }

        /// <summary>
        /// 二维码路径
        /// </summary>
        [Description("二维码")]
        public string QRCode { get; set; }

        [Description("收款姓名")]
        public string AccountName { get; set; }

        private string _type = "Alipay";
        [Description("类型")]
        public string Type
        {
            get { return this._type; }
            set { this._type = value; }
        }

        [Description("备注信息")]
        public string Remark { get; set; }

        protected override string GetMark()
        {
            return this.Remark;
        }

        public override string GetAccount()
        {
            return this.Account;
        }

        /// <summary>
        /// 检查密钥是否正确
        /// </summary>
        /// <param name="account"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public override bool CheckKey(string account, string key)
        {
            return this.Account == account && this.Key == key;
        }

        public AlipayAccount() : base() { }

        public AlipayAccount(string setting) : base(setting) { }


        public override void GoGateway()
        {
            RechargeOrder order = UserAgent.Instance().GetRechargeOrderInfo(long.Parse(this.OrderID));
            if (order == null) { HttpContext.Current.Response.Write(false, this.OrderID); return; }

            string userName = UserAgent.Instance().GetUserName(order.UserID);
            long time = WebAgent.GetTimeStamp();
            string sign = SP.Studio.Security.MD5.Encryp(string.Join("|", new object[] { Account, this.QRCode, this.AccountName, this.Money.ToString("0.00"), Name, OrderID, userName, time }));
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Type", this.Type);
            dic.Add("Account", this.Account);
            dic.Add("QRCode", this.QRCode);
            dic.Add("AccountName", this.AccountName);
            dic.Add("Money", this.Money.ToString("0.00"));
            dic.Add("Name", this.Name);
            dic.Add("OrderID", this.OrderID);
            dic.Add("UserName", userName);
            dic.Add("Time", time.ToString());
            dic.Add("Sign", sign);
            dic.Add("Remark", this.Remark);

            if (this.IsWechat())
            {
                this.CreateWechatPayment(true, this.QRCode);
            }

            this.CreateQRCode(this.QRCode, BW.Handler.PaymentHandler.REDIRECT_PLUS, dic);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //http://www.180xy.com/handler/payment/AlipayAccount.aspx?
            //account=42010004816695704&orderid=niubi8888|111739990000024464&tradeno=111739990000024464&amount=1&fee=0.00&sign=0db47433c014ffb1cbce923896d93e35
            //account=qss2262@163.com&orderid=vip9988|20161024200040011100250063742198&tradeno=20161024200040011100250063742198&amount=100.00&fee=0.00&sign=3239b3ec2e40a07099c75efa28456eed

            decimal money;
            string systemId;
            this.OrderID = this.GetTradeNo(out money, out systemId);
            string tradeNo = WebAgent.GetParam("tradeno");
            decimal fee = WebAgent.GetParam("fee", 0.00M);
            string account = WebAgent.GetParam("account");


            string param = string.Join("|", new object[] { account, this.OrderID, tradeNo, money.ToString("0.00"), fee.ToString("0.00"), this.Key });
            string sign = SP.Studio.Security.MD5.Encryp(param);

            if (sign == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            this.Money = money = WebAgent.GetParam("amount", 0.00M);
            systemId = WebAgent.GetParam("tradeno");
            return WebAgent.GetParam("orderid");
        }
    }
}
