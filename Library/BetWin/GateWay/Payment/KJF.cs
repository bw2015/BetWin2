using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.Payment
{
    public class KJF : IPayment
    {

        public KJF() : base()
        {
        }

        public KJF(string settingString) : base(settingString)
        {
        }

        [Description("网关")]
        public string Gateway { get; set; } = "http://kjfpay.seepay.net/serviceDirect.html";

        [Description("商户号")]
        public string merchantId { get; set; }

        [Description("通知URL")]
        public string notifyUrl { get; set; } = "/handler/payment/KJF";

        [Description("返回URL")]
        public string returnUrl { get; set; } = "/handler/payment/KJF";

        [Description("密钥")]
        public string Key { get; set; }

        [Description("支付方式")]
        public string payMethod { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("TransactionId");
            money = WebAgent.GetParam("FaceValue", decimal.Zero);
            return WebAgent.GetParam("OrderId");
        }

        public override void GoGateway()
        {
            // 构造要请求的参数数组
            //$parameter = [
            //    "service" => $service,
            //    "inputCharset" => $inputCharset,
            //    "merchantId" => $merchantId,
            //    "payMethod" => $payMethod,
            //    "outOrderId" => $outOrderId,
            //    "subject" => $subject,
            //    "body" => $body,
            //    "transAmt" => $transAmt,
            //    "notifyUrl" => $notifyUrl,
            //    "returnUrl" => $returnUrl,
            //    "signType" => $signType,
            //    "defaultBank" => $defaultBank,
            //    "channel" => $channel,
            //    "cardAttr" => $cardAttr,
            //    "attach"   => $attach
            //];


            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"service","directPay" },
                {"inputCharset","UTF-8" },
                {"merchantId",this.merchantId },
                {"payMethod",this.payMethod },
                {"outOrderId",this.OrderID },
                {"subject","PAY" },
                {"body","PAYMENT" },
                {"transAmt",this.Money.ToString("0.00") },
                {"notifyUrl",this.GetUrl(this.notifyUrl) },
                {"returnUrl",this.GetUrl(this.returnUrl) },
                {"defaultBank","" },
                {"channel","B2C" },
                {"cardAttr","01" },
                {"attach","PAYMENT" }
            };

            string signStr = dic.Where(t => !string.IsNullOrEmpty(t.Value)).OrderBy(t => t.Key).ToQueryString() + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());
            dic.Add("signType", "MD5");
            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in this.context.Request.Form.AllKeys)
            {
                dic.Add(key, this.context.Request.Form[key]);
            }
            string sign = dic["Sign"];
            dic.Remove("Sign");
            string signStr = dic.Where(t => !string.IsNullOrEmpty(t.Value)).OrderBy(t => t.Key).ToQueryString() + this.Key;
            if (sign.Equals(MD5.toMD5(signStr).ToLower()))
            {
                callback.Invoke();
                return true;
            }
            return false;

        }

        public override string ShowCallback()
        {
            return "ok";
        }
    }
}
