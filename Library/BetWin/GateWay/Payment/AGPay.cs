using BW.Common.Sites;
using Newtonsoft.Json.Linq;
using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 阿甘支付
    /// </summary>
    public class AGPay : IPayment
    {
        public AGPay() : base() { }

        public AGPay(string setting) : base(setting) { }

        [Description("商户编号")]
        public string pay_memberid { get; set; }

        [Description("支付类型 911:快捷支付 907:网银 908:QQ 905:QQ H5 904:支付宝H5 903:支付宝扫码 902:微信扫码")]
        public string pay_service { get; set; }

        private string _pay_notifyurl = "/handler/payment/AGPay";
        [Description("通知地址")]
        public string pay_notifyurl
        {
            get
            {
                return this._pay_notifyurl;
            }
            set
            {
                this._pay_notifyurl = value;
            }
        }

        private string _pay_callbackurl = "/handler/payment/AGPay";
        [Description("通知地址")]
        public string pay_callbackurl
        {
            get
            {
                return this._pay_callbackurl;
            }
            set
            {
                this._pay_callbackurl = value;
            }
        }

        private string _gateway = "https://www.agpay88.com/Pay_Index.html";
        [Description("网关")]
        public string GateWay
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


        [Description("密钥")]
        public string Key { get; set; }

        public override string ShowCallback()
        {
            return "ok";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("transaction_id");
            money = WebAgent.GetParam("amount", decimal.Zero);
            return WebAgent.GetParam("orderid");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //            $native = array(
            //    "pay_memberid" => $pay_memberid,
            //    "pay_orderid" => $pay_orderid,
            //    "pay_amount" => $pay_amount,
            //    "pay_applydate" => $pay_applydate,
            //    "pay_service" => $pay_service,
            //    //"pay_bankcode" => '10002',
            //    "pay_bankcode" => $pay_witchbank,
            //    "pay_notifyurl" => $pay_notifyurl,
            //    "pay_callbackurl" => $pay_callbackurl,
            //);
            dic.Add("pay_memberid", this.pay_memberid);
            dic.Add("pay_orderid", this.OrderID);
            dic.Add("pay_amount", this.Money.ToString("0.00"));
            dic.Add("pay_applydate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("pay_bankcode", this.pay_service);
            //dic.Add("pay_bankcode", string.IsNullOrEmpty(this.BankValue) ? "10001" : this.BankValue);
            dic.Add("pay_notifyurl", this.GetUrl(this.pay_notifyurl));
            dic.Add("pay_callbackurl", this.GetUrl(this.pay_callbackurl));
            string signStr = dic.Sort().ToQueryString() + "&key=" + this.Key;
            dic.Add("pay_md5sign", MD5.toMD5(signStr));
            this.BuildForm(dic, this.GateWay);
            
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("memberid", WebAgent.GetParam("memberid"));
            dic.Add("orderid", WebAgent.GetParam("orderid"));
            dic.Add("amount", WebAgent.GetParam("amount"));
            dic.Add("returncode", WebAgent.GetParam("returncode"));
            dic.Add("datetime", WebAgent.GetParam("datetime"));
            dic.Add("transaction_id", WebAgent.GetParam("transaction_id"));

            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + this.Key;

            string sign = WebAgent.GetParam("sign");
            if (MD5.toMD5(signStr).Equals(sign, StringComparison.CurrentCultureIgnoreCase))
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
                if (this.pay_service != "907") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "10001");
                dic.Add(BankType.ABC, "10002");
                dic.Add(BankType.BOC, "10003");
                dic.Add(BankType.CCB, "10004");
                dic.Add(BankType.COMM, "10005");
                dic.Add(BankType.CMB, "10006");
                dic.Add(BankType.GDB, "10007");
                dic.Add(BankType.CITIC, "10008");
                dic.Add(BankType.CMBC, "10009");
                dic.Add(BankType.CEB, "10010");
                dic.Add(BankType.SPABANK, "10011");
                dic.Add(BankType.SPDB, "10012");
                dic.Add(BankType.PSBC, "10013");
                dic.Add(BankType.HXBANK, "10014");
                dic.Add(BankType.CIB, "10015");
                dic.Add(BankType.BJBANK, "10016");
                return dic;
            }
        }
    }
}
