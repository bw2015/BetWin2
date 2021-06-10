using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using SP.Studio.Security;
using System.ComponentModel;

using SP.Studio.Net;
using SP.Studio.Json;
using SP.Studio.Web;

namespace BW.GateWay.Payment
{
    public class HuiHePay : IPayment
    {
        public HuiHePay() : base() { }

        public HuiHePay(string setting) : base(setting) { }

        private const string Method = "trade.page.pay";

        private const string Format = "JSON";

        private const string Charset = "UTF-8";

        private const string Version = "1.0";

        private const string SignType = "MD5";

        private string _gateway = "https://pay.huihepay.com/Gateway";
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
        public string AppId { get; set; }

        [Description("支付类型")]
        public int PayType { get; set; }

        private string _notifyUrl = "/handler/payment/HuiHePay";
        [Description("通知地址")]
        public string NotifyUrl
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
        public string Key { get; set; }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }


        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("TradeNo");
            money = WebAgent.GetParam("TotalAmount", decimal.Zero);
            return WebAgent.GetParam("OutTradeNo");
        }

        public override void GoGateway()
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("AppId", this.AppId);
            dic.Add("Method", Method);
            dic.Add("Format", Format);
            dic.Add("Charset", Charset);
            dic.Add("Version", Version);
            dic.Add("Timestamp", timestamp);
            dic.Add("PayType", this.PayType.ToString());
            if (this.PayType == 1)
                dic.Add("BankCode", this.BankValue);
            dic.Add("OutTradeNo", this.OrderID);
            dic.Add("TotalAmount", this.Money.ToString("0.00"));
            dic.Add("Subject", this.Name);
            dic.Add("Body", this.Name);
            dic.Add("NotifyUrl", this.GetUrl(this.NotifyUrl));
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
            dic.Add("Sign", MD5.toMD5(signStr));
            dic.Add("SignType", SignType);
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);

            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("Code") || !ht.ContainsKey("QrCode") || ht["Code"].ToString() != "0")
            {
                HttpContext.Current.Response.Write(result);
                return;
            }

            string code = ht["QrCode"].ToString();

            switch (this.PayType)
            {
                case 2:
                case 7:
                    this.CreateWXCode(code);
                    break;
                case 4:
                case 5:
                case 6:
                    this.CreateAliCode(code);
                    break;
                case 3:
                    this.CreateQQCode(code);
                    break;
                default:
                    HttpContext.Current.Response.Write("PayType参数错误=" + this.PayType);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in new string[] { "Code", "Message", "AppId", "TradeNo", "OutTradeNo", "TotalAmount", "PassbackParams" })
            {
                dic.Add(key, WebAgent.GetParam(key));
            }

            string signStr = string.Join("&", dic.Where(t => !string.IsNullOrEmpty(t.Value)).OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
            if (MD5.toMD5(signStr) != WebAgent.GetParam("Sign"))
            {
                return false;
            }

            callback.Invoke();
            return true;

        }
    }
}
