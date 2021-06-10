using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Security;

namespace BW.GateWay.Payment
{
    public class Pay18 : IPayment
    {
        public Pay18() : base() { }

        public Pay18(string setting) : base(setting) { }

        [Description("商户ID")]
        public string pay_memberid { get; set; }

        private string _pay_notifyurl = "/handler/payment/Pay18";
        [Description("通知地址")]
        public string pay_notifyurl
        {
            get
            {
                return this._pay_notifyurl;
            }
            set
            {
                this._pay_notifyurl = value;
            }
        }


        private string _pay_callbackurl = "/handler/payment/Pay18";
        [Description("回调地址")]
        public string pay_callbackurl
        {
            get
            {
                return this._pay_callbackurl;
            }
            set
            {
                this._pay_callbackurl = value;
            }
        }

        [Description("密钥")]
        public string pay_key { get; set; }

        [Description("银行编码")]
        public string pay_bankcode { get; set; }

        private string _gateway = "http://www.pay18.cn/Pay_Index.html";
        [Description("网关地址")]
        public string GateWay
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

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            throw new NotImplementedException();
        }

        public override void GoGateway()
        {
            //stringSignTemp="pay_amount=pay_amount&pay_applydate=pay_applydate&pay_bankcode=pay_bankcode&pay_callbackurl=pay_callbackurl&pay_memberid=pay_memberid&pay_notifyurl=pay_notifyurl&pay_orderid=pay_orderid&key=key"  sign=MD5(stringSignTemp).toUpperCase() 
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add("pay_memberid", this.pay_memberid);
            dic.Add("pay_orderid", this.OrderID);
            dic.Add("pay_applydate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dic.Add("pay_bankcode", this.pay_bankcode);
            dic.Add("pay_notifyurl", this.GetUrl(this.pay_notifyurl));
            dic.Add("pay_callbackurl", this.GetUrl(this.pay_callbackurl));
            dic.Add("pay_amount", this.Money.ToString("0.00"));
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&key=" + this.pay_key;
            dic.Add("sign", MD5.toMD5(signStr));
            this.BuildForm(dic, this.GateWay);

        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }
    }
}
