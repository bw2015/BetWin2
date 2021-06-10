using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Security;
using SP.Studio.Web;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// DD支付宝付款
    /// </summary>
    public class hhnnm : IPayment
    {
        public hhnnm() : base() { }

        public hhnnm(string setting) : base(setting) { }

        [Description("商户号")]
        public string userId { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        [Description("支付类型")]
        public string type { get; set; }

        private string _returnUrl = "/handler/payment/hhnnm";
        [Description("同步通知")]
        public string returnUrl
        {
            get
            {
                return this._returnUrl;
            }
            set
            {
                this._returnUrl = value;
            }
        }


        private string _notifyUrl = "/handler/payment/hhnnm";
        [Description("同步通知")]
        public string notifyUrl
        {
            get
            {
                return this._notifyUrl;
            }
            set
            {
                this._notifyUrl = value;
            }
        }

        private string _gateway = "http://www.hhnnm.com/api/ddalipay/services/initorder";
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

        public override string ShowCallback()
        {
            return "success";
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("userId", this.userId);
            dic.Add("sdorderNo", this.OrderID);
            dic.Add("totalFee", ((int)(this.Money * 100)).ToString());
            dic.Add("returnUrl", this.GetUrl(returnUrl));
            dic.Add("notifyUrl", this.GetUrl(notifyUrl));
            switch (this.type)
            {
                case "232":
                case "242":
                    dic.Add("gatType", "EasyPay");
                    dic.Add("goodsId", this.type);
                    break;
                default:
                    dic.Add("type", this.type);
                    break;
            }

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string status = WebAgent.GetParam("status");
            if (status != "1") return false;
            Dictionary<string, string> dic = new Dictionary<string, string>();

            //customerid=11071&status=1&sdpayno=2018052811543825340&sdorderno=20180528115432021&total_fee=900.00&paytype=EasyPay&sign=049a541dd5eff7dd588b41be3129d90d
            //customerid : 11071      status : 1      sdpayno : 2018052811543825340      sdorderno : 20180528115432021      total_fee : 900.00      paytype : EasyPay      sign : 049a541dd5eff7dd588b41be3129d90d
            foreach (string key in new string[] { "status", "customerid", "sdpayno", "sdorderno", "total_fee", "paytype" })
            {
                string value = WebAgent.GetParam(key);
                if (!string.IsNullOrEmpty(value)) dic.Add(key, WebAgent.GetParam(key));
            }
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.Key;
            string sign = WebAgent.GetParam("sign");
            if (MD5.toMD5(signStr).ToLower() == sign)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("sdpayno");
            money = WebAgent.GetParam("total_fee", decimal.Zero);
            return WebAgent.GetParam("sdorderno");
        }
    }
}
