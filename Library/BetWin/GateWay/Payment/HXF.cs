using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using SP.Studio.Security;
using SP.Studio.Web;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 好想付
    /// </summary>
    public class HXF : IPayment
    {
        public HXF() : base() { }

        public HXF(string setting) : base(setting) { }

        private string _gateway = "https://www.hxf-pay.com/payApi";
        [Description("网关地址")]
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
        public string mchid { get; set; }

        [Description("商户密钥")]
        public string mchsecrect { get; set; }

        [Description("商户公钥")]
        public string pubsecrect { get; set; }

        private string _return_url = "/handler/payment/HXF";
        [Description("通知Url")]
        public string return_url
        {
            get
            {
                return this._return_url;
            }
            set
            {
                this._return_url = value;
            }
        }

        private string _notify_url = "/handler/payment/HXF";
        [Description("异步Url")]
        public string notify_url
        {
            get
            {
                return this._notify_url;
            }
            set
            {
                this._notify_url = value;
            }
        }

        [Description("支付方式 wxpay|alipay|bank")]
        public string pay_type { get; set; }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("total_fee", decimal.Zero);
            systemId = WebAgent.GetParam("out_trade_no");
            return systemId;
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("mchid", this.mchid);
            dic.Add("mchsecrect", this.mchsecrect);
            dic.Add("out_trade_no", this.OrderID);
            dic.Add("total_fee", this.Money.ToString("0.00"));
            dic.Add("return_url", this.GetUrl(this.return_url));
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("pay_type", this.pay_type);
            dic.Add("product_name", this.Name);
            dic.Add("remark", this.Name);
            dic.Add("out_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            dic.Add("sign", MD5.toSHA1Sign(signStr));
            dic.Remove("mchsecrect");

            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            NameValueCollection post = this.context.Request.HttpMethod == "GET" ? this.context.Request.QueryString : this.context.Request.Form;
            foreach (string key in post)
            {
                dic.Add(key, post[key]);
            }
            string sign = dic["sign"];
            dic.Remove("sign");
            dic.Add("pubsecrect", this.pubsecrect);
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            if (sign != MD5.toSHA1Sign(signStr)) return false;
            callback.Invoke();
            return true;
        }
    }
}
