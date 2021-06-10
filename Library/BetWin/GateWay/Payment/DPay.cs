using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SP.Studio.Array;
using SP.Studio.Security;
using BW.Common.Sites;
using SP.Studio.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class DPay : IPayment
    {
        public DPay() : base() { }

        public DPay(string setting) : base(setting) { }

        private string _gateway = "http://s3.av8dpay.com/bifubao-gateway/front-pay/ebank-pay.htm";
        [Description("网关")]
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

        [Description("商户号")]
        public string MERCHANT_ID { get; set; }

        private string _NO_URL = "/handler/payment/DPay";
        [Description("通知地址")]
        public string NO_URL
        {
            get
            {
                return this._NO_URL;
            }
            set
            {
                this._NO_URL = value;
            }
        }


        private string _RET_URL = "/handler/payment/DPay";
        [Description("返回地址")]
        public string RET_URL
        {
            get
            {
                return this._RET_URL;
            }
            set
            {
                this._RET_URL = value;
            }
        }

        [Description("密钥")]
        public string KEY { get; set; }

        public override string ShowCallback()
        {
            return "success";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            string result = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            JObject json = JObject.Parse(result);

            money = json.Value<decimal>("TRAN_AMT") / 100M;
            systemId = json.Value<string>("SYS_CODE");
            return json.Value<string>("TRAN_CODE");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("MERCHANT_ID", this.MERCHANT_ID);
            dic.Add("TRAN_CODE", this.OrderID);
            dic.Add("TRAN_AMT", ((int)(this.Money * 100)).ToString());
            dic.Add("NO_URL", this.GetUrl(this.NO_URL));
            dic.Add("RET_URL", this.GetUrl(this.RET_URL));
            dic.Add("SUBMIT_TIME", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("BANK_ID", this.BankValue);
            dic.Add("VERSION", "1");
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.KEY;
            dic.Add("SIGNED_MSG", MD5.toMD5(signStr).ToLower());

            this.BuildForm(dic, this.GateWay);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string result = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            JObject json = JObject.Parse(result);

            //{"MERCHANT_ID":"SP20180709143726","PAY_TIME":"20180711123430","REMARK":"","SIGNED_MSG":"53a6b58d575dff65bcf6e935de8f630e","STATUS":"1","SYS_CODE":"PA20180711123353AINON3445","TRAN_AMT":"11100","TRAN_CODE":"20180711123352192","TYPE":"4"} 

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("TYPE", json.Value<string>("TYPE"));
            dic.Add("MERCHANT_ID", json.Value<string>("MERCHANT_ID"));
            dic.Add("TRAN_CODE", json.Value<string>("TRAN_CODE"));
            dic.Add("SYS_CODE", json.Value<string>("SYS_CODE"));
            dic.Add("TRAN_AMT", json.Value<string>("TRAN_AMT"));
            dic.Add("REMARK", json.Value<string>("REMARK"));
            dic.Add("STATUS", json.Value<string>("STATUS"));
            dic.Add("PAY_TIME", json.Value<string>("PAY_TIME"));
            string sign = json.Value<string>("SIGNED_MSG");

            if (dic["STATUS"] != "1") return false;

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.KEY;
            if (MD5.toMD5(signStr).Equals(sign, StringComparison.CurrentCultureIgnoreCase))
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
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.CCB, "1004");
                dic.Add(BankType.ABC, "1002");
                dic.Add(BankType.ICBC, "1001");
                dic.Add(BankType.BOC, "1003");
                dic.Add(BankType.SPDB, "1014");
                dic.Add(BankType.CEB, "1008");
                dic.Add(BankType.SPABANK, "1011");
                dic.Add(BankType.CIB, "1013");
                dic.Add(BankType.PSBC, "1006");
                dic.Add(BankType.CITIC, "1007");
                dic.Add(BankType.HXBANK, "1009");
                dic.Add(BankType.CMB, "1012");
                dic.Add(BankType.GDB, "1017");
                dic.Add(BankType.BJBANK, "1016");
                dic.Add(BankType.SHBANK, "1025");
                dic.Add(BankType.CMBC, "1010");
                dic.Add(BankType.COMM, "1005");
                dic.Add(BankType.BJRCB, "1103");
                return dic;
            }
        }
    }
}
