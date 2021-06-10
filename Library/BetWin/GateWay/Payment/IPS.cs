using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using SP.Studio.Web;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 环迅
    /// </summary>
    public class IPS : IPayment
    {
        public IPS() : base() { }

        public IPS(string setting) : base(setting) { }

        [Description("商户编号")]
        public string Mer_code { get; set; }

        [Description("密钥")]
        public string Mer_key { get; set; }

        [Description("返回地址")]
        public string Merchanturl { get; set; }

        [Description("通知地址")]
        public string ServerUrl { get; set; }

        private string _url = "https://pay.ips.com.cn/ipayment.aspx";
        [Description("支付网关")]
        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
            }
        }

        public override void GoGateway()
        {
            string billDate = DateTime.Now.ToString("yyyyMMdd");
            string SignMD5 = SP.Studio.Security.MD5.toMD5("billno" + this.OrderID + "currencytype" + "RMB" +
                "amount" + this.Money.ToString("0.00") + "date" + billDate + "orderencodetype" + 5 + Mer_key).ToLower();


            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>正在提交...</title></head><body>");
            sb.AppendFormat("<form name=\"{1}\" method=\"post\" action=\"{0}\" id=\"{1}\">", this.Url, this.GetType().Name);
            sb.Append(this.CreateInput("Mer_code", this.Mer_code));
            sb.Append(this.CreateInput("Billno", this.OrderID));
            sb.Append(this.CreateInput("Amount", this.Money.ToString("0.00")));
            sb.Append(this.CreateInput("Date", billDate));
            sb.Append(this.CreateInput("Currency_Type", "RMB"));
            sb.Append(this.CreateInput("Gateway_Type", "01"));
            sb.Append(this.CreateInput("Lang", "GB"));
            sb.Append(this.CreateInput("Merchanturl", this.Merchanturl));
            sb.Append(this.CreateInput("OrderEncodeType", "5"));
            sb.Append(this.CreateInput("RetEncodeType", "17"));
            sb.Append(this.CreateInput("Rettype", "1"));
            sb.Append(this.CreateInput("ServerUrl", this.ServerUrl));
            sb.Append(this.CreateInput("SignMD5", SignMD5));

            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);
            sb.Append("</body></html>");


            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string billno = WebAgent.GetParam("billno");
            string currencytype = WebAgent.GetParam("currencytype");
            string amount = WebAgent.GetParam("amount");
            string date = WebAgent.GetParam("date");
            string succ = WebAgent.GetParam("succ");
            string ipsbillno = WebAgent.GetParam("ipsbillno");
            string retencodetype = WebAgent.GetParam("retencodetype");

            if (succ != "Y") return false;

            string sign = SP.Studio.Security.MD5.toMD5("billno" + billno + "currencytype" + currencytype +
                "amount" + amount + "date" + date + "succ" + succ + "ipsbillno" + ipsbillno + "retencodetype" + retencodetype + Mer_key).ToLower();

            if (sign == WebAgent.GetParam("signature"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("amount", 0.00M);
            systemId = WebAgent.GetParam("ipsbillno");
            return WebAgent.GetParam("billno");
        }
    }
}
