using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Json;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 中诺支付
    /// </summary>
    public class ZhongNuo : IPayment
    {
        public ZhongNuo() : base() { }

        public ZhongNuo(string setting) : base(setting) { }

        [Description("商户编号")]
        public string merchantNo { get; set; }

        private string _notifyUrl = "/handler/payment/ZhongNuo";
        [Description("异步通知")]
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

        private string _callbackUrl = "/handler/payment/ZhongNuo";
        [Description("页面回调")]
        public string callbackUrl
        {
            get
            {
                return this._callbackUrl;
            }
            set
            {
                this._callbackUrl = value;
            }
        }

        [Description("支付类型")]
        public string payType { get; set; }

        [Description("密钥")]
        public string KEY { get; set; }

        private string _gateway = "http://39.108.64.56:8080/wappay/payapi/order";
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

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("wtfOrderNo");
            money = WebAgent.GetParam("orderAmount", decimal.Zero) / 100M;
            return WebAgent.GetParam("orderNo");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merchantNo", this.merchantNo);
            dic.Add("orderAmount", ((int)this.Money * 100).ToString());
            dic.Add("orderNo", this.OrderID);
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            dic.Add("callbackUrl", this.GetUrl(this.callbackUrl));
            dic.Add("payType", this.payType);
            dic.Add("productName", this.Name);
            dic.Add("productDesc", this.GetType().Name);
            switch (this.payType)
            {
                case "13":
                    dic.Add("deviceType", "01");
                    dic.Add("mchAppId", this.GetType().Name);
                    dic.Add("mchAppName", this.GetType().Name);
                    break;
                case "":
                    dic.Add("bankName", this.BankValue);
                    dic.Add("currencyType", "CNY");
                    dic.Add("cardType", "1");
                    dic.Add("businessType", "01");
                    break;
            }
            foreach (string key in dic.Where(t => string.IsNullOrEmpty(t.Value)).Select(t => t.Key).ToArray())
            {
                dic.Remove(key);
            }
            string signStr = dic.Sort().ToQueryString() + this.KEY;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());
            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);

            string status = JsonAgent.GetValue<string>(result, "status");

            if (status != "T")
            {
                context.Response.Write(JsonAgent.GetValue<string>(result, "errMsg") ?? result);
                context.Response.Write("<!-- " + dic.ToQueryString() + "-->");
                context.Response.Write("<!-- " + result + "-->");
                return;
            }

            string payUrl = JsonAgent.GetValue<string>(result, "payUrl");

            switch (this.payType)
            {
                case "2":
                    this.CreateWXCode(payUrl);
                    break;
                case "13":
                    if (WebAgent.IsMobile())
                    {
                        this.BuildForm(payUrl);
                    }
                    else
                    {
                        this.CreateQRCode(payUrl);
                    }
                    break;
                case "":
                    this.BuildForm(payUrl);
                    break;
                default:
                    this.CreateQRCode(payUrl);
                    break;
            }
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.payType != "") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                return dic;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in new string[]
            {
                "merchantNo","orderAmount","orderNo","wtfOrderNo","orderStatus","payTime",
                "productName","productDesc","remark"
            })
            {
                string value = WebAgent.GetParam(key);
                if (!string.IsNullOrEmpty(value)) dic.Add(key, value);
            }
            string sign = WebAgent.GetParam("sign");
            string status = WebAgent.GetParam("orderStatus");
            if (status != "SUCCESS") return false;

            string signStr = dic.Sort().ToQueryString() + this.KEY;
            if (sign == MD5.toMD5(signStr).ToLower())
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string ShowCallback()
        {
            return "OK";
        }
    }
}
