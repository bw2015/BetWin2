using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Array;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Json;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class JinYang : IPayment
    {
        public JinYang() : base() { }

        public JinYang(string setting) : base(setting) { }

        private string _gateway = "http://pay.095pay.com/zfapi/order/pay";
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

        [Description("商户ID")]
        public string p1_mchtid { get; set; }

        [Description("支付类型")]
        public string p2_paytype { get; set; }

        private string _p5_callbackurl = "/handler/payment/JinYang";
        [Description("异步回调")]
        public string p5_callbackurl
        {
            get
            {
                return this._p5_callbackurl;
            }
            set
            {
                this._p5_callbackurl = value;
            }
        }

        private string _p6_notifyurl = "/handler/payment/JinYang";
        [Description("同步通知")]
        public string p6_notifyurl
        {
            get
            {
                return this._p6_notifyurl;
            }
            set
            {
                this._p6_notifyurl = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        private const string p7_version = "v2.8";

        private const string p8_signtype = "1";

        private const string p11_isshow = "0";

        public override string ShowCallback()
        {
            return "ok";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("sysnumber");
            money = WebAgent.GetParam("paymoney", decimal.Zero);
            return WebAgent.GetParam("ordernumber");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("p1_mchtid", this.p1_mchtid);
            dic.Add("p2_paytype", string.IsNullOrEmpty(this.p2_paytype) ? this.BankValue : this.p2_paytype);
            dic.Add("p3_paymoney", this.Money.ToString("0.00"));
            dic.Add("p4_orderno", this.OrderID);
            dic.Add("p5_callbackurl", this.GetUrl(this.p5_callbackurl));
            dic.Add("p6_notifyurl", this.GetUrl(this.p6_notifyurl));
            dic.Add("p7_version", p7_version);
            dic.Add("p8_signtype", p8_signtype);
            dic.Add("p9_attach", this.Name);
            dic.Add("p10_appname", string.Empty);
            dic.Add("p11_isshow", p11_isshow);
            dic.Add("p12_orderip", IPAgent.IP);
            string signStr = dic.ToQueryString() + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());

            switch (this.p2_paytype)
            {
                case "":
                case "QQPAYWAP":
                case "ALIPAYWAP":
                case "WEIXINWAP":
                case "JDPAYWAP":
                    this.BuildForm(dic, this.Gateway);
                    break;
            }

            string data = dic.ToQueryString();
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);

            int rspCode = JsonAgent.GetValue<int>(result, "rspCode");
            if (rspCode != 1)
            {
                context.Response.Write(JsonAgent.GetValue<string>(result, "rspMsg") ?? result);
            }
            else
            {
                string code = JsonAgent.GetValue<string>(result, "data", "r6_qrcode");
                if (string.IsNullOrEmpty(code))
                {
                    context.Response.Write(result);
                    return;
                }

                switch (this.p2_paytype)
                {
                    case "WEIXIN":
                        this.CreateWXCode(code);
                        break;
                    case "ALIPAY":
                        this.CreateAliCode(code);
                        break;
                    case "QQPAY":
                        this.CreateQQCode(code);
                        break;
                    default:
                        this.CreateQRCode(code);
                        break;
                }
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string partner = WebAgent.GetParam("partner");
            string ordernumber = WebAgent.GetParam("ordernumber");
            string orderstatus = WebAgent.GetParam("orderstatus");
            string paymoney = WebAgent.GetParam("paymoney");
            string sign = WebAgent.GetParam("sign");
            if (orderstatus != "1") return false;
            string signStr = string.Format("partner={0}&ordernumber={1}&orderstatus={2}&paymoney={3}{4}", partner, ordernumber, orderstatus, paymoney, this.Key);
            if (MD5.toMD5(signStr).ToLower() != sign) return false;
            callback.Invoke();
            return true;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (!string.IsNullOrEmpty(this.p2_paytype)) return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.CMB, "CMB");
                dic.Add(BankType.BJBANK, "BCCB");
                dic.Add(BankType.COMM, "BOCO");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.NJCB, "NJCB");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.SPABANK, "PINGANBANK");
                dic.Add(BankType.BOHAIB, "CBHB");
                dic.Add(BankType.HKBEA, "HKBEA");
                dic.Add(BankType.NBBANK, "NBCB");
                dic.Add(BankType.CITIC, "CTTIC");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.SHBANK, "SHB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.PSBC, "PSBS");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.BJRCB, "BJRCB");
                dic.Add(BankType.SHRCB, "SRCB");
                return dic;
            }
        }
    }
}
