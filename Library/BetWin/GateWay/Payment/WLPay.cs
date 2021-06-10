using BW.Common.Sites;
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
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class WLPay : IPayment
    {
        public WLPay()
        {
        }

        public WLPay(string settingString) : base(settingString)
        {
        }

        [Description("网关")]
        public string Gateway { get; set; } = "http://paygate.chongshengwei.cn:9090/powerpay-gateway-onl/txn";


        [Description("安全密钥")]
        public string Key { get; set; }

        [Description("商户号")]
        public string merId { get; set; }

        [Description("页面通知")]
        public string pageReturnUrl { get; set; } = "/handler/payment/WLPay";

        [Description("后台通知")]
        public string notifyUrl { get; set; } = "/handler/payment/WLPay";

        /// <summary>
        /// 31=微信支付 32=支付宝  33=QQ支付 34=云闪付  35=京东扫码 21 = 网银
        /// </summary>
        [Description("支付类型")]
        public string txnSubType { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("txnId");
            money = WebAgent.GetParam("txnAmt", decimal.Zero) / 100M;
            return WebAgent.GetParam("orderId");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"txnType","01" },
                {"txnSubType",this.txnSubType },
                {"secpVer","icp3-1.1" },
                {"secpMode","perm" },
                {"macKeyId",this.merId },
                {"orderDate",DateTime.Now.ToString("yyyyMMdd") },
                {"orderTime",DateTime.Now.ToString("HHmmss") },
                {"merId",this.merId },
                {"orderId",this.OrderID },
                {"pageReturnUrl",this.GetUrl(this.pageReturnUrl) },
                {"notifyUrl",this.GetUrl(this.notifyUrl) },
                {"productTitle","PAY" },
                {"productDesc","PAYMENT" },
                {"txnAmt", ((int)this.Money * 100M).ToString() },
                {"currencyCode","156" },
                {"timeStamp",DateTime.Now.ToString("yyyyMMddHHmmss") }
            };

            switch (this.txnSubType)
            {
                case "21":
                    if (!string.IsNullOrEmpty(this.BankValue)) dic.Add("bankNum", this.BankValue);
                    break;
                case "41":
                case "42":
                case "43":
                case "44":
                case "45":
                    dic.Add("clientIp", IPAgent.IP);
                    dic.Add("sceneBizType", "WAP");
                    dic.Add("wapUrl", "https://www.baidu.com");
                    dic.Add("wapName", "PAYMENT");
                    break;
            }

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&k=" + this.hex();
            dic.Add("mac", MD5.toMD5(signStr).ToLower());
            if (this.txnSubType == "21" || this.txnSubType == "22" || this.txnSubType == "23")
            {
                this.BuildForm(dic, this.Gateway);
                return;
            }


            //{ "orderId" : "20190104033725426", "codeImgUrl" : "", "secpVer" : "icp3-1.1", "mac" : "3bbb6ca118b09526e2f7563eb859bf0c", "extInfo" : "", "timeStamp" : "20190104033725", "txnStatusDesc" : "交易失败", "orderTime" : "033725", "txnStatus" : "20", "macKeyId" : "999350900000435", "respMsg" : "无法获取该商户的交易参数 (商户号：999350900000435，交易类型：0132)", "secpMode" : "perm", "merId" : "999350900000435", "currencyCode" : "156", "orderDate" : "20190104", "respCode" : "3000", "txnAmt" : "10000", "txnId" : "201901040000001869052874" }
            //{ "orderId" : "20190104034003752", "codeImgUrl" : "https://qr.95516.com/00010000/01851342851990517041320557910034", "secpVer" : "icp3-1.1", "mac" : "dccd77e3996d3cb204005243a377f4ef", "extInfo" : "", "timeStamp" : "20190104034005", "txnStatusDesc" : "支付請求成功", "orderTime" : "034003", "txnStatus" : "01", "macKeyId" : "999350900000435", "respMsg" : "成功", "secpMode" : "perm", "merId" : "999350900000435", "currencyCode" : "156", "orderDate" : "20190104", "respCode" : "0000", "txnAmt" : "10000", "txnId" : "201901040000001869069514" }
            //{
            //"orderId" : "201810110000000000228280",
            //"secpVer" : "icp3-1.1",
            //"codePageUrl" : "http://codepageurl.com",
            //"mac" : "c276a8ac657a9614fdcdc053f0270609",
            //"extInfo" : "test",
            //"txnStatusDesc" : "交易成功",
            //"orderTime" : "223310",
            //"txnStatus" : "10",
            //"macKeyId" : "999110000000070",
            //"timeStamp" : "20181011223310",
            //"secpMode" : "perm",
            //"respMsg" : "成功",
            //"merId" : "999110000000070",
            //"orderDate" : "20181011",
            //"currencyCode" : "156",
            //"txnAmt" : "10000",
            //"respCode" : "0000",
            //"txnId" : "201806250000000007174279"
            //}
            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);
            try
            {
                JObject info = (JObject)JsonConvert.DeserializeObject(result);

                string code = null;
                if (info.ContainsKey("codeImgUrl")) code = info["codeImgUrl"].Value<string>();
                if (info.ContainsKey("codePageUrl")) code = info["codePageUrl"].Value<string>();

                if (string.IsNullOrEmpty(code))
                {
                    this.context.Response.Write(info["respMsg"].Value<string>());
                    return;
                }

                //31=微信支付 32=支付宝  33=QQ支付 34=云闪付  35=京东扫码
                switch (this.txnSubType)
                {
                    case "31":
                    case "41":
                        this.CreateWXCode(code);
                        break;
                    case "32":
                    case "42":
                        this.CreateAliCode(code);
                        break;
                    case "33":
                    case "43":
                        this.CreateQQCode(code);
                        break;
                    default:
                        this.CreateQRCode(code);
                        break;
                }
            }
            catch
            {
                this.context.Response.Write(result);
            }
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in this.context.Request.Form.AllKeys)
            {
                dic.Add(key, this.context.Request[key]);
            }
            if (dic.Get("respCode", string.Empty) == "0000" && dic.Get("txnStatus", string.Empty) == "10")
            {
                string sign = dic.Get("mac", string.Empty);
                dic.Remove("mac");
                string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&k=" + this.hex();
                if (MD5.toMD5(signStr).Equals(sign, StringComparison.CurrentCultureIgnoreCase))
                {
                    callback.Invoke();
                    return true;
                }
            }

            return false;
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                return null;
                //if (this.txnSubType != "21") return null;

                //return new Dictionary<BankType, string>()
                //{
                //    { BankType.ICBC,"01020000" },
                //    { BankType.ABC,"01030000" },
                //    { BankType.BOC,"01040000" },
                //    { BankType.CCB,"01050000" },
                //    { BankType.COMM,"03010000" },
                //    { BankType.CITIC,"03020000" },
                //    { BankType.CEB,"03030000" },
                //    { BankType.HXBANK,"03040000" },
                //    { BankType.CMBC,"03050000" },
                //    { BankType.GDB,"03060000" },
                //    { BankType.SPABANK,"03070000" },
                //    { BankType.CMB,"03080000" },
                //    { BankType.CIB,"03090000" },
                //    { BankType.SPDB,"03100000" },
                //    { BankType.EGBANK,"03110000" },
                //    { BankType.SHBANK,"03130000" },
                //    { BankType.BJBANK,"03131000" },
                //    { BankType.PSBC,"04030000" },
                //    { BankType.GCB,"04135810" }
                //};
            }
        }

        private string hex()
        {
            return this.Key;
        }
    }
}
