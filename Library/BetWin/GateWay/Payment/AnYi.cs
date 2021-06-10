using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Json;
using SP.Studio.Web;

namespace BW.GateWay.Payment
{
    public class AnYi : IPayment
    {
        public AnYi() : base() { }

        public AnYi(string setting) : base(setting) { }

        private string _gateway = "http://117.25.133.19:9000/scan/getQrCode";
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

        [Description("商户编号")]
        public string platSource { get; set; }

        [Description("支付类型")]
        public string payType { get; set; }

        private string _notifyUrl = "/handler/payment/AnYi";
        [Description("回调地址")]
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

        [Description("密钥")]
        public string Key { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            //            platSource 是   商户编号 / 平台来源
            //orderNo 是   订单编号
            //payAmt  是 交易金额
            //success 是   是否支付成功
            //sign    是 MD5加密签名（大写）
            money = WebAgent.GetParam("payAmt", decimal.Zero);
            systemId = WebAgent.GetParam("orderNo");
            return systemId;
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("platSource", this.platSource);
            dic.Add("payType", this.payType);
            dic.Add("payAmt", this.Money.ToString("0.00"));
            dic.Add("orderNo", this.OrderID);
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            string signStr = string.Join("|", dic.OrderBy(t => t.Key).Select(t => t.Value)) + "|" + this.Key;
            dic.Add("sign", MD5.toMD5(signStr));

            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);

            bool success = JsonAgent.GetValue<bool>(result, "success");
            if (!success)
            {
                context.Response.Write(JsonAgent.GetValue<string>(result, "message"));
                return;
            }
            string code = JsonAgent.GetValue<string>(result, "info", "qrCode");
            if (string.IsNullOrEmpty(code))
            {
                context.Response.Write(result);
                return;
            }

            code = code.ToLower();
            switch (this.payType)
            {
                case "alipay":
                    this.CreateAliCode(code);
                    break;
                case "wechat":
                    this.CreateWXCode(code);
                    break;
                default:
                    this.CreateQRCode(code);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("platSource", WebAgent.GetParam("platSource"));
            dic.Add("orderNo", WebAgent.GetParam("orderNo"));
            dic.Add("payAmt", WebAgent.GetParam("payAmt"));
            dic.Add("success", WebAgent.GetParam("success"));
            if (dic["success"] != "true") return false;

            string signStr = string.Join("|", dic.OrderBy(t => t.Key).Select(t => t.Value)) + "|" + this.Key;
            if (MD5.toMD5(signStr).ToLower() == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }
    }
}
