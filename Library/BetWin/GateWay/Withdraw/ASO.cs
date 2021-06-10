using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Sites;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Text;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Withdraw
{
    public class ASO : IWithdraw
    {
        public ASO() : base() { }

        public ASO(string setting) : base(setting) { }

        private const string service = "TRADE.SETTLE";

        /// <summary>
        /// 查询服务
        /// </summary>
        private const string service_query = "TRADE.SETTLE.QUERY";

        private const string version = "1.0.0.0";

        [Description("商户账号")]
        public string merId { get; set; }

        private string _notifyUrl = "/handler/payment/SUCCESS";
        [Description("结果通知")]
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

        private string _gateway = "http://gate.aospay.cn/cooperate/gateway.cgi";
        [Description("网关地址")]
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

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> _bankCode = new Dictionary<BankType, string>();
                _bankCode.Add(BankType.ABC, "ABC");
                _bankCode.Add(BankType.BOC, "BOC");
                _bankCode.Add(BankType.BOHAIB, "CBHB");
                _bankCode.Add(BankType.CCB, "CCB");
                _bankCode.Add(BankType.CEB, "CEB");
                _bankCode.Add(BankType.CIB, "CIB");
                _bankCode.Add(BankType.CMB, "CMB");
                _bankCode.Add(BankType.CMBC, "CMBC");
                _bankCode.Add(BankType.CITIC, "CNCB");
                _bankCode.Add(BankType.COMM, "COMM");
                _bankCode.Add(BankType.GDB, "GDB");
                _bankCode.Add(BankType.HXBANK, "HXB");
                _bankCode.Add(BankType.ICBC, "ICBC");
                _bankCode.Add(BankType.SPABANK, "PAB");
                _bankCode.Add(BankType.PSBC, "PSBC");
                _bankCode.Add(BankType.SPDB, "SPDB");
                return _bankCode;
            }
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("service", service);
            dic.Add("version", version);
            dic.Add("merId", merId);
            dic.Add("tradeNo", this.OrderID);
            dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("amount", this.Money.ToString("0.00"));
            dic.Add("notifyUrl", this.notifyUrl);
            dic.Add("extra", this.GetType().Name);
            dic.Add("summary", this.GetType().Name);
            dic.Add("bankCardNo", this.CardNo);
            dic.Add("bankCardName", this.Account);
            dic.Add("bankId", this.GetBankCode(this.BankCode));
            dic.Add("bankName", this.BankCode.GetDescription());
            dic.Add("purpose", WebAgent.GetRandom(1, 10).ToString());
            string sign = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
            dic.Add("sign", MD5.Encryp(sign).ToUpper());
            string result = NetAgent.UploadData(this.Gateway, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8);

            string code = StringAgent.GetString(result, "<code>", "</code>");
            msg = StringAgent.GetString(result, "<desc>", "</desc>");

            if (code == "00")
            {
                return true;
            }
            if (string.IsNullOrEmpty(msg)) msg = result;
            return false;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            DateTime createAt = UserAgent.Instance().GetWithdrawOrderDate(orderId);
            if (createAt == DateTime.MinValue)
            {
                msg = "找不到订单";
                return WithdrawStatus.Error;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("service", service_query);
            dic.Add("version", version);
            dic.Add("merId", merId);
            dic.Add("tradeNo", orderId);
            dic.Add("tradeDate", createAt.ToString("yyyyMMdd"));
            string sign = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
            dic.Add("sign", MD5.Encryp(sign).ToUpper());
            string result = NetAgent.UploadData(this.Gateway, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8);

            string status = StringAgent.GetString(result, "<status>", "</status>");
            msg = StringAgent.GetString(result, "<msgError>", "</msgError>");
            WithdrawStatus withdraw = WithdrawStatus.Error;
            switch (status)
            {
                case "0":
                case "5":
                    withdraw = WithdrawStatus.Paymenting;
                    break;
                case "1":
                    withdraw = WithdrawStatus.Success;
                    break;
                case "2":
                    withdraw = WithdrawStatus.Return;
                    break;
                default:
                    if (string.IsNullOrEmpty(msg)) msg = result;
                    break;
            }
            return withdraw;

        }
    }
}
