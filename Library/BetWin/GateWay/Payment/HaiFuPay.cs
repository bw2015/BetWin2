using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net;

using SP.Studio.Web;
using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Json;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class HaiFuPay : IPayment
    {
        public HaiFuPay() : base() { }

        public HaiFuPay(string setting) : base(setting) { }

        private string _gateway = "http://haifu.cloudlock.cc:9091/paying/lovepay/getQr";
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

        [Description("操作用户")]
        public string op_user_id { get; set; }

        private string _notify_url = "/handler/payment/HaiFuPay";
        [Description("后端回调")]
        public string notify_url
        {
            get
            {
                return this._notify_url;
            }
            set
            {
                this._notify_url = value;
            }
        }

        private string _front_notify_url = "/handler/payment/HaiFuPay";
        [Description("前端回调")]
        public string front_notify_url
        {
            get
            {
                return this._front_notify_url;
            }
            set
            {
                this._front_notify_url = value;
            }
        }

        [Description("0,微信扫码 1,微信公众号 2,微信wap 3,支付宝wap 4,支付宝扫码 5,网银 6,快捷支付")]
        public string pay_type { get; set; }

        [Description("密钥")]
        public string appSecret { get; set; }


        public override string GetTradeNo(out decimal money, out string systemId)
        {
            //{"info": "pay success", "fee": 100, 
            //"notifyUrl": "http://ceshi.xlai.co/handler/payment/HaiFuPay", 
            //"tradeNum": "29c16571df3888339381cfddf626ea67", "errcode": 200, 
            //"sign": "FE1981DE570A65653E7BF3F906E20FCCB5DB2934", "state": 1, 
            //"time": "2018-03-15 15:59:43.666851", "opUserId": "e23cf8aa29308ccca8ea6dd275598e11", "productId": "20180315155905464"}

            string result = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            systemId = JsonAgent.GetValue<string>(result, "tradeNum");
            money = JsonAgent.GetValue<decimal>(result, "fee") / 100M;
            return JsonAgent.GetValue<string>(result, "productId");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("body", this.Name);
            dic.Add("total_fee", ((int)(this.Money * 100)).ToString());
            dic.Add("product_id", this.OrderID);
            dic.Add("goods_tag", this.Name);
            dic.Add("op_user_id", this.op_user_id);
            dic.Add("nonce_str", Guid.NewGuid().ToString("N").Substring(0, 16).ToLower());
            dic.Add("spbill_create_ip", IPAgent.IP);
            dic.Add("notify_url", this.GetUrl(notify_url));
            dic.Add("front_notify_url", this.GetUrl(front_notify_url));
            dic.Add("pay_type", this.pay_type);
            if (this.pay_type == "5")
            {
                dic.Add("bank_id", this.BankValue);
            }
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.appSecret;
            dic.Add("sign", MD5.toSHA1(signStr));
            string data = dic.ToJson();
            string result;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                result = Encoding.UTF8.GetString(wc.UploadData(this.Gateway, "POST", Encoding.UTF8.GetBytes(data)));
            }

            int errcode = JsonAgent.GetValue<int>(result, "errcode");
            if (errcode != 200)
            {
                context.Response.Write(JsonAgent.GetValue<string>(result, "info"));
                return;
            }

            string url = JsonAgent.GetValue<string>(result, "code_url");

            this.BuildForm(url);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string result = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            int code = JsonAgent.GetValue<int>(result, "errcode");
            if (code != 200) return false;

            int state = JsonAgent.GetValue<int>(result, "state");
            if (state != 1) return false;

            IDictionary<string, string> dic = JsonAgent.GetDictionary<string, string>(result);

            string sign = dic["sign"];
            dic.Remove("sign");

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.appSecret;
            if (MD5.toSHA1(signStr) != sign) return false;

            callback.Invoke();
            return true;
        }

        public override string ShowCallback()
        {
            string result = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("errcode", "200");
            dic.Add("info", "SUCCESS");
            dic.Add("tradeNum", JsonAgent.GetValue<string>(result, "tradeNum"));
            dic.Add("notifyUrl", this.GetUrl(this.notify_url));
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.appSecret;
            dic.Add("sign", MD5.toSHA1(signStr));

            return dic.ToJson();
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.pay_type != "5") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "10001");
                dic.Add(BankType.CMB, "10002");
                dic.Add(BankType.ABC, "10003");
                dic.Add(BankType.CCB, "10004");
                dic.Add(BankType.COMM, "10005");
                dic.Add(BankType.CIB, "10006");
                dic.Add(BankType.CMBC, "10007");
                dic.Add(BankType.BOC, "10008");
                dic.Add(BankType.SPABANK, "10009");
                dic.Add(BankType.CITIC, "10010");
                dic.Add(BankType.GDB, "10011");
                dic.Add(BankType.PSBC, "10012");
                dic.Add(BankType.CEB, "10013");
                return dic;
            }
        }
    }
}
