using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;

using SP.Studio.Model;
using SP.Studio.Web;

using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class xBeiPay : IPayment
    {
        public xBeiPay() : base() { }

        public xBeiPay(string setting) : base(setting) { }

        private const string Version = "V1.0";

        /// <summary>
        /// 网关地址
        /// </summary>
        private const string GATEWAY = "http://gateway.xbeipay.com/Gateway/XbeiPay";

        [Description("通知地址")]
        public string NotifyUrl { get; set; }

        [Description("商户号")]
        public string MerchantCode { get; set; }

        [Description("密钥")]
        public string TokenKey { get; set; }

        /// <summary>
        /// 支付模式
        /// </summary>
        [Description("1:微信，2：网银")]
        public int Type { get; set; }

        public override void GoGateway()
        {
            string OrderDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            string payCode = this.Type == 1 ? "100040" : this.BankValue;

            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>正在提交...</title></head><body>");
            sb.AppendFormat("<form name=\"{1}\" method=\"post\" action=\"{0}\" id=\"{1}\">", GATEWAY, this.GetType().Name);
            sb.Append(this.CreateInput("Version", Version));
            sb.Append(this.CreateInput("MerchantCode", this.MerchantCode));
            sb.Append(this.CreateInput("OrderId", this.OrderID));    ////版本 当前为4.0请勿修改 
            sb.Append(this.CreateInput("Amount", this.Money.ToString("0.00")));   //加密方式默认1 MD5
            sb.Append(this.CreateInput("AsyNotifyUrl", this.NotifyUrl));
            sb.Append(this.CreateInput("SynNotifyUrl", this.NotifyUrl));
            sb.Append(this.CreateInput("OrderDate", OrderDate));
            sb.Append(this.CreateInput("TradeIp", SP.Studio.Web.IPAgent.IP));
            sb.Append(this.CreateInput("PayCode", payCode));
            sb.Append(this.CreateInput("GoodsName", this.Name));
            sb.Append(this.CreateInput("GoodsDescription", this.Description));
            sb.Append(this.CreateInput("SignValue", this.GetSign(OrderDate, payCode)));
            sb.Append("</form>");
            sb.AppendFormat("<script language=\"javascript\" type=\"text/javascript\"> document.getElementById(\"{0}\").submit(); </script>", this.GetType().Name);
            sb.Append("</body></html>");


            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(sb);
            HttpContext.Current.Response.End();
        }

        private string GetSign(string orderDate, string payCode)
        {
            string str = string.Concat("Version=[", Version,
                "]MerchantCode=[", this.MerchantCode,
                "]OrderId=[", this.OrderID,
                "]Amount=[", this.Money.ToString("0.00"),
                "]AsyNotifyUrl=[", this.NotifyUrl, "]SynNotifyUrl=[", this.NotifyUrl,
                "]OrderDate=[", orderDate,
                "]TradeIp=[", SP.Studio.Web.IPAgent.IP, "]PayCode=[", payCode, "]TokenKey=[", this.TokenKey, "]");

            return SP.Studio.Security.MD5.toMD5(str);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("State") != "8888") return false;

            string str = string.Concat("Version=[", Version, "]MerchantCode=[", MerchantCode, "]OrderId=[", WebAgent.GetParam("OrderId"),
                "]OrderDate=[", WebAgent.GetParam("OrderDate") + "]TradeIp=[", WebAgent.GetParam("TradeIp"), "]SerialNo=[" + WebAgent.GetParam("SerialNo"),
                "]Amount=[", WebAgent.GetParam("Amount"), "]PayCode=[", WebAgent.GetParam("PayCode") + "]State=[", WebAgent.GetParam("State"), "]FinishTime=[", WebAgent.GetParam("FinishTime"),
                "]TokenKey=[", this.TokenKey, "]");
            string sign = SP.Studio.Security.MD5.toMD5(str);
            if (sign != WebAgent.GetParam("SignValue")) return false;

            callback.Invoke();
            return true;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("Amount", 0.00M);
            systemId = WebAgent.GetParam("SerialNo");

            return WebAgent.GetParam("OrderId");
        }


        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.Type != 2) return null;
                Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                _code.Add(BankType.ICBC, "100012");
                _code.Add(BankType.ABC, "100013");
                _code.Add(BankType.CCB, "100014");
                _code.Add(BankType.COMM, "100015");
                _code.Add(BankType.CMB, "100016");
                _code.Add(BankType.BOC, "100017");
                _code.Add(BankType.CMBC, "100018");
                _code.Add(BankType.HXBANK, "100019");
                _code.Add(BankType.CIB, "100020");
                _code.Add(BankType.SPDB, "100021");
                _code.Add(BankType.CITIC, "100023");
                _code.Add(BankType.CEB, "100024");
                _code.Add(BankType.PSBC, "100025");
                _code.Add(BankType.BJBANK, "100026");
                _code.Add(BankType.TCCB, "天津银行");
                _code.Add(BankType.SPABANK, "100030");
                return _code;
            }
        }
    }
}
