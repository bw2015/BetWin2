using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;
using SP.Studio.Web;
using SP.Studio.Security;
using BankType = BW.Common.Sites.BankType;
namespace BW.GateWay.Payment
{
    /// <summary>
    /// 易百易支付
    /// </summary>
    public class YBYPay : IPayment
    {
        public YBYPay() : base() { }

        public YBYPay(string setting) : base(setting) { }

        private string _gateway = "http://pay.ybypay.com/bank/index.aspx";

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

        [Description("商户ID")]
        public string parter { get; set; }

        [Description("银行类型")]
        public string bank { get; set; }

        private string _callbackurl = "/handler/payment/YBYPay";
        [Description("异步通知")]
        public string callbackurl
        {
            get
            {
                return this._callbackurl;
            }
            set
            {
                this._callbackurl = value;
            }
        }

        private string _hrefbackurl = "/handler/payment/YBYPay";
        [Description("同步通知")]
        public string hrefbackurl
        {
            get
            {
                return this._hrefbackurl;
            }
            set
            {
                this._hrefbackurl = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("sysorderid");
            money = WebAgent.GetParam("ovalue", decimal.Zero);
            return WebAgent.GetParam("orderid");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parter", this.parter);
            dic.Add("bank", string.IsNullOrEmpty(bank) ? this.BankValue : this.bank);
            dic.Add("value", this.Money.ToString("0.00"));
            dic.Add("orderid", this.OrderID);
            dic.Add("callbackurl", this.GetUrl(this.callbackurl));
            dic.Add("hrefbackurl", this.GetUrl(this.hrefbackurl));
            dic.Add("payerIp", IPAgent.IP);
            dic.Add("attach", Guid.NewGuid().ToString("N"));

            string signStr = string.Join("&",
                new string[] { "parter", "bank", "value", "orderid", "callbackurl" }.Select(t => string.Format("{0}={1}", t, dic[t]))) + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());

            this.BuildForm(dic, this.Gateway, "GET");
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string opstate = WebAgent.GetParam("opstate");
            if (opstate != "0") return false;
            string signStr = string.Format("orderid={0}&opstate={1}&ovalue={2}{3}", WebAgent.GetParam("orderid"), WebAgent.GetParam("opstate"),
                WebAgent.GetParam("ovalue"), this.Key);

            if (MD5.toMD5(signStr).ToLower() != WebAgent.GetParam("sign")) return false;
            callback.Invoke();
            return true;
        }

        /// <summary>
        /// 银行类型
        /// </summary>
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.bank)) return null;
                Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                _code.Add(BankType.CITIC, "962");
                _code.Add(BankType.BOC, "963");
                _code.Add(BankType.ABC, "964");
                _code.Add(BankType.CCB, "965");
                _code.Add(BankType.ICBC, "967");
                _code.Add(BankType.CZBANK, "968");
                _code.Add(BankType.CZCB, "969");
                _code.Add(BankType.CMB, "970");
                _code.Add(BankType.PSBC, "971");
                _code.Add(BankType.CIB, "972");
                _code.Add(BankType.SDEB, "973");
                _code.Add(BankType.SPABANK, "978");
                _code.Add(BankType.SHBANK, "975");
                _code.Add(BankType.SPDB, "977");
                _code.Add(BankType.NJCB, "979");
                _code.Add(BankType.CMBC, "980");
                _code.Add(BankType.COMM, "981");
                _code.Add(BankType.HXBANK, "982");
                _code.Add(BankType.HZCB, "983");
                _code.Add(BankType.GDB, "985");
                _code.Add(BankType.CEB, "986");
                _code.Add(BankType.HKBEA, "987");
                _code.Add(BankType.BOHAIB, "988");
                _code.Add(BankType.BJBANK, "989");
                return _code;
            }
        }
    }
}
