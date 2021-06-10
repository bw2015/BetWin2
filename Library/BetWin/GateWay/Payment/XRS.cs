using Newtonsoft.Json;
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

namespace BW.GateWay.Payment
{
    public class XRS : IPayment
    {
        public XRS()
        {
        }

        public XRS(string settingString) : base(settingString)
        {
        }

        [Description("网关")]
        public string Gateway { get; set; } = "http://47.75.30.61/v1/pay";

        [Description("商户号")]
        public string code { get; set; }

        [Description("密钥")]
        public string KEY { get; set; }

        [Description("异步通知")]
        public string notifyUrl { get; set; } = "/handler/payment/XRS";

        /// <summary>
        /// wxpay / wxpay2 / wxpaywap
        /// </summary>
        [Description("支付类型")]
        public string payCode { get; set; }

        public override decimal[] GetMoneyValue()
        {
            return new decimal[]
            {
                10.00M,
                20.00M,
                30.00M,
                50.00M,
                100.00M,
                200.00M,
                300.00M,
                500.00M
            };
        }


        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            string input = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            JObject json = (JObject)JsonConvert.DeserializeObject(input);
            money = json["tradeAmount"].Value<decimal>();
            systemId = json["outOrderNo"].Value<string>();
            return systemId;
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            dic.Add("outOrderNo", this.OrderID);
            dic.Add("goodsClauses", "PAYMENT");
            dic.Add("tradeAmount", this.Money.ToString("0.00"));
            dic.Add("code", this.code);
            dic.Add("payCode", this.payCode);
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.KEY;
            dic.Add("sign", MD5.toMD5(signStr));

            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);
            try
            {
                JObject json = (JObject)JsonConvert.DeserializeObject(result);
                if (json["resultStatus"] != null) return;
                string payState = json["payState"].Value<string>();
                if (payState != "success")
                {
                    context.Response.Write(result);
                    return;
                }
                string url = json["url"].Value<string>();
                //wxpay / wxpay2 / wxpaywap
                switch (json["payCode"].Value<string>())
                {
                    case "alipay":
                        this.CreateAliCode(WebAgent.GetQRCode(url));
                        break;
                    case "wxpay":
                    case "wxpay2":
                    case "wxpaywap":
                        this.CreateWXCode(WebAgent.GetQRCode(url));
                        break;
                    default:
                        context.Response.Write(json["payCode"].Value<string>());
                        break;
                }
            }
            catch (Exception ex)
            {
                this.context.Response.Write(ex.Message);
                this.context.Response.Write(result);
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string input = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            JObject json = (JObject)JsonConvert.DeserializeObject(input);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("outOrderNo", json["outOrderNo"].Value<string>());
            dic.Add("goodsClauses", json["goodsClauses"].Value<string>());
            dic.Add("tradeAmount", json["tradeAmount"].Value<string>());
            dic.Add("shopCode ", json["shopCode "].Value<string>());
            dic.Add("code", json["code"].Value<string>());
            dic.Add("nonStr", json["nonStr"].Value<string>());
            dic.Add("msg", json["msg"].Value<string>());
            if (dic["msg"] != "SUCCESS" || dic["code"] != "0") return false;

            string sign = json["sign"].Value<string>();
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&key=" + this.KEY;
            if (sign.Equals(MD5.toMD5(signStr), StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }
    }
}
