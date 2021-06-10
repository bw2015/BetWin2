using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using BW.Common.Sites;

using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 西部支付
    /// </summary>
    public sealed class RXlicai : IPayment
    {
        public RXlicai() : base() { }

        public RXlicai(string setting) : base(setting) { }

        [Description("商户ID")]
        public string user_id { get; set; }

        [Description("密钥")]
        public string Ukey { get; set; }

        private string _syn_url = "/handler/payment/RXlicai";
        [Description("异步URL")]
        public string syn_url
        {
            get
            {
                return this._syn_url;
            }
            set
            {
                this._syn_url = value;
            }
        }

        private string _re_url = "/handler/payment/RXlicai";
        [Description("同步URL")]
        public string re_url
        {
            get
            {
                return this._re_url;
            }
            set
            {
                this._re_url = value;
            }
        }

        [Description("支付方式 网银为空 微信扫码:weixin 微信WAP:weixin_wap 支付宝扫码:alipay    支付宝WAP:alipay_wap")]
        public string pay_type { get; set; }

        /// <summary>
        /// 网关地址
        /// </summary>
        private const string GATEWAY = "http://gateway.rxlicai.cn";

        protected override string BankValue
        {
            get
            {
                if (!string.IsNullOrEmpty(this.pay_type)) return this.pay_type;
                return base.BankValue;
            }
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("user_id", this.user_id);
            dic.Add("order_id", this.OrderID);
            dic.Add("user_rmb", this.Money.ToString("0.00"));
            dic.Add("syn_url", this.GetUrl(this.syn_url));
            dic.Add("re_url", this.GetUrl(this.re_url));
            dic.Add("pay_type", this.BankValue);
            dic.Add("ext_info", this.Name);
            dic.Add("sign", MD5.toMD5(string.Join("&", "user_id,order_id,user_rmb,syn_url,pay_type".Split(',').Select(t => string.Format("{0}={1}", t, dic.Get(t, string.Empty)))) + this.Ukey));

            this.BuildForm(dic, GATEWAY, "get");
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string sign = string.Format("user_id={0}&user_order={1}&user_money={2}&user_status={3}&user_ext={4}{5}",
                WebAgent.GetParam("user_id"), WebAgent.GetParam("user_order"), WebAgent.GetParam("user_money"), WebAgent.GetParam("user_status"), WebAgent.GetParam("user_ext"), this.Ukey);

            if (WebAgent.GetParam("user_status") == "1" && WebAgent.GetParam("user_sign").Equals(MD5.toMD5(sign), StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("user_order");
            money = WebAgent.GetParam("user_money", decimal.Zero);
            return systemId;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.pay_type)) return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.CMB, "CMBCHINA");
                dic.Add(BankType.CITIC, "ECITIC");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.PSBC, "POST");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.COMM, "BOCO");
                dic.Add(BankType.NJCB, "NJCB");
                dic.Add(BankType.SPABANK, "PINGANBANK");
                dic.Add(BankType.BOHAIB, "CBHB");
                dic.Add(BankType.HKBEA, "HKBEA");
                dic.Add(BankType.NBBANK, "NBCB");
                dic.Add(BankType.SHBANK, "SHB");
                dic.Add(BankType.CZBANK, "CZ");
                dic.Add(BankType.HZCB, "HZBANK");
                return dic;
            }
        }
    }
}
