using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Web;
using SP.Studio.Security;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 萝卜支付
    /// </summary>
    public class LuoBo : IPayment
    {
        public LuoBo() : base() { }

        public LuoBo(string setting) : base(setting) { }

        private string _gateway = "http://gt.luobofu.net/chargebank.aspx";
        [Description("网银网关")]
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

        private string _callbackurl = "/handler/payment/LuoBo";
        [Description("异步通知")]
        public string callbackurl
        {
            get { return this._callbackurl; }
            set { this._callbackurl = value; }
        }

        private string _hrefbackurl = "/handler/payment/LuoBo";
        [Description("同步通知")]
        public string hrefbackurl
        {
            get { return this._hrefbackurl; }
            set { this._hrefbackurl = value; }
        }

        [Description("密钥")]
        public string Key { get; set; }

        /// <summary>
        /// 支付宝 992  微信扫码：993   QQ扫码：995
        /// </summary>
        [Description("类型")]
        public string Type { get; set; }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parter", this.parter);
            if (this.Type == "992" || this.Type == "993" || this.Type == "995")
            {
                dic.Add("type", this.Type);
            }
            else
            {
                dic.Add("type", this.BankValue);
            }
            dic.Add("value", this.Money.ToString("0.00"));
            dic.Add("orderid", this.OrderID);
            dic.Add("callbackurl", this.GetUrl(this.callbackurl));
            dic.Add("hrefbackurl", this.GetUrl(this.hrefbackurl));
            dic.Add("payerIp", IPAgent.IP);
            dic.Add("attach", this.Description);

            string sign = string.Format("parter={0}&type={1}&value={2}&orderid ={3}&callbackurl={4}{5}", this.parter, dic["type"], dic["value"], dic["orderid"], dic["callbackurl"], this.Key);
            dic.Add("sign", MD5.toMD5(sign).ToLower());

            this.BuildForm(dic, this.Gateway, "GET");
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

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("ovalue", decimal.Zero);
            systemId = WebAgent.GetParam("ekaorderid");
            return WebAgent.GetParam("orderid");
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.Type == "992" || this.Type == "993" || this.Type == "995") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "101");
                dic.Add(BankType.CCB, "102");
                dic.Add(BankType.BOC, "103");
                dic.Add(BankType.ABC, "104");
                dic.Add(BankType.COMM, "105");
                dic.Add(BankType.CMB, "106");
                dic.Add(BankType.CITIC, "107");
                dic.Add(BankType.CMBC, "108");
                dic.Add(BankType.CIB, "109");
                dic.Add(BankType.SPDB, "110");
                dic.Add(BankType.PSBC, "111");
                dic.Add(BankType.CEB, "112");
                dic.Add(BankType.SPABANK, "113");
                dic.Add(BankType.HXBANK, "114");
                dic.Add(BankType.BJBANK, "115");
                dic.Add(BankType.GDB, "116");
                dic.Add(BankType.SHBANK, "117");
                dic.Add(BankType.CZBANK, "118");
                dic.Add(BankType.NJCB, "119");
                dic.Add(BankType.NBBANK, "120");
                return dic;
            }
        }

    }
}
