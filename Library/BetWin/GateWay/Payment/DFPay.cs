using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Array;
using SP.Studio.Security;
using BW.Common.Sites;
using SP.Studio.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class DFPay : IPayment
    {
        public DFPay() : base() { }

        public DFPay(string setting) : base(setting) { }

        private const string signType = "MD5";

        private const string service = "directPay";

        private const string inputCharset = "UTF-8";

        private string _gateway = "http://epay.dfpay.vip/serviceDirect.html";
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

        [Description("商户号")]
        public string merchantId { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        private string _notifyUrl = "/handler/payment/DFPay";
        [Description("通知URL")]
        public string notifyUrl
        {
            get
            {
                return this._notifyUrl;
            }
            set
            {
                this._notifyUrl = value;
            }
        }

        private string _returnUrl = "/handler/payment/DFPay";
        [Description("返回URL")]
        public string returnUrl
        {
            get
            {
                return this._returnUrl;
            }
            set
            {
                this._returnUrl = value;
            }
        }

        /// <summary>
        /// 900 微信扫码    901 微信H5    902 ⽀付宝扫码   903 ⽀付宝H5   904 ⽹银跳转    905 ⽹银直连
        /// 906 百度钱包    907 QQ钱包    908 京东钱包    909 QQ钱包H5      910 QQWAP   911 银联扫码
        /// 912 快捷⽀付    913 京东WAP   914 微信条码⽀付
        /// </summary>
        [Description("支付方式")]
        public string payMethod { get; set; }

        public override string ShowCallback()
        {
            return "ok";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("TransactionId");
            money = WebAgent.GetParam("FaceValue", decimal.Zero);
            return WebAgent.GetParam("OrderId");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("service", service);
            dic.Add("merchantId", this.merchantId);
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            dic.Add("returnUrl", this.GetUrl(this.returnUrl));
            dic.Add("inputCharset", inputCharset);
            dic.Add("outOrderId", this.OrderID);
            dic.Add("subject", "subject");
            dic.Add("body", "body");
            dic.Add("transAmt", this.Money.ToString("0.00"));
            dic.Add("payMethod", this.payMethod);
            if (this.payMethod == "905") dic.Add("defaultBank", this.BankValue);
            dic.Add("channel", "B2C");
            dic.Add("cardAttr", "01");
            dic.Add("attach", "attach");

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.Key;

            dic.Add("signType", signType);
            dic.Add("sign", MD5.toMD5(signStr).ToLower());

            this.BuildForm(dic, this.Gateway);

        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                dic.Add(key, WebAgent.QF(key));
            }
            string sign = dic["sing"];
            dic.Remove("sign");
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.Key;
            if (MD5.toMD5(signStr).ToLower() == sign)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.payMethod != "905") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.CZBANK, "CZB");
                dic.Add(BankType.NBBANK, "NBCB");
                dic.Add(BankType.NJCB, "NJCB");
                dic.Add(BankType.BJBANK, "BOB");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.PSBC, "PSBC");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.CMB, "CMBC");
                dic.Add(BankType.COMM, "BCM");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.CITIC, "CNCB");
                dic.Add(BankType.SPABANK, "PAB");
                dic.Add(BankType.CMBC, "CMSB");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.SHBANK, "BOS");
                return dic;
            }
        }
    }
}
