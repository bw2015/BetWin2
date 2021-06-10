using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;

using SP.Studio.Web;
using SP.Studio.Security;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class OKPay : IPayment
    {
        public OKPay() : base() { }

        public OKPay(string setting) : base(setting) { }

        private const string version = "1.0";

        private string _gateway = "https://gateway.okfpay.com/Gate/payindex.aspx";
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

        [Description("用户ID")]
        public string partner { get; set; }

        private string _notifyurl = "/handler/payment/OKPay";
        [Description("异步通知")]
        public string notifyurl
        {
            get
            {
                return this._notifyurl;
            }
            set
            {
                this._notifyurl = value;

            }
        }

        private string _returnurl = "/handler/payment/OKPay";
        [Description("同步通知")]
        public string returnurl
        {
            get
            {
                return this._returnurl;
            }
            set
            {
                this._returnurl = value;
            }
        }

        [Description("支付类型")]
        public string paytype { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("orderno");
            money = WebAgent.GetParam("payamount", decimal.Zero);
            return WebAgent.GetParam("orderid");
        }

        public override void GoGateway()
        {
            //sign=md5(partner=1000|orderid=2016032620152314|payamount=100.00|opstate=2|orderno=1603262015231446578|&okfpaytime=2016032521112310|message=success|paytype=ICBC|remark=qq12345|key=8a0ecdfb1e2d4dbe8ea3f3b99762fc)
            string bank = string.IsNullOrEmpty(this.BankValue) ? this.paytype : this.BankValue;
            string sign = string.Format("version={0}&partner={1}&orderid={2}&payamount={3}&payip={4}&notifyurl={5}&returnurl={6}&paytype={7}&remark={8}&key={9}",
                version, this.partner, this.OrderID, this.Money.ToString("0.00"), IPAgent.IP, this.GetUrl(this.notifyurl), this.GetUrl(this.returnurl), bank, this.Description, this.Key);
            sign = MD5.toMD5(sign).ToLower();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("version", version);
            dic.Add("partner", this.partner);
            dic.Add("orderid", this.OrderID);
            dic.Add("payamount", this.Money.ToString("0.00"));
            dic.Add("payip", IPAgent.IP);
            dic.Add("notifyurl", this.GetUrl(this.notifyurl));
            dic.Add("returnurl", this.GetUrl(this.returnurl));
            dic.Add("paytype", bank);
            dic.Add("remark", this.Description);
            dic.Add("sign", sign);
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("opstate") != "2") return false;
            string[] key = new string[] { "version", "partner", "orderid", "payamount", "opstate", "orderno", "okfpaytime", "message", "paytype", "remark", "key" };

            string sign = string.Join("&", key.Select(t => string.Format("{0}={1}", t, t == "key" ? this.Key : WebAgent.GetParam(t))));

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
                if (new string[] { "UNION", "ALIPAY", "TENPAY", "WECHAT" }.Contains(this.paytype)) return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                switch (this.paytype)
                {
                    case "CREDIT":
                        dic.Add(BankType.ICBC, "ICBC_C");
                        dic.Add(BankType.CMB, "CMB_C");
                        dic.Add(BankType.CCB, "CCB_C");
                        dic.Add(BankType.BOC, "BOC_C");
                        dic.Add(BankType.ABC, "ABC_C");
                        dic.Add(BankType.COMM, "BOCM_C");
                        dic.Add(BankType.SPDB, "SPDB_C");
                        dic.Add(BankType.GDB, "CGB_C");
                        dic.Add(BankType.CITIC, "CTITC_C");
                        dic.Add(BankType.CEB, "CEB_C");
                        dic.Add(BankType.CIB, "CIB_C");
                        dic.Add(BankType.SPABANK, "SDB_C");
                        dic.Add(BankType.CMBC, "CMBC_C");
                        dic.Add(BankType.HXBANK, "HXB_C");
                        dic.Add(BankType.PSBC, "PSBC_C");
                        dic.Add(BankType.BJBANK, "BCCB_C");
                        dic.Add(BankType.SHBANK, "SHBANK_C");
                        dic.Add(BankType.BOHAIB, "BOHAI_C");
                        dic.Add(BankType.SHRCB, "SHNS_C");
                        break;
                    default:
                        dic.Add(BankType.ICBC, "ICBC");
                        dic.Add(BankType.CMB, "CMB");
                        dic.Add(BankType.CCB, "CCB");
                        dic.Add(BankType.BOC, "BOC");
                        dic.Add(BankType.ABC, "ABC");
                        dic.Add(BankType.COMM, "BOCM");
                        dic.Add(BankType.SPDB, "SPDB");
                        dic.Add(BankType.GDB, "CGB");
                        dic.Add(BankType.CITIC, "CTITC");
                        dic.Add(BankType.CEB, "CEB");
                        dic.Add(BankType.CIB, "CIB");
                        dic.Add(BankType.SPABANK, "SDB");
                        dic.Add(BankType.CMBC, "CMBC");
                        dic.Add(BankType.HXBANK, "HXB");
                        dic.Add(BankType.PSBC, "PSBC");
                        dic.Add(BankType.BJBANK, "BCCB");
                        dic.Add(BankType.SHBANK, "SHBANK");
                        dic.Add(BankType.BOHAIB, "BOHAI");
                        dic.Add(BankType.SHRCB, "SHNS");
                        break;
                }
                return dic;
            }
        }
    }
}
