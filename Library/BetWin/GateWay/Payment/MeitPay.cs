using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using SP.Studio.Web;
using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Json;

namespace BW.GateWay.Payment
{
    public class MeitPay : IPayment
    {
        public MeitPay() : base() { }

        public MeitPay(string setting) : base(setting) { }

        private string _gateway = "http://api.meitpay.com/payment/create";
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

        /// <summary>
        /// 商户号
        /// </summary>
        [Description("商户号")]
        public string app_id { get; set; }

        /// <summary>
        /// 支付宝H5：1        微信H5：2   QQ钱包：3  银联wap：4 支付宝扫码：5 微信扫码：6  QQ扫码：7
        /// </summary>
        [Description("支付方式")]
        public string pay_type { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string appKey { get; set; }

        public override string ShowCallback()
        {
            return "success";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            //code : 200      ts : 1540454910      rand : 427121      app_id : 200067      order_id : 20181025155503256      platform_order_id : mf1540454070970021967      user_id : 3006414e2b2744b9      pay_type : 1      pay_id : 152      price : 300000      money : 300000      status : 2      pay_order_id :       pay_ts : 1540454145      sign : afe3230119ff5211b0a56819ae28d0d3

            money = WebAgent.GetParam("money", decimal.Zero) / 100M;
            systemId = WebAgent.GetParam("platform_order_id");
            return WebAgent.GetParam("order_id");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("app_id", this.app_id);
            dic.Add("order_id", this.OrderID);
            dic.Add("remark", "PAYMENT");
            dic.Add("user_id", Guid.NewGuid().ToString("N").Substring(0, 16));
            dic.Add("client_ip", IPAgent.IP);
            dic.Add("price", ((int)this.Money * 100).ToString());
            dic.Add("pay_type", this.pay_type);
            dic.Add("extend", "PAYMENT");
            dic.Add("ts", WebAgent.GetTimeStamps().ToString());
            dic.Add("rand", Guid.NewGuid().ToString("N").Substring(0, 6));

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.appKey;

            dic.Add("sign", SP.Studio.Security.MD5.toMD5(signStr).ToLower());

            //{"code":200,"msg":"SUCCESS","data":{"h5_pay_url":"http:\/\/api.meitpay.com\/pay_qr_do.php?merId=100660073&body=pay&callbackUrl=http%3A%2F%2Fapi.meitpay.com%2Freturn%2Fqieboxpay&notifyUrl=http%3A%2F%2Fapi.meitpay.com%2Fcallback%2Fqieboxpay&outTradeNo=mf1538747297170726746&totalFee=10000&nonceStr=mokpu5m7pt3shlnuir61e7o9&payChannel=ALIH5&sign=D57F88F89C5DE6FFF1FFEC1FACBBB8AA&api_url=http%3A%2F%2Ftrade.dragonhe.cn%2Fpay%2Fgateway&is_post=1&a=10000","platfrom_order_id":"mf1538747297170726746","order_id":"20181005214841241"}}

            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);
            if (JsonAgent.GetValue<int>(result, "code") != 200)
            {
                context.Response.Write(JsonAgent.GetValue<string>(result, "msg"));
                return;
            }

            string url = JsonAgent.GetValue<string>(result, "data", "h5_pay_url");
            if (!string.IsNullOrEmpty(url))
            {
                this.BuildForm(url);
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("code", 0) != 200) return false;
            if (WebAgent.GetParam("status", 0) != 2) return false;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in this.context.Request.Form.AllKeys)
            {
                if (key == "sign") continue;
                dic.Add(key, this.context.Request.Form[key]);
            }
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.appKey;
            string sign = WebAgent.GetParam("sign");
            if (SP.Studio.Security.MD5.toMD5(signStr).ToLower() == sign)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }
    }
}
