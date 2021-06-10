using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Security;
using SP.Studio.Web;
using System.Web;
using SP.Studio.Net;

namespace BW.GateWay.Payment
{
    public sealed class PPL : IPayment
    {
        public PPL() : base() { }

        public PPL(string setting) : base(setting) { }

        private const string version = "1.0";

        private const string sign_type = "MD5";

        private const string GATEWAY = "http://pay.pplsvc.com/gateway";

        [Description("商户ID")]
        public string merchant_id { get; set; }

        private string _service_type = "WECHAT";
        [Description("支付方式")]
        public string service_type
        {
            get
            {
                return this._service_type;
            }
            set
            {
                this._service_type = value;
            }
        }


        private string _notify_url = "/handler/payment/PPL";
        [Description("回调地址")]
        public string notify_url
        {
            get
            {
                return _notify_url;
            }
            set
            {
                _notify_url = value;
            }
        }

        [Description("密钥")]
        public string key { get; set; }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merchant_id", this.merchant_id);
            dic.Add("version", version);
            dic.Add("order_no", this.OrderID);
            dic.Add("product_name", this.Name);
            dic.Add("service_type", this.service_type);
            dic.Add("amount", this.Money.ToString("0.00"));
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            string sign = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value, Encoding.UTF8)))) + "&key=" + this.key;
            sign = Regex.Replace(sign, @"%\w{2}", t => t.Value.ToUpper());
            dic.Add("sign_type", sign_type);
            dic.Add("sign", MD5.toMD5(sign).ToLower());

            //dic.Add("_sign", sign);
            this.BuildForm(dic, GATEWAY);
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("trade_status") != "SUCCESS") return false;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merchant_id", WebAgent.GetParam("merchant_id"));
            dic.Add("notify", WebAgent.GetParam("notify"));
            dic.Add("order_no", WebAgent.GetParam("order_no"));
            dic.Add("amount", WebAgent.GetParam("amount"));
            dic.Add("trade_no", WebAgent.GetParam("trade_no"));
            dic.Add("trade_time", WebAgent.GetParam("trade_time"));
            dic.Add("trade_status", WebAgent.GetParam("trade_status"));
            dic.Add("version", WebAgent.GetParam("version"));

            string sign = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value, Encoding.UTF8)))) + "&key=" + this.key;
            sign = Regex.Replace(sign, @"%\w{2}", t => t.Value.ToUpper());

            if (MD5.Encryp(sign) == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("trade_no");
            money = WebAgent.GetParam("amount", decimal.Zero);
            return WebAgent.GetParam("order_no");
        }
    }
}
