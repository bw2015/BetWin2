using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Common.Sites;
using SP.Studio.Array;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Json;

using BW.Agent;

using System.ComponentModel;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 金贝支付
    /// </summary>
    public class JBPay : IWithdraw
    {
        public JBPay() : base() { }

        public JBPay(string setting) : base(setting) { }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.BOC, "1000");
                dic.Add(BankType.ABC, "1001");
                dic.Add(BankType.ICBC, "1002");
                dic.Add(BankType.CCB, "1003");
                dic.Add(BankType.CMB, "1004");
                dic.Add(BankType.CMBC, "1005");
                dic.Add(BankType.CIB, "1006");
                dic.Add(BankType.COMM, "1007");
                dic.Add(BankType.CEB, "1008");
                dic.Add(BankType.SPDB, "1009");
                dic.Add(BankType.SPABANK, "1010");
                dic.Add(BankType.GDB, "1011");
                dic.Add(BankType.CITIC, "1012");
                dic.Add(BankType.BJBANK, "1013");
                dic.Add(BankType.PSBC, "1014");
                dic.Add(BankType.SHBANK, "1015");
                dic.Add(BankType.HXBANK, "1016");
                dic.Add(BankType.BOHAIB, "1017");
                dic.Add(BankType.NBBANK, "1018");
                dic.Add(BankType.HZCB, "1019");
                return dic;
            }
        }

        /// <summary>
        /// 0：对私；1：对公
        /// </summary>
        private const string dtype = "0";

        [Description("商户ID")]
        public string amchid { get; set; }

        private string _gateway = "https://${amchid}.jbpay.org/apay/";
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

        private string _queryGate = "https://api.huayuepay.com/aquery/";
        [Description("查询网关")]
        public string QueryGate
        {
            get
            {
                return this._queryGate;
            }
            set
            {
                this._queryGate = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("amchid", this.amchid);
            dic.Add("border", orderId);
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.Key;
            dic.Add("sign", MD5.Encryp(signStr, "GBK"));
            string url = this.QueryGate.Replace("${amchid}", this.amchid) + "?" + dic.ToQueryString();
            
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            //JSON：{"order":"df000000000000000000","money":"100.01","state":2,"time":"2017/09/07 08:08:56","err":""}
            string state = JsonAgent.GetValue<string>(result, "state");
            WithdrawStatus status = WithdrawStatus.Error;
            msg = JsonAgent.GetValue<string>(result, "err");
            switch (state)
            {
                case "0":
                    status = WithdrawStatus.Paymenting;
                    break;
                case "1":
                    status = WithdrawStatus.Return;
                    break;
                case "2":
                    status = WithdrawStatus.Success;
                    break;
            }
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("amchid", this.amchid);
            dic.Add("border", this.OrderID);
            dic.Add("cmoney", this.Money.ToString("0.00"));
            dic.Add("dtype", dtype);
            dic.Add("ebacc", this.Account);
            dic.Add("fbaccount", this.CardNo);
            dic.Add("gbcode", this.GetBankCode(BankCode));
            dic.Add("hbranch", "碧海湾社区支行");
            dic.Add("iprovince", "11");
            dic.Add("jcity", "1000");
            dic.Add("kiphone", "13888888888");

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.Key;
            dic.Add("sign", MD5.Encryp(signStr, "GBK"));

            string url = this.Gateway.Replace("${amchid}", this.amchid) + "?" + dic.ToQueryString();
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            //{"order":"df000000000000000000","state":1,"err":""}
            int state = JsonAgent.GetValue<int>(result, "state");
            msg = JsonAgent.GetValue<string>(result, "err");
            return state == 1;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
