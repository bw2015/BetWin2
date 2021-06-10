using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Security;
using SP.Studio.Web;
using SP.Studio.Array;
using SP.Studio.Net;
using System.ComponentModel;
using SP.Studio.Json;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class MMPay : IPayment
    {
        public MMPay() : base() { }

        public MMPay(string setting) : base(setting) { }

        private string _gateway = "http://gateway.imemepay.com/cnp/gateway";
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

        [Description("商户号")]
        public string trx_key { get; set; }

        [Description("密钥")]
        public string secret_key { get; set; }

        [Description("产品类型")]
        public string product_type { get; set; }

        private string _return_url = "/handler/payment/MMPay";
        [Description("页面通知")]
        public string return_url
        {
            get
            {
                return this._return_url;
            }
            set
            {
                this._return_url = value;
            }
        }


        private string _callback_url = "/handler/payment/MMPay";
        [Description("页面通知")]
        public string callback_url
        {
            get
            {
                return this._callback_url;
            }
            set
            {
                this._callback_url = value;
            }
        }

        private string request_time { get { return DateTime.Now.ToString("yyyyMMddHHmmss"); } }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("ord_amount", decimal.Zero);
            systemId = WebAgent.GetParam("pay_request_id");
            return WebAgent.GetParam("request_id");
        }

        public override void GoGateway()
        {
            //callback_url=http://localhost:8080/demo/callback/notify&
            //goods_name =abc&
            //ord_amount =2&product_type=70203&
            //request_id =1515935195465&request_ip=127.0.0.1&
            //request_time =20180114210635&return_url=http://www.baidu.com&
            //trx_key =4de8b7c8c9594ad9a6ca2753b4cfdd2a&secret_key=7b525d3e7bfb4fbdb7e67a41b84eb4a7

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("callback_url", this.GetUrl(this.callback_url));
            dic.Add("goods_name", this.Name);
            dic.Add("ord_amount", this.Money.ToString("0.00"));
            dic.Add("product_type", this.product_type);
            dic.Add("request_id", this.OrderID);
            dic.Add("request_ip", IPAgent.IP);
            dic.Add("request_time", this.request_time);
            dic.Add("return_url", this.GetUrl(this.return_url));
            dic.Add("trx_key", this.trx_key);
            if (this.product_type == "50103")
            {
                dic.Add("bank_code", this.BankValue);
                dic.Add("account_type", "PRIVATE_DEBIT_ACCOUNT");
            }

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&secret_key=" + this.secret_key;
            dic.Add("sign", MD5.toMD5(signStr));


            //{"sign":"D7CAB5B48F07B8B309E4273E72E99211","data":"https://qpay.qq.com/qr/6f340478","rsp_code":"0000","rsp_msg":""}
            string result = NetAgent.UploadData(this.Gateway, dic.ToQueryString(), Encoding.UTF8);
            string rsp_code = JsonAgent.GetValue<string>(result, "rsp_code");
            if (rsp_code != "0000")
            {
                string msg = JsonAgent.GetValue<string>(result, "rsp_msg");
                context.Response.Write(string.IsNullOrEmpty(msg) ? result : msg);
                return;
            }

            string data = JsonAgent.GetValue<string>(result, "data");
            switch (this.product_type)
            {
                case "50103":
                    this.BuildForm(data);
                    break;
                case "10103":
                case "10303":
                case "10203":
                    this.CreateWXCode(data);
                    break;
                case "20203":
                case "20303":
                    this.CreateAliCode(data);
                    break;
                case "70103":
                case "70203":
                    this.CreateQQCode(data);
                    break;
                default:
                    this.CreateQRCode(data);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            ///handler/payment/MMPay?
            ///goods_name=longhu6&ord_amount=200.00&pay_request_id=T77772018063010169965&product_type=50103
            ///&request_id=20180630131701911&request_time=20180630131701&trx_key=9ce0d8f4984a440e9f40df4694d292f7
            ///&trx_status=SUCCESS&trx_time=20180630131745&sign=055260E45E5B6B6728AFCE00FB87DF94&
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in "trx_key,ord_amount,request_id,trx_status,product_type,request_time,goods_name,trx_time,pay_request_id".Split(','))
            {
                dic.Add(key, WebAgent.GetParam(key));
            }
            string sign = WebAgent.GetParam("sign");
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + "&secret_key=" + this.secret_key;
            if (sign.Equals(MD5.toMD5(signStr), StringComparison.CurrentCultureIgnoreCase))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.product_type != "50103") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.PSBC, "POST");
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.GDB, "CGB");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.COMM, "BOCO");
                dic.Add(BankType.CMB, "CMBCHINA");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.SPABANK, "PAB");
                dic.Add(BankType.BJBANK, "BCCB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.SHBANK, "SHB");
                dic.Add(BankType.CITIC, "ECITIC");
                return dic;
            }
        }
    }
}
