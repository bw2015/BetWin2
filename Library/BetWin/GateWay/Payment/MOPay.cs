using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.ComponentModel;
using SP.Studio.Model;

using SP.Studio.Web;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// mo宝支付
    /// </summary>
    public class MOPay : IPayment
    {
        public MOPay() : base() { }

        public MOPay(string setting) : base(setting) { }

        /// <summary>
        /// 接口名字
        /// </summary>
        private string apiName
        {
            get
            {
                return WebAgent.IsMobile() ? "WAP_PAY_B2C" : "WEB_PAY_B2C";
            }
        }

        /// <summary>
        /// 接口版本
        /// </summary>
        private const string apiVersion = "1.0.0.0";

        private string _gateway = "https://trade.mobaopay.com/cgi-bin/netpayment/pay_gate.cgi";
        [Description("接口地址")]
        public string Gateway { get { return _gateway; } set { _gateway = value; } }

        [Description("商户ID")]
        public string platformID { get; set; }

        [Description("商户账号")]
        public string merchNo { get; set; }

        private string _merchUrl = "/handler/payment/MOPay";
        [Description("通知地址")]
        public string merchUrl
        {
            get
            {
                return this._merchUrl;
            }
            set
            {
                this._merchUrl = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        /// <summary>
        /// 用于跳转的域名
        /// </summary>
        [Description("跳转域名")]
        public string Domain { get; set; }

        private string _choosePayType = "1";

        [Description("支付方式")]
        public string choosePayType
        {
            get { return _choosePayType; }
            set { _choosePayType = value; }
        }

        public override void GoGateway()
        {
            Dictionary<string, string> payData = new Dictionary<string, string>();
            payData.Add("apiName", this.apiName);
            payData.Add("apiVersion", apiVersion);
            payData.Add("platformID", this.platformID);
            payData.Add("merchNo", this.merchNo);
            payData.Add("orderNo", this.OrderID);
            payData.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
            payData.Add("amt", this.Money.ToString("0.00"));
            payData.Add("merchUrl", this.GetUrl(this.merchUrl));
            payData.Add("merchParam", string.Empty);
            payData.Add("tradeSummary", this.Name);
            payData.Add("bankCode", this.BankValue);
            payData.Add("signMsg", this.Sign(string.Format("apiName={0}&apiVersion={1}&platformID={2}&merchNo={3}&orderNo={4}&tradeDate={5}&amt={6}&merchUrl={7}&merchParam={8}&tradeSummary={9}",
                apiName, apiVersion, platformID, merchNo, OrderID, DateTime.Now.ToString("yyyyMMdd"), this.Money.ToString("0.00"), this.GetUrl(merchUrl), string.Empty, this.Name)));
            payData.Add(_GATEWAY, this.Gateway);
            payData.Add("choosePayType", this.choosePayType);
            this.BuildForm(payData, this.GetGateway(this.Domain, this.Gateway), "POST");
        }

        /// <summary>
        /// 签名加密
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private string Sign(string srcString)
        {
            string result = srcString + this.Key;
            return SP.Studio.Security.MD5.Encryp(result).ToLower();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string srcString = string.Format("apiName={0}&notifyTime={1}&tradeAmt={2}&merchNo={3}&merchParam={4}&orderNo={5}&tradeDate={6}&accNo={7}&accDate={8}&orderStatus={9}",
                           WebAgent.GetParam("apiName"),
                           WebAgent.GetParam("notifyTime"),
                           WebAgent.GetParam("tradeAmt"),
                           WebAgent.GetParam("merchNo"),
                           WebAgent.GetParam("merchParam"),
                           WebAgent.GetParam("orderNo"),
                           WebAgent.GetParam("tradeDate"),
                           WebAgent.GetParam("accNo"),
                           WebAgent.GetParam("accDate"),
                           WebAgent.GetParam("orderStatus"));

            string sigString = WebAgent.GetParam("signMsg");
            string notifyType = WebAgent.GetParam("notifyType");
            if (sigString.Equals(this.Sign(srcString), StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("tradeAmt", 0.00M);
            systemId = WebAgent.GetParam("accNo");
            return WebAgent.GetParam("orderNo");
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.choosePayType != "1") return null;
                Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                _code.Add(BankType.ICBC, "ICBC");
                _code.Add(BankType.ABC, "ABC");
                _code.Add(BankType.BOC, "BOC");
                _code.Add(BankType.CCB, "CCB");
                _code.Add(BankType.COMM, "COMM");
                _code.Add(BankType.CMB, "CMB");
                _code.Add(BankType.SPDB, "SPDB");
                _code.Add(BankType.CIB, "CIB");
                _code.Add(BankType.CMBC, "CMBC");
                _code.Add(BankType.GDB, "GDB");
                _code.Add(BankType.CITIC, "CNCB");
                _code.Add(BankType.CEB, "CEB");
                _code.Add(BankType.HXBANK, "HXB");
                _code.Add(BankType.PSBC, "PSBC");
                _code.Add(BankType.SPABANK, "PAB");
                return _code;
            }
        }
    }
}
