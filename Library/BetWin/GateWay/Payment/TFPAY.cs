using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Array;
using SP.Studio.Web;
using SP.Studio.Json;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class TFPAY : IPayment
    {
        public TFPAY() : base() { }

        public TFPAY(string setting) : base(setting) { }

        /// <summary>
        /// 时间字符串
        /// </summary>
        private string dateTime
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        /// <summary>
        /// 签名方式
        /// </summary>
        private string signType = "MD5";

        /// <summary>
        /// 网银-支付GWUP001 同步POST
        /// 网银-结果查询GWUP002 同步POST
        /// 网银-结果异步通知GWUP003 异步通知POST
        /// 银联扫码BMPC001 异步通知POST
        /// QQ 扫码BMPC002 异步通知POST
        /// 京东扫码BMPC003 异步通知POST
        /// 微信扫码支付BMPC004 异步通知POST
        /// 支付宝扫码支付BMPC005 异步通知POST
        /// 支付结果查询BMPC006 同步POST
        /// 支付结果异步通知BMPC007 同步POST
        /// </summary>
        [Description("接口编码")]
        public string command { get; set; }

        [Description("合作方编号")]
        public string groupCode { get; set; }

        [Description("商户编号")]
        public string merchantCode { get; set; }

        [Description("终端编号")]
        public string terminalCode { get; set; }

        private string _notifyUrl = "/handler/payment/TFPAY";
        [Description("异步通知")]
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

        private string _returnUrl = "/handler/payment/TFPAY";
        [Description("跳转页面")]
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

        private string _cardType = "0";
        [Description("卡类型 0:借记卡 1:贷记卡")]
        public string cardType
        {
            get
            {
                return this._cardType;
            }
            set
            {
                this._cardType = value;
            }
        }

        private string _gateway;
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
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string KEY { get; set; }


        [Description("门店编号")]
        public string shopCode { get; set; }


        [Description("机具终端编号")]
        public string shopTerminateCode { get; set; }

        [Description("收款商户名称")]
        public string payeeName { get; set; }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetPage("platOrderNum");
            money = WebAgent.GetParam("payMoney", decimal.Zero) / 100M;
            return WebAgent.GetParam("orderNum");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("command", this.command);
            dic.Add("groupCode", this.groupCode);
            dic.Add("signType", this.signType);
            dic.Add("dateTime", this.dateTime);
            dic.Add("merchantCode", this.merchantCode);
            dic.Add("terminalCode", this.terminalCode);
            dic.Add("orderNum", this.OrderID);
            dic.Add("payMoney", ((int)this.Money * 100).ToString());
            dic.Add("productName", "PAYMENT");
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            switch (this.command)
            {
                case "GWUP001":
                    dic.Add("returnUrl", this.GetUrl(this.returnUrl));
                    dic.Add("currency", "CNY");
                    dic.Add("cardType", this.cardType);
                    dic.Add("bankLink", this.BankValue);
                    break;
                case "BMPC005": // 支付宝扫码
                case "BMPC004": // 微信扫码
                case "BMPC001": // 银联扫码
                case "BMPC002": // QQ扫码
                case "BMPC003": // 京东扫码
                    dic.Add("shopCode", this.shopCode);
                    dic.Add("shopTerminateCode", this.shopTerminateCode);
                    dic.Add("payeeName", this.payeeName);
                    break;
            }

            string[] keys = dic.Keys.ToArray();
            Array.Sort(keys, string.CompareOrdinal);
            string signStr = string.Join(string.Empty, keys.Select(t => dic[t])) + this.KEY;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());

            switch (this.command)
            {
                case "GWUP001":
                    this.BuildForm(dic, this.Gateway);
                    break;
                case "BMPC005": // 支付宝扫码
                case "BMPC004": // 微信扫码
                case "BMPC001": // 银联扫码
                case "BMPC002": // QQ扫码
                case "BMPC003": // 京东扫码
                    string data = dic.ToQueryString();
                    string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
                    string payUrl = JsonAgent.GetValue<string>(result, "payUrl");
                    if (string.IsNullOrEmpty(payUrl))
                    {
                        context.Response.Write(JsonAgent.GetValue<string>(result, "platRespMessage"));
                    }
                    else
                    {
                        switch (this.command)
                        {
                            case "BMPC005":
                                this.CreateAliCode(payUrl);
                                break;
                            case "BMPC004":
                                this.CreateWXCode(payUrl);
                                break;
                            case "BMPC002":
                                this.CreateQQCode(payUrl);
                                break;
                            default:
                                this.CreateQRCode(payUrl, dic);
                                break;
                        }
                    }
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in this.context.Request.Form.AllKeys)
            {
                dic.Add(key, this.context.Request.Form[key]);
            }
            string platPayResultCode = dic["platPayResultCode"];
            if (platPayResultCode != "PTN0004") return false;
            string signStr = string.Join(string.Empty, dic.Where(t => t.Key != "sign").OrderBy(t => t.Key).Select(t => t.Value)) + this.KEY;
            if (MD5.toMD5(signStr).Equals(dic["sign"], StringComparison.CurrentCultureIgnoreCase))
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
                if (this.command != "GWUP001") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "01020000");
                dic.Add(BankType.ABC, "01030000");
                dic.Add(BankType.BOC, "01040000");
                dic.Add(BankType.CCB, "01050000");
                dic.Add(BankType.COMM, "03010000");
                dic.Add(BankType.CMB, "03080000");
                dic.Add(BankType.GDB, "03060000");
                dic.Add(BankType.CITIC, "03020000");
                dic.Add(BankType.CMBC, "03050000");
                dic.Add(BankType.CEB, "03030000");
                dic.Add(BankType.SPABANK, "03070000");
                dic.Add(BankType.SPDB, "03100000");
                dic.Add(BankType.PSBC, "01000000");
                dic.Add(BankType.HXBANK, "03040000");
                dic.Add(BankType.CIB, "03090000");
                return dic;
            }
        }
    }
}
