using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Array;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 优+支付
    /// </summary>
    public class UPlus : IPayment
    {
        public UPlus() : base() { }

        public UPlus(string setting) : base(setting) { }

        [Description("商户号")]
        public string pay_memberid { get; set; }

        private string _pay_notifyurl = "/handler/payment/UPlus";
        [Description("服务端通知")]
        public string pay_notifyurl
        {
            get
            {
                return this._pay_notifyurl;
            }
            set
            {
                this._pay_notifyurl = value;
            }
        }

        private string _pay_callbackurl = "/handler/payment/UPlus";
        [Description("页面跳转")]
        public string pay_callbackurl
        {
            get
            {
                return this._pay_callbackurl;
            }
            set
            {
                this._pay_callbackurl = value;
            }
        }

        /// <summary>
        /// 901	微信H5支付  902	微信扫码支付  903	支付宝扫码支付 904	支付宝H5支付
        /// 905	QQ钱包H5支付    907	网银支付    911	快捷一码付
        /// </summary>
        [Description("通道编号")]
        public string pay_bankcode { get; set; }

        [Description("密钥")]
        public string pay_key { get; set; }

        private string _gateway = "http://www.stfuu.com/Pay_Index.html";
        [Description("网关")]
        public string Gateway
        {
            get
            {
                return this._gateway;
            }
            set
            {
                this._gateway = value;
            }
        }

        public override string ShowCallback()
        {
            return "ok";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("transaction_id");
            money = WebAgent.GetParam("amount", decimal.Zero);
            return WebAgent.GetParam("orderid");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("pay_memberid", this.pay_memberid);
            dic.Add("pay_applydate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("pay_orderid", this.OrderID);
            dic.Add("pay_bankcode", this.pay_bankcode);
            dic.Add("pay_notifyurl", this.GetUrl(this.pay_notifyurl));
            dic.Add("pay_callbackurl", this.GetUrl(this.pay_callbackurl));
            dic.Add("pay_amount", this.Money.ToString("0.00"));
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.pay_key;
            dic.Add("pay_md5sign", MD5.toMD5(signStr));
            dic.Add("pay_productname", this.Name);
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in new string[] { "memberid", "orderid", "amount", "transaction_id", "datetime", "returncode" })
            {
                dic.Add(key, WebAgent.GetParam(key));
            }
            if (dic.Get("returncode", "") != "00") return false;

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.pay_key;
            string sign = WebAgent.GetParam("sign");
            if (MD5.toMD5(signStr).Equals(sign, StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }
    }
}
