using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;
using SP.Studio.Data;
using BW.Agent;
using BW.Common.Sites;
using BW.Common.Users;


using SP.Studio.Web;
using SP.Studio.Security;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// MY18线下转账
    /// </summary>
    public class MY18 : IPayment
    {
        public MY18() : base() { }

        public MY18(string setting) : base(setting) { }

        [Description("开户行")]
        public string BankName { get; set; }

        [Description("账号")]
        public string Account { get; set; }

        [Description("账户名")]
        public string AccountName { get; set; }

        [Description("备注信息")]
        public string Remark { get; set; }

        [Description("银行名字")]
        public string AccountBank { get; set; }

        public readonly string GateWay = "/handler/payment/Redirect";

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string Key { get; set; }

        [Description("过滤关键词")]
        public string Keyword { get; set; }

        [Description("微信备注")]
        public string WechatRemark { get; set; }


        [Description("二维码")]
        public string QRCode { get; set; }

        public override string GetAccount()
        {
            return this.AccountBank;
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        protected override string GetMark()
        {
            return this.WechatRemark;
        }

        public override void GoGateway()
        {
            if (this.IsWechat())
            {
                Uri uri = HttpContext.Current.Request.UrlReferrer;
                string host = HttpContext.Current.Request.UrlReferrer.Authority;
                string url = string.Format("{0}://{1}/wechat/user/recharge-submit.html?ID={2}", uri.Scheme, host, this.OrderID);
                this.CreateWechatPayment(true, WebAgent.GetQRCode(url));
            }

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("_gatetype", "bank");
            data.Add("_money", this.Money.ToString("0.00"));
            data.Add("_orderid", this.OrderID);
            data.Add("_remark", this.Remark);
            data.Add("_gateway", this.GateWay);
            data.Add("_accountname", this.AccountName);
            data.Add("_account", this.Account);
            data.Add("_bankname", this.BankName);
            data.Add("_name", this.Name);
            data.Add("_qrcode", this.QRCode);

            this.BuildForm(data, this.GetUrl(this.GateWay));
        }

        public override bool Verify(VerifyCallBack callback)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                if (key == "sign") continue;
                dic.Add(key, WebAgent.QF(key));
            }
            string sign = WebAgent.QF("sign");
            if (sign != MD5.toMD5(string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + this.Key)) return false;
            callback.Invoke();
            return true;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("Money", decimal.Zero);
            systemId = WebAgent.GetParam("SystemID");
            string bank = WebAgent.GetParam("Bank");
            string name = WebAgent.GetParam("Name");
            DateTime date = WebAgent.GetParam("Date", DateTime.Now);
            PaymentSetting pay = SiteAgent.Instance().GetPaymentInfo(PaymentType.MY18, bank);
            if (pay == null) return null;

            MY18 payment = (MY18)pay.PaymentObject;
            foreach (string key in payment.Keyword.Split(','))
            {
                if (string.IsNullOrEmpty(key)) continue;
                Regex regex = new Regex(key);
                name = regex.Replace(name, string.Empty);
            }

            TransferOrder order = UserAgent.Instance().GetTransferOrderInfo(pay.ID, name, money, date);
            if (order == null) return null;
            if (order.RechargeID != 0) return order.RechargeID.ToString();
            long orderId = UserAgent.Instance().CreateRechargeOrder(order.UserID, order.PayID, money, "MY18自动审核", true);
            if (orderId == 0) return null;
            order.RechargeID = orderId;
            order.Update(null, t => t.RechargeID);
            return orderId.ToString();
        }
    }
}
