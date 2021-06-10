using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;
using SP.Studio.Security;
using BW.Common.Sites;

namespace BW.GateWay.Payment
{
    public class ACPay : IPayment
    {
        public ACPay() : base() { }

        public ACPay(string setting) : base(setting) { }

        [Description("商户ID")]
        public string customer { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        [Description("支付类型")]
        public string banktype { get; set; }

        private string _asynbackurl = "/handler/payment/ACPay";
        [Description("异步通知")]
        public string asynbackurl
        {
            get
            {
                return this._asynbackurl;
            }
            set
            {
                this._asynbackurl = value;
            }
        }

        private string _synbackurl = "/handler/payment/ACPay";
        [Description("同步通知")]
        public string synbackurl
        {
            get
            {
                return this._synbackurl;
            }
            set
            {
                this._synbackurl = value;
            }
        }

        private string _gateway = "https://gateway.acpay365.com/GateWay/Index";
        [Description("支付网关")]
        public string GATEWAY
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
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("customer", this.customer);
            if (string.IsNullOrEmpty(this.banktype))
            {
                dic.Add("banktype", this.BankValue);
            }
            else
            {
                dic.Add("banktype", this.banktype);
            }
            dic.Add("amount", this.Money.ToString("0.00"));
            dic.Add("orderid", this.OrderID);
            dic.Add("asynbackurl", this.GetUrl(this.asynbackurl));
            dic.Add("request_time", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("synbackurl", this.GetUrl(this.synbackurl));
            dic.Add("israndom", "Y");

            string signStr = string.Format("customer={0}&banktype={1}&amount={2}&orderid={3}&asynbackurl={4}&request_time={5}&key={6}",
                dic["customer"], dic["banktype"], dic["amount"], dic["orderid"], dic["asynbackurl"], dic["request_time"], this.Key);
            dic.Add("sign", this.MD5(signStr));

            this.BuildForm(dic, this.GATEWAY);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }

        private string MD5(string strToEncrypt)
        {
            var bytes = Encoding.UTF8.GetBytes(strToEncrypt);
            bytes = new MD5CryptoServiceProvider().ComputeHash(bytes);
            var encryptStr = "";
            for (var i = 0; i < bytes.Length; i++)
                encryptStr = encryptStr + bytes[i].ToString("x").PadLeft(2, '0');
            return encryptStr.ToLower();
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (string.IsNullOrEmpty(this.banktype))
                {
                    Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                    dic.Add(BankType.CITIC, "962");
                    dic.Add(BankType.BOC, "963");
                    dic.Add(BankType.ABC, "964");
                    dic.Add(BankType.CCB, "965");
                    dic.Add(BankType.ICBC, "967");
                    dic.Add(BankType.CMB, "970");
                    dic.Add(BankType.PSBC, "971");
                    dic.Add(BankType.CIB, "972");
                    dic.Add(BankType.SHRCB, "976");
                    dic.Add(BankType.SPDB, "977");
                    dic.Add(BankType.NJCB, "979");
                    dic.Add(BankType.CMBC, "980");
                    dic.Add(BankType.COMM, "981");
                    dic.Add(BankType.HZCB, "983");
                    dic.Add(BankType.GDB, "985");
                    dic.Add(BankType.CEB, "986");
                    dic.Add(BankType.HKBEA, "987");
                    dic.Add(BankType.BJBANK, "989");
                    return dic;
                }

                return null;
            }
        }
    }
}
