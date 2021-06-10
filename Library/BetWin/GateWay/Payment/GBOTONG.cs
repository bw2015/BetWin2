using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;
using BW.Common.Sites;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Array;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// GBOTONG https://mct.gbotong.com/
    /// </summary>
    public class GBOTONG : IPayment
    {
        public GBOTONG() : base() { }

        public GBOTONG(string setting) : base(setting) { }

        /// <summary>
        /// 接口版本
        /// </summary>
        private const string apiVersion = "1.0.0.0";

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

        [Description("商户ID")]
        public string platformID { get; set; }

        /// <summary>
        /// 商户帐号
        /// </summary>
        [Description("商户账号")]
        public string merchNo { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        private string _merchUrl = "/handler/payment/GBOTONG";
        [Description("到帐通知")]
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

        /// <summary>
        /// 支付方式
        /// </summary>
        [Description("类型 微信：5，支付宝：4、网银：1")]
        public string choosePayType { get; set; }

        [Description("商城域名")]
        public string Shop { get; set; }

        /// <summary>
        /// 支付宝、网银网关
        /// </summary>
        private const string GATEWAY = "https://epay.gbotong.com/cgi-bin/netpayment/pay_gate.cgi";

        protected override string BankValue
        {
            get
            {
                string _bankValue = string.Empty;
                switch (this.choosePayType)
                {
                    case "4":
                    case "5":
                        _bankValue = string.Empty;
                        break;
                    default:
                        _bankValue = base.BankValue;
                        break;
                }
                return _bankValue;
            }
        }
        public override void GoGateway()
        {
            string payerIp = IPAgent.IP;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("apiName", this.apiName);
            dic.Add("apiVersion", apiVersion);
            dic.Add("platformID", this.platformID);
            dic.Add("merchNo", this.merchNo);
            dic.Add("orderNo", this.OrderID);
            dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("amt", this.Money.ToString("0.00"));
            dic.Add("merchUrl", this.GetUrl(this.merchUrl));
            dic.Add("merchParam", this.Description);
            dic.Add("tradeSummary", this.Name);
            dic.Add("bankCode", this.BankValue);
            dic.Add("choosePayType", this.choosePayType);
            dic.Add(_GATEWAY, GATEWAY);

            // apiName,apiVersion,platformID,merchNo,orderNo,tradeDate，amt, merchUrl, merchParam,tradeSummary
            string str = string.Join("&", new string[] { "apiName", "apiVersion", "platformID", "merchNo", "orderNo", "tradeDate", "amt", "merchUrl", "merchParam", "tradeSummary" }.Select(t => string.Format("{0}={1}", t, dic.Get(t, string.Empty))));
            dic.Add("signMsg", MD5.toMD5(str + this.Key).ToLower());

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form action=\"{0}\" method=\"post\" id=\"{1}\">", this.GetGateway(this.Shop, GATEWAY), this.GetType().Name);
            sb.Append(string.Join(string.Empty, dic.Select(t => this.CreateInput(t.Key, t.Value))))
                .Append("</form>")
            .AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();

        }

        public override bool IsWechat()
        {
            return this.choosePayType == "5";
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string str = string.Join("&",
                "apiName,notifyTime,tradeAmt,merchNo,merchParam,orderNo,tradeDate,accNo,accDate,orderStatus".Split(',').Select(t => string.Format("{0}={1}", t, WebAgent.GetParam(t))));
            string sign = MD5.toMD5(str + this.Key).ToUpper();
            if (WebAgent.GetParam("signMsg") != sign || WebAgent.GetParam("orderStatus") != "1") return false;
            callback.Invoke();
            return true;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("tradeAmt", decimal.Zero);
            systemId = WebAgent.GetParam("accNo");
            return WebAgent.GetParam("orderNo");
        }

        private Dictionary<BankType, string> _bankCode;
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.choosePayType != "1") return null;
                if (_bankCode == null)
                {
                    _bankCode = new Dictionary<BankType, string>();
                    _bankCode.Add(BankType.ICBC, "ICBC");
                    _bankCode.Add(BankType.ABC, "ABC");
                    _bankCode.Add(BankType.BOC, "BOC");
                    _bankCode.Add(BankType.CCB, "CCB");
                    _bankCode.Add(BankType.COMM, "COMM");
                    _bankCode.Add(BankType.CMB, "CMB");
                    _bankCode.Add(BankType.SPDB, "SPDB");
                    _bankCode.Add(BankType.CMBC, "CMBC");
                    _bankCode.Add(BankType.GDB, "GDB");
                    _bankCode.Add(BankType.CITIC, "CNCB");
                    _bankCode.Add(BankType.CEB, "CEB");
                    _bankCode.Add(BankType.HXBANK, "HXB");
                    _bankCode.Add(BankType.PSBC, "PSBC");
                    _bankCode.Add(BankType.SPABANK, "PAB");
                    _bankCode.Add(BankType.BJBANK, "BOBJ");
                    _bankCode.Add(BankType.NBBANK, "BONB");

                }
                return _bankCode;
            }
        }
    }
}
