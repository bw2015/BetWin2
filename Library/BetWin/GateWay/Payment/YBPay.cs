using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;

using SP.Studio.Security;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 优宝支付
    /// </summary>
    public class YBPay : IPayment
    {
        public YBPay() { }

        public YBPay(string setting) : base(setting) { }

        /// <summary>
        /// 支付网关
        /// </summary>
        private const string GATEWAY = "http://pay.uospay.com/pay.php";


        private const string apiVersion = "V.1.0.0";

        [Description("商城域名")]
        public string Shop { get; set; }

        /// <summary>
        /// WAP方式：“WAP_PAY”（手机支付）
        /// WWW方式：“WWW_PAY”（pc浏览器）
        /// </summary>
        public string apiName
        {
            get
            {
                return "WWW_PAY";
            }
        }

        [Description("商户ID")]
        public string platformID { get; set; }

        [Description("商户账号")]
        public string merchNo { get; set; }

        [Description("商户密钥")]
        public string Key { get; set; }

        private string _notifyUrl = "/handler/payment/YBPay";
        [Description("通知地址")]
        public string notifyUrl
        {
            get
            {
                return _notifyUrl;
            }
            set
            {
                _notifyUrl = value;
            }
        }


        private string _returnUrl = "/handler/payment/YBPay";
        [Description("返回地址")]
        public string returnUrl
        {
            get
            {
                return _returnUrl;
            }
            set
            {
                _returnUrl = value;
            }
        }

        /// <summary>
        /// 支付方式
        /// </summary>
        [Description("支付方式 网银(YOUBAO) 微信(WEIXIN) 支付宝(ALIPAY)")]
        public string choosePayType { get; set; }

        public override void GoGateway()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string str = string.Format("apiName={0}&apiVersion={1}&platformID={2}&merchNo={3}&orderNo={4}&tradeDate={5}&amt={6}&notifyUrl={7}&returnUrl={8}&merchParam={9}",
               this.apiName, apiVersion, this.platformID, this.merchNo, this.OrderID.Substring(8), date, this.Money.ToString("0.00"), this.GetUrl(this.notifyUrl), this.GetUrl(this.returnUrl), this.Description);

            string sign = MD5.toMD5(str + this.Key).ToLower();

            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("apiName", this.apiName);
            dic.Add("apiVersion", apiVersion);
            dic.Add("platformID", this.platformID);
            dic.Add("merchNo", this.merchNo);
            dic.Add("orderNo", this.OrderID.Substring(8));
            dic.Add("tradeDate", date);
            dic.Add("amt", this.Money.ToString("0.00"));
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            dic.Add("returnUrl", this.GetUrl(this.returnUrl));
            dic.Add("merchParam", this.Description);
            dic.Add("tradeSummary", this.Name);
            dic.Add("signMsg", sign);
            dic.Add("bankCode", this.choosePayType);
            dic.Add("choosePayType", this.choosePayType);


            sb.AppendFormat("<form action=\"{0}\" method=\"post\" id=\"{1}\">", this.GetGateway(this.Shop, GATEWAY), this.GetType().Name);
            sb.Append(string.Join(string.Empty, dic.Select(t => this.CreateInput(t.Key, t.Value))));
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            throw new NotImplementedException();
        }
    }
}
