using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;
using SP.Studio.Security;
using SP.Studio.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 多宝
    /// </summary>
    public class DUOBAO : IPayment
    {
        public DUOBAO() : base() { }

        public DUOBAO(string setting) : base(setting) { }

        private string _gateway = "https://gw.169.cc/interface/AutoBank/index.aspx";
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
        public string parter { get; set; }

        [Description("类型")]
        public string Type { get; set; }

        private string _callbackurl = "/handler/payment/DUOBAO";
        [Description("异步通知")]
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

        private string _hrefbackurl = "/handler/payment/DUOBAO";
        [Description("同步通知")]
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


        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("sysorderid");
            money = WebAgent.GetParam("ovalue", decimal.Zero);
            return WebAgent.GetParam("orderid");
        }

        public override void GoGateway()
        {
            string type = string.IsNullOrEmpty(this.BankValue) ? this.Type : this.BankValue;
            string sign = string.Format("parter={0}&type={1}&value={2}&orderid={3}&callbackurl={4}{5}",
                this.parter, type, this.Money.ToString("0.00"), this.OrderID, this.GetUrl(this.callbackurl), this.Key);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parter", this.parter);
            dic.Add("type", type);
            dic.Add("value", this.Money.ToString("0.00"));
            dic.Add("orderid", this.OrderID);
            dic.Add("callbackurl", this.GetUrl(this.callbackurl));
            dic.Add("hrefbackurl", this.GetUrl(this.hrefbackurl));
            dic.Add("attach", this.Description);
            dic.Add("sign", MD5.toMD5(sign).ToLower());
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("opstate") != "0") return false;
            string sign = string.Format("orderid={0}&opstate={1}&ovalue={2}{3}", WebAgent.GetParam("orderid"), WebAgent.GetParam("opstate"), WebAgent.GetParam("ovalue"), this.Key);
            if (MD5.toMD5(sign).ToLower() == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Type)) return null;

                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "967");
                dic.Add(BankType.CITIC, "962");
                dic.Add(BankType.BOC, "963");
                dic.Add(BankType.ABC, "964");
                dic.Add(BankType.CCB, "965");
                dic.Add(BankType.CMB, "970");
                dic.Add(BankType.PSBC, "971");
                dic.Add(BankType.CIB, "972");
                dic.Add(BankType.SHRCB, "976");
                dic.Add(BankType.SPDB, "977");
                dic.Add(BankType.NJCB, "979");
                dic.Add(BankType.CMBC, "980");
                dic.Add(BankType.COMM, "981");
                dic.Add(BankType.HZCB, "983");
                dic.Add(BankType.GDB, "985");
                dic.Add(BankType.CEB, "986");
                dic.Add(BankType.BJBANK, "989");
                return dic;
            }
        }
    }
}
