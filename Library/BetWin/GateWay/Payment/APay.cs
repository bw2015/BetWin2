using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Web;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Json;
using System.Web;
namespace BW.GateWay.Payment
{
    public class APay : IPayment
    {
        public APay() : base() { }

        public APay(string setting) : base(setting) { }

        private string _gateway = "https://gateway.aabill.com/quickGateWayPay/initPay";
        [Description("支付网关")]
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


        [Description("支付Key")]
        public string payKey { get; set; }

        [Description("产品类型")]
        public string productType { get; set; }

        private string _returnUrl = "/handler/payment/APay";
        [Description("同步通知")]
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

        private string _notifyUrl = "/handler/payment/APay";
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

        [Description("密钥")]
        public string paySecret { get; set; }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }
        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("orderPrice", decimal.Zero);
            systemId = WebAgent.GetParam("trxNo");
            return WebAgent.GetParam("outTradeNo");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("payKey", this.payKey);
            dic.Add("orderPrice", this.Money.ToString("0.00"));
            dic.Add("outTradeNo", this.OrderID);
            dic.Add("productType", this.productType);
            dic.Add("orderTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("productName", this.Name);
            dic.Add("orderIp", IPAgent.IP);
            dic.Add("returnUrl", this.GetUrl(this.returnUrl));
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            dic.Add("sign", MD5.toMD5(signStr + "&paySecret=" + this.paySecret));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = null;
            switch (this.productType)
            {
                case "10000102":
                case "10000103":
                case "10000101":
                    if (this.GetCode(data, out result))
                    {
                        this.CreateWXCode(result);
                    }
                    else
                    {
                        HttpContext.Current.Response.Write(result);
                    }
                    break;
                case "20000302":
                case "20000303":
                case "20000301":
                    if (this.GetCode(data, out result))
                    {
                        this.CreateAliCode(result);
                    }
                    else
                    {
                        HttpContext.Current.Response.Write(result);
                    }
                    break;
                case "70000202":
                case "70000203":
                case "70000201":
                    if (this.GetCode(data, out result))
                    {
                        this.CreateQQCode(result);
                    }
                    else
                    {
                        HttpContext.Current.Response.Write(result);
                    }
                    break;
                default:
                    this.BuildForm(dic, this.Gateway);
                    break;
            }

        }

        private bool GetCode(string data, out string result)
        {
            string json = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            result = null;
            Hashtable ht = JsonAgent.GetJObject(json);
            if (ht == null || !ht.ContainsKey("payMessage") || !ht.ContainsKey("resultCode") || ht["resultCode"].ToString() != "0000")
            {
                result = json + "\n" + data;
                return false;
            }
            result = ht["payMessage"].ToString();
            return true;
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("tradeStatus") != "SUCCESS") return false;
            string sign = WebAgent.GetParam("sign");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            switch (HttpContext.Current.Request.HttpMethod)
            {
                case "GET":
                    foreach (string key in HttpContext.Current.Request.QueryString.AllKeys.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        dic.Add(key, WebAgent.QS(key));
                    }
                    break;
                case "POST":
                    foreach (string key in HttpContext.Current.Request.Form.AllKeys.Where(t => !string.IsNullOrEmpty(t)))
                    {
                        dic.Add(key, WebAgent.QF(key));
                    }
                    break;
            }
            if (dic.ContainsKey("sign")) dic.Remove("sign");
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&paySecret=" + this.paySecret;
            if (MD5.toMD5(signStr) == sign)
            {
                callback.Invoke();
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
