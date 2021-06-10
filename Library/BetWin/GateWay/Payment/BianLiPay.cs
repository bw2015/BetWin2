using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;
using SP.Studio.Array;

using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Text;
using SP.Studio.Web;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 便利付
    /// </summary>
    public class BianLiPay : IPayment
    {
        public BianLiPay() : base() { }

        public BianLiPay(string setting) : base(setting) { }

        public string _gateway = "http://p.bianlipay.com/Pay_Index.html";
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
        public string pay_memberid { get; set; }

        [Description("银行编码")]
        public string pay_bankcode { get; set; }

        [Description("通道编码")]
        public string pay_tongdao { get; set; }

        private string _pay_notifyurl = "/handler/payment/BianLiPay";
        [Description("通知地址")]
        public string pay_notifyurl
        {
            get
            {
                return this._pay_notifyurl;
            }
            set
            {
                this._pay_notifyurl = value;
            }
        }

        private string _pay_callbackurl = "/handler/payment/BianLiPay";
        [Description("跳转地址")]
        public string pay_callbackurl
        {
            get
            {
                return this._pay_callbackurl;
            }
            set
            {
                this._pay_callbackurl = value;
            }
        }


        [Description("密钥")]
        public string Key { get; set; }


        public override string ShowCallback()
        {
            return "OK";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("orderid");
            money = WebAgent.GetParam("amount", decimal.Zero);
            return systemId;
        }

        public override void GoGateway()
        {
            //pay_service
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("pay_memberid", this.pay_memberid);
            dic.Add("pay_orderid", this.OrderID);
            dic.Add("pay_amount", this.Money.ToString("0.00"));
            dic.Add("pay_applydate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("pay_bankcode", this.pay_bankcode);
            dic.Add("pay_notifyurl", this.GetUrl(this.pay_notifyurl));
            dic.Add("pay_callbackurl", this.GetUrl(this.pay_callbackurl));
            string sign = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + this.Key;
            dic.Add("pay_tongdao", this.pay_tongdao);
            dic.Add("pay_productname", this.Name);
            dic.Add("sign", MD5.toMD5(sign));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8).Replace(@"\", "");

            string code = StringAgent.GetString(result, @"""pay_url"":""", "\"");
            if (string.IsNullOrEmpty(code))
            {
                HttpContext.Current.Response.Write(result);
                return;
            }
            switch (this.pay_bankcode)
            {
                case "QQ_NATIVE":
                    this.CreateQQCode(code);
                    break;
                case "ALIPAY_NATIVE":
                    this.CreateAliCode(code);
                    break;
                case "WEIXIN_NATIVE":
                    this.CreateWXCode(code);
                    break;
                case "KUAIJIE":
                    if (!WebAgent.IsMobile())
                    {
                        code = StringAgent.GetString(result, "\"pay_QR\":\"", "\"");
                    }
                    this.BuildForm(new Dictionary<string, string>(), code);
                    break;
                default:
                    HttpContext.Current.Response.Write(result);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("returncode") != "00") return false;
            string sign = string.Join("&",
                new string[] { "memberid", "orderid", "amount", "datetime", "returncode" }.OrderBy(t => t).Select(t => string.Format("{0}={1}", t, WebAgent.GetParam(t)))) + "&key=" + this.Key;

            if (MD5.toMD5(sign) != WebAgent.GetParam("sign")) return false;
            callback.Invoke();
            return true;
        }
    }
}
