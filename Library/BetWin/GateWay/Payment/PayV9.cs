using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Web;
using System.Net;

using BW.Common.Sites;

using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Net;
using SP.Studio.Model;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 银保商务
    /// </summary>
    public class PayV9 : IPayment
    {
        public PayV9() { }

        public PayV9(string setting) : base(setting) { }

        [Description("商户ID")]
        public string partner { get; set; }

        [Description("异步通知地址")]
        public string callbackurl { get; set; }

        [Description("同步通知地址")]
        public string hrefbackurl { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        private string _gateway = "http://wytj.9vpay.com/PayBank.aspx";
        [Description("网关地址")]
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


        private string _type = "BANK";
        [Description("类型(BANK|ALIPAY|TENPAY|WEIXIN)")]
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public override bool IsWechat()
        {
            return this.Type == "WEIXIN";
        }

        public override void GoGateway()
        {
            string banktype = this.BankValue;
            switch (this.Type)
            {
                case "ALIPAY":
                    banktype = "ALIPAY";
                    break;
                case "TENPAY":
                    banktype = "TENPAY";
                    break;
                case "WEIXIN":
                    banktype = "WEIXIN";
                    break;
            }

            string postKey = MD5.toMD5(string.Format("partner={0}&banktype={1}&paymoney={2}&ordernumber={3}&callbackurl={4}{5}",
              this.partner, banktype, this.Money.ToString("0.00"), this.OrderID, this.GetUrl(this.callbackurl), this.Key)).ToLower();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("partner", this.partner);
            dic.Add("banktype", banktype);
            dic.Add("paymoney", this.Money.ToString("0.00"));
            dic.Add("ordernumber", this.OrderID);
            dic.Add("callbackurl", this.GetUrl(this.callbackurl));
            dic.Add("hrefbackurl", this.GetUrl(this.hrefbackurl));
            dic.Add("attach", WebAgent.GetTimeStamp().ToString());
            dic.Add("sign", postKey);

            //partner=18461&banktype=WEIXIN&paymoney=100.00&ordernumber=20170103203903965&callbackurl=http%3a%2f%2fwww.boqu518.com%2fhandler%2fpayment%2fPayV9&hrefbackurl=http%3a%2f%2fwww.boqu518.com%2fhandler%2fpayment%2fPayV9&attach=1483447143&sign=3057044b05af79b9e59e4a8077830399

            if (this.IsWechat() && (WebAgent.IsWechat() || WebAgent.GetParam("wechat", 0) == 1))
            {
                using (WebClient wc = new WebClient())
                {
                    string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
                    wc.Headers.Add(HttpRequestHeader.Referer, HttpContext.Current.Request.UrlReferrer == null ? HttpContext.Current.Request.Url.ToString() : HttpContext.Current.Request.UrlReferrer.ToString());
                    string url = string.Concat(this.Gateway, "?", data);
                    string result = NetAgent.DownloadData(url, Encoding.UTF8, wc);
                    Regex regexSuccess = new Regex(@"<div class=""divCode"">[^\<]+<img\s+src=""(?<Code>[^""]+)""",RegexOptions.Multiline);
                    if (regexSuccess.IsMatch(result))
                    {
                        HttpContext.Current.Response.Write(true, "微信支付", new
                        {
                            data = regexSuccess.Match(result).Groups["Code"].Value
                        });
                    }
                    else
                    {
                        HttpContext.Current.Response.Write(false, result);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<form action=\"{0}\" method=\"get\" id=\"{1}\">", this.Gateway, this.GetType().Name);
            sb.Append(string.Join(string.Empty, dic.Select(t => this.CreateInput(t.Key, t.Value))));
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> if(document.getElementById(\"{0}\")) document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);

            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("orderstatus") != "1") return false;

            string postKey = MD5.toMD5(string.Format("partner={0}&ordernumber={1}&orderstatus={2}&paymoney={3}{4}",
                this.partner,
                WebAgent.GetParam("ordernumber"),
                WebAgent.GetParam("orderstatus"),
                WebAgent.GetParam("paymoney"),
                this.Key)).ToLower();
            if (WebAgent.GetParam("sign") == postKey)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("sysnumber");
            money = WebAgent.GetParam("paymoney", decimal.Zero);
            return WebAgent.GetParam("ordernumber");
        }

        private Dictionary<BankType, string> _code;

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.Type != "BANK") return null;
                if (_code == null)
                {
                    _code = new Dictionary<BankType, string>();
                    _code.Add(BankType.ICBC, "ICBC");
                    _code.Add(BankType.ABC, "ABC");
                    _code.Add(BankType.CCB, "CCB");
                    _code.Add(BankType.CMB, "CMB");
                    _code.Add(BankType.BOC, "BOC");
                    _code.Add(BankType.BJBANK, "BCCB");
                    _code.Add(BankType.COMM, "BOCO");
                    _code.Add(BankType.CIB, "CIB");
                    _code.Add(BankType.NJCB, "NJCB");
                    _code.Add(BankType.CMBC, "CMBC");
                    _code.Add(BankType.CEB, "CEB");
                    _code.Add(BankType.SPABANK, "PINGANBANK");
                    _code.Add(BankType.BOHAIB, "CBHB");
                    _code.Add(BankType.HKBEA, "HKBEA");
                    _code.Add(BankType.NBBANK, "NBCB");
                    _code.Add(BankType.CITIC, "CTTIC");
                    _code.Add(BankType.GDB, "GDB");
                    _code.Add(BankType.SHBANK, "SHB");
                    _code.Add(BankType.SPDB, "SPDB");
                    _code.Add(BankType.PSBC, "PSBS");
                    _code.Add(BankType.HXBANK, "HXB");
                    _code.Add(BankType.BJRCB, "BJRCB");
                    _code.Add(BankType.SHRCB, "SRCB");
                    _code.Add(BankType.CZCB, "CZB");
                }
                return _code;
            }
        }

        public override string ShowCallback()
        {
            return "ok";
        }
    }
}
