using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using SP.Studio.Model;
using System.ComponentModel;
using BW.Common.Sites;
using SP.Studio.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 久付
    /// </summary>
    public class Pay9 : IPayment
    {
        public Pay9() : base() { }

        public Pay9(string setting) : base(setting) { }

        private string _gateway = "http://trade.9payonline.com/cgi-bin/netpayment/pay_gate.cgi";
        [Description("接口地址")]
        public string Gateway
        {
            get
            {
                return _gateway;
            }
            set
            {
                _gateway = value;
            }
        }

        /// <summary>
        /// 接口名字
        /// </summary>
        private const string apiName = "WEB_PAY_B2C";

        /// <summary>
        /// 接口版本
        /// </summary>
        private const string apiVersion = "1.0.0.0";

        /// <summary>
        /// 商户ID
        /// </summary>
        [Description("商户ID")]
        public string platformID { get; set; }

        /// <summary>
        /// 商户账号
        /// </summary>
        [Description("商户账号")]
        public string merchNo { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        private string tradeDate
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd");
            }
        }

        [Description("支付结果通知地址")]
        public string merchUrl { get; set; }

        [Description("支付方式,0:微信、1:银行")]
        public int Type { get; set; }

        public override void GoGateway()
        {
            Dictionary<string, string> payData = new Dictionary<string, string>();
            payData.Add("apiName", apiName);
            payData.Add("apiVersion", apiVersion);
            payData.Add("platformID", this.platformID);
            payData.Add("merchNo", this.merchNo);
            payData.Add("orderNo", this.OrderID);
            payData.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
            payData.Add("amt", this.Money.ToString("0.00"));
            payData.Add("merchUrl", this.merchUrl);
            payData.Add("merchParam", string.Empty);
            payData.Add("tradeSummary", this.Name);
            payData.Add("bankCode", this.BankValue);
            payData.Add("signMsg", this.Sign(string.Format("apiName={0}&apiVersion={1}&platformID={2}&merchNo={3}&orderNo={4}&tradeDate={5}&amt={6}&merchUrl={7}&merchParam={8}&tradeSummary={9}",
                apiName, apiVersion, platformID, merchNo, OrderID, this.tradeDate, this.Money.ToString("0.00"), merchUrl, string.Empty, this.Name)));

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>正在提交...</title></head><body>");
            sb.AppendFormat("<form name=\"{1}\" method=\"post\" action=\"{0}\" id=\"{1}\">", this.Gateway, this.GetType().Name);
            foreach (KeyValuePair<string, string> keyValue in payData)
            {
                sb.Append(this.CreateInput(keyValue.Key, keyValue.Value));
            }
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);
            sb.Append("</body></html>");


            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
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
                Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                _code.Add(BankType.Wechat, "");
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

        /// <summary>
        /// 签名加密
        /// </summary>
        private string Sign(string srcString)
        {
            string result = srcString + this.Key;
            return SP.Studio.Security.MD5.Encryp(result).ToLower();
        }

    }
}
