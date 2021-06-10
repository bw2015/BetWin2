using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;

using BW.Common.Sites;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Model;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 智刷支付
    /// </summary>
    public class ZhiShuaPay : IPayment
    {
        public ZhiShuaPay() : base() { }

        public ZhiShuaPay(string setting) : base(setting) { }

        private string _gateway = "http://pay.zhishuapay.com/Bank/index.aspx";
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

        [Description("商户ID")]
        public string parter { get; set; }

        [Description("类型（wx|alipay|bank)")]
        public string paytype { get; set; }

        private string _callbackurl = "/handler/payment/ZhiShuaPay";
        [Description("通知地址")]
        public string callbackurl
        {
            get
            {
                return this._callbackurl;
            }
            set
            {
                this._callbackurl = value;
            }
        }

        private string _hrefbackurl = "/handler/payment/ZhiShuaPay";
        [Description("同步地址")]
        public string hrefbackurl
        {
            get
            {
                return this._hrefbackurl;
            }
            set
            {
                this._hrefbackurl = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        [Description("备注")]
        public string Remark { get; set; }

        public override bool IsWechat()
        {
            return this.paytype == "1004" && WebAgent.GetParam("iswechat", 0) == 1;
        }

        public override void GoGateway()
        {
            string type;
            switch (this.paytype)
            {
                case "wx":
                    type = "1004";
                    break;
                case "alipay":
                    type = "992";
                    break;
                default:
                    type = this.BankValue;
                    break;
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parter", this.parter);
            dic.Add("type", type);
            dic.Add("value", this.Money.ToString("0.00"));
            dic.Add("orderid", this.OrderID);
            dic.Add("callbackurl", this.GetUrl(this.callbackurl));
            dic.Add("hrefbackurl", this.GetUrl(this.hrefbackurl));
            dic.Add("payerIp", IPAgent.IP);
            dic.Add("attach", this.Description);
            string signStr = string.Format("parter={0}&type={1}&value={2}&orderid={3}&callbackurl={4}{5}", dic["parter"], dic["type"], dic["value"], dic["orderid"], dic["callbackurl"], this.Key);
            dic.Add("sign", MD5.Encryp(signStr, "gb2312").ToLower());

            if (this.IsWechat())
            {
                string result = NetAgent.UploadData(this.Gateway, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8);
                HttpContext.Current.Response.Write(false, "发生错误", new
                {
                    data = result
                });
            }
            else
            {
                this.BuildForm(dic, this.Gateway);
            }
        }

        public override string ShowCallback()
        {
            return "opstate=0";
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("opstate", 1) != 0) return false;
            string sign = string.Format("orderid={0}&opstate={1}&ovalue={2}{3}", WebAgent.GetParam("orderid"),
                WebAgent.GetParam("opstate"),
                WebAgent.GetParam("ovalue"),
                this.Key);
            if (WebAgent.GetParam("sign") == MD5.Encryp(sign, "gb2312").ToLower())
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("ovalue", decimal.Zero);
            systemId = WebAgent.GetParam("sysorderid");
            return WebAgent.GetParam("orderid");
        }

        private Dictionary<BankType, string> _bankCode;
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.paytype == "wx" || this.paytype == "alipay") return null;
                if (_bankCode == null)
                {
                    this._bankCode = new Dictionary<BankType, string>();
                    this._bankCode.Add(BankType.CITIC, "962");
                    this._bankCode.Add(BankType.BOC, "963");
                    this._bankCode.Add(BankType.ABC, "964");
                    this._bankCode.Add(BankType.CCB, "965");
                    this._bankCode.Add(BankType.ICBC, "967");
                    this._bankCode.Add(BankType.CZBANK, "968");
                    this._bankCode.Add(BankType.CMB, "970");
                    this._bankCode.Add(BankType.PSBC, "971");
                    this._bankCode.Add(BankType.CIB, "972");
                    this._bankCode.Add(BankType.SPDB, "977");
                    this._bankCode.Add(BankType.SPABANK, "978");
                    this._bankCode.Add(BankType.CMBC, "980");
                    this._bankCode.Add(BankType.COMM, "981");
                    this._bankCode.Add(BankType.HXBANK, "982");
                    this._bankCode.Add(BankType.GDB, "985");
                    this._bankCode.Add(BankType.CEB, "986");
                }
                return _bankCode;
            }
        }
    }
}
