using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Web;
using System.Net;
using System.Collections;

using SP.Studio.Web;
using SP.Studio.Model;
using BW.Common.Sites;
using SP.Studio.Net;
using SP.Studio.Text;
using SP.Studio.Security;
using BankType = BW.Common.Sites.BankType;

using System.Reflection;
namespace BW.GateWay.Payment
{
    /// <summary>
    /// 汇通卡支付
    /// </summary>
    public class HTCard : IPayment
    {
        public HTCard() : base() { }

        public HTCard(string setting) : base(setting) { }

        /// <summary>
        /// 参数字符集编码
        /// </summary>
        private const string input_charset = "UTF-8";


        /// <summary>
        /// 网银 1  微信： WEIXIN
        /// </summary>
        [Description("支付方式 网银:1 微信:WEIXIN/2 支付宝:3 QQ:5")]
        public string Type { get; set; }

        private string _notify_url = "/handler/payment/HTCard";
        /// <summary>
        /// 服务器异步通知地址
        /// </summary>
        [Description("异步通知地址")]
        public string notify_url { get { return this._notify_url; } set { this._notify_url = value; } }

        private string _return_url = "/handler/payment/HTCard";
        /// <summary>
        /// 页面同步跳转通知地址
        /// </summary>
        [Description("同步通知地址")]
        public string return_url { get { return this._return_url; } set { this._return_url = value; } }

        /// <summary>
        /// 商户号
        /// </summary>
        [Description("商户号")]
        public string merchant_code { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string key { get; set; }

        private string _version = "1.0";

        [Description("版本号")]
        public string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }


        /// <summary>
        /// 客户端IP
        /// </summary>
        public string customer_ip
        {
            get
            {
                return IPAgent.IP;
            }
        }

        public override bool IsWechat()
        {
            return this.Type == "WEIXIN" && WebAgent.GetParam("wechat", 0) == 1;
        }

        private string _gateway = "https://pay.41.cn/gateway";
        [Description("接口地址")]
        public string Gateway { get { return _gateway; } set { _gateway = value; } }

        public override void GoGateway()
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("input_charset", input_charset);
            dic.Add("return_url", this.GetUrl(return_url));
            dic.Add("pay_type", this.Type);

            dic.Add("merchant_code", merchant_code);
            dic.Add("order_no", this.OrderID);
            dic.Add("order_time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("req_referer", (HttpContext.Current.Request.UrlReferrer ?? HttpContext.Current.Request.Url).ToString());
            dic.Add("customer_ip", this.customer_ip);
            switch (this.Version)
            {
                case "2.0":
                    if (this.Type == "1") dic.Add("bank_code", this.BankValue);
                    dic.Add("inform_url", this.GetUrl(notify_url));
                    dic.Add("order_amount", AES.AESEncrypts(this.Money.ToString("0.00"), this.key));
                    break;
                default:
                    dic.Add("bank_code", this.Type == "1" ? this.BankValue : this.Type);
                    dic.Add("notify_url", this.GetUrl(notify_url));
                    dic.Add("order_amount", this.Money.ToString("0.00"));
                    dic.Add("product_name", this.Name);
                    break;
            }
            dic.Add("sign", this.Sign(dic));

            switch (this.Type)
            {
                case "ZHIFUBAO":
                    string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
                    string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
                    string code = StringAgent.GetString(result, "<url>", "</url>");
                    if (string.IsNullOrEmpty(code))
                    {
                        HttpContext.Current.Response.Write(false, result);
                    }
                    else
                    {
                        this.CreateAliCode(code);
                    }
                    break;
                default:
                    this.BuildForm(dic, this.Gateway);
                    break;
            }
        }

        private string Sign(SortedDictionary<string, string> dic)
        {
            string queryString = string.Join("&", dic.Where(t => !string.IsNullOrEmpty(t.Value)).OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            return SP.Studio.Security.MD5.Encryp(string.Concat(queryString, "&key=", this.key), input_charset).ToLower();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string trade_status = WebAgent.GetParam("trade_status");
            if (trade_status != "success") return false;
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            switch (this.Version)
            {
                case "2.0":
                    foreach (string key in new string[] { "merchant_code", "order_no", "order_amount", "order_time", "trade_status", "trade_no", "return_params" })
                    {
                        string value = WebAgent.GetParam(key);
                        if (value == "null") value = string.Empty;
                        dic.Add(key, value);
                    }
                    break;
                default:
                    foreach (string key in new string[] { "merchant_code", "notify_type", "order_no", "order_amount", "order_time", "return_params", "trade_no", "trade_time", "trade_status" })
                    {
                        dic.Add(key, WebAgent.GetParam(key));
                    }
                    break;
            }

            string sign = WebAgent.GetParam("sign");
            if (sign == this.Sign(dic))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("order_amount", 0.00M);
            systemId = WebAgent.GetParam("trade_no");
            return WebAgent.GetParam("order_no");
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.Type != "1") return null;
                Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                switch (this.Version)
                {
                    case "2.0":
                        _code.Add(BankType.ABC, "ABC");
                        _code.Add(BankType.BOC, "BOC");
                        _code.Add(BankType.COMM, "BOCOM");
                        _code.Add(BankType.CCB, "CCB");
                        _code.Add(BankType.ICBC, "ICBC");
                        _code.Add(BankType.PSBC, "PSBC");
                        _code.Add(BankType.CMB, "CMBC");
                        _code.Add(BankType.SPDB, "SPDB");
                        _code.Add(BankType.CEB, "CEBBANK");
                        _code.Add(BankType.CITIC, "ECITIC");
                        _code.Add(BankType.SPABANK, "PINGAN");
                        _code.Add(BankType.CMBC, "CMBCS");
                        _code.Add(BankType.HXBANK, "HXB");
                        _code.Add(BankType.GDB, "CGB");
                        _code.Add(BankType.BJBANK, "BCCB");
                        _code.Add(BankType.SHBANK, "BOS");
                        _code.Add(BankType.CIB, "CIB");
                        _code.Add(BankType.NBBANK, "NBCB");
                        _code.Add(BankType.TCCB, "TCCB");
                        _code.Add(BankType.NJCB, "NJCB");
                        break;
                    default:
                        _code.Add(BankType.ABC, "ABC");
                        _code.Add(BankType.BOC, "BOC");
                        _code.Add(BankType.ICBC, "ICBC");
                        _code.Add(BankType.CCB, "CCB");
                        _code.Add(BankType.CEB, "CEBBANK");
                        _code.Add(BankType.COMM, "BOCOM");
                        _code.Add(BankType.CMB, "CMB");
                        _code.Add(BankType.CMBC, "CMBCS");
                        _code.Add(BankType.CITIC, "ECITIC");
                        _code.Add(BankType.PSBC, "PSBC");
                        _code.Add(BankType.SPDB, "SPDB"); ;
                        _code.Add(BankType.SPABANK, "PINGAN");
                        _code.Add(BankType.HXBANK, "HXB");
                        _code.Add(BankType.GDB, "CGB");
                        _code.Add(BankType.CIB, "CIB");
                        break;
                }
                return _code;
            }
        }
    }
}
