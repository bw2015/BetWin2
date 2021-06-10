using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Security;
using System.ComponentModel;
using BW.Common.Sites;

using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Json;
using SP.Studio.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 喜付
    /// </summary>
    public class XIFPay : IPayment
    {
        public XIFPay() : base() { }

        public XIFPay(string setting) : base(setting) { }

        /// <summary>
        /// 支付类型，固定值为1
        /// </summary>
        private readonly string paymentType = "1";


        /// <summary>
        /// 固定值online_pay，表示网上支付
        /// </summary>
        private readonly string service = "online_pay";

        private string _gateway = "https://ebank.xifpay.com/payment/v1/order/";
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

        private string _isApp = "web";
        /// <summary>
        /// 当该值传“app”时，表示app接入，返回二维码地址，需商户自行生成二维码；值为“web”时，表示web接入，直接在收银台页面上显示二维码；值为“H5”时，表示手机端html5接入，会在手机端唤醒支付app
        /// </summary>
        [Description("接入方式")]
        public string isApp
        {
            get
            {
                return this._isApp;
            }
            set
            {
                this._isApp = value;
            }
        }

        [Description("商户ID")]
        public string merchantId { get; set; }

        private string _notifyUrl = "/handler/payment/XIFPay";
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

        private string _returnUrl = "/handler/payment/XIFPay";
        [Description("跳转URL")]
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

        /// <summary>
        /// 支付方式，directPay：直连模式；bankPay：收银台模式
        /// </summary>
        private string _paymethod = "directPay";
        [Description("支付方式")]
        public string paymethod
        {
            get
            {
                return this._paymethod;
            }
            set
            {
                this._paymethod = value;
            }
        }

        /// <summary>
        /// 网银代码 为空使用网银
        /// 微信支付	WXPAY   支付宝支付 ALIPAY    QQ扫码 QQPAY  京东扫码 JDPAY  快捷支付 QUICKPAY   中国银联 UNIONPAY
        /// 百度钱包 BDPAY	银联扫码 UNIONQRPAY
        /// </summary>
        [Description("网银代码")]
        public string defaultbank { get; set; }

        [Description("密钥")]
        public string KEY { get; set; }


        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("body", "XIFPay");
            dic.Add("buyerEmail", this.Name + "@gmail.com");
            dic.Add("charset", "UTF-8");
            if (this.paymethod == "bankPay")
            {
                dic.Add("defaultbank", "");
            }
            else
            {
                dic.Add("defaultbank", string.IsNullOrEmpty(defaultbank) ? this.BankValue : this.defaultbank);
            }
            dic.Add("isApp", this.isApp);
            dic.Add("merchantId", this.merchantId);
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            dic.Add("orderNo", this.OrderID);
            dic.Add("paymentType", this.paymentType);
            dic.Add("paymethod", this.paymethod);
            dic.Add("returnUrl", this.GetUrl(this.returnUrl));
            dic.Add("riskItem", "");
            dic.Add("service", this.service);
            dic.Add("title", this.Name);
            dic.Add("totalFee", this.Money.ToString("0.00"));
            dic.Add("signType", "SHA");
            //signType sign_type sign
            string signStr = string.Join("&", dic.Where(t => !new string[] { "signType", "sign_type", "sign" }.Contains(t.Key) && !string.IsNullOrEmpty(t.Value))
                .OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.KEY;
            dic.Add("sign", this.Sign(signStr));
            string gateway = string.Format("{0}{1}-{2}", this.Gateway, this.merchantId, this.OrderID);

            switch (this.isApp)
            {
                case "app":
                    string result = NetAgent.UploadData(gateway, dic.ToQueryString(), Encoding.UTF8);
                    string code = JsonAgent.GetValue<string>(result, "codeUrl");
                    if (string.IsNullOrEmpty(code))
                    {
                        this.context.Response.Write(result);
                        return;
                    }
                    switch (this.defaultbank)
                    {
                        case "WXPAY":
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
                    break;
                case "web":
                    this.BuildForm(dic, gateway);
                    break;
            }
        }

        private string Sign(string signStr)
        {
            var sha = System.Security.Cryptography.SHA1.Create();
            var hashed = sha.ComputeHash(Encoding.UTF8.GetBytes(signStr));
            return BitConverter.ToString(hashed).Replace("-", "");
        }
        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("total_fee", decimal.Zero);
            systemId = WebAgent.GetParam("trade_no");
            return WebAgent.GetParam("order_no");
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override bool Verify(VerifyCallBack callback)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in this.context.Request.Form.AllKeys)
            {
                dic.Add(key, WebAgent.QF(key));
            }
            /*
            gmt_create:2018-03-23 14:29:04
            order_no:20180323142913008
            seller_email:110550@qq.com
            sign:74EEBD5BD12C2B1111E64124DD0D20DFFFFA12F7
            discount:0.00
            body:XIFPay
            is_success:T
            title:ceshi011
            notify_id:cba754e203cb4a1183834daceadfcdf6
            notify_type:WAIT_TRIGGER
            ext_param2:JDPAY
            price:10.00
            total_fee:10.00
            trade_status:TRADE_FINISHED
            sign_type:SHA
            seller_id:100000000002552
            is_total_fee_adjust:0
            buyer_email:ceshi011@gmail.com
            gmt_payment:2018-03-23 14:29:50
            notify_time:2018-03-23 14:29:50
            quantity:1
            gmt_logistics_modify:2018-03-23 14:29:50
            payment_type:1
            trade_no:101803238545305
            seller_actions:SEND_GOODS

            gmt_create : 2018-03-25 16:39:00      order_no : 20180325163900036      
            gmt_payment : 2018-03-25 16:40:35      seller_email : 110550@qq.com      
            notify_time : 2018-03-25 16:40:35      quantity : 1      
            sign : 0DF72F19F301851729DCF5BAD04B1B5B414C4B42      discount : 0.00      
            body : XIFPay      is_success : T      title : ceshixianlu      
            gmt_logistics_modify : 2018-03-25 16:39:00      notify_id : 6b7253f761734979ba6897472004c89a      
            notify_type : WAIT_TRIGGER      payment_type : 1      
            ext_param2 : BANKPAY      price : 12000.00      
            total_fee : 12000.00      trade_status : TRADE_FINISHED      
            trade_no : 101803258648053      signType : SHA      
            seller_actions : SEND_GOODS      seller_id : 100000000002552      
            is_total_fee_adjust : 0

            */
            if (dic.Get("is_success", "F") != "T" || dic.Get("trade_status", "") != "TRADE_FINISHED") return false;

            string sign = WebAgent.QF("sign");

            string signStr = dic.Where(t => !new string[] { "sign", "sign_type", "signType" }.Contains(t.Key)).OrderBy(t => t.Key).ToQueryString() + this.KEY;
            if (this.Sign(signStr).Equals(sign, StringComparison.CurrentCultureIgnoreCase))
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
                if (this.paymethod == "directPay" && string.IsNullOrEmpty(this.defaultbank))
                {
                    Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                    dic.Add(BankType.CMB, "CMB");
                    dic.Add(BankType.ICBC, "ICBC");
                    dic.Add(BankType.CCB, "CCB");
                    dic.Add(BankType.BOC, "BOC");
                    dic.Add(BankType.ABC, "ABC");
                    dic.Add(BankType.COMM, "BOCM");
                    dic.Add(BankType.SPDB, "SPDB");
                    dic.Add(BankType.GDB, "CGB");
                    dic.Add(BankType.CITIC, "CITIC");
                    dic.Add(BankType.CEB, "CEB");
                    dic.Add(BankType.CIB, "CIB");
                    dic.Add(BankType.SPABANK, "PAYH");
                    dic.Add(BankType.CMBC, "CMBC");
                    dic.Add(BankType.HXBANK, "HXB");
                    dic.Add(BankType.PSBC, "PSBC");
                    dic.Add(BankType.BJBANK, "BCCB");
                    dic.Add(BankType.SHBANK, "SHBANK");
                    return dic;
                }
                return null;
            }
        }
    }
}
