using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;

using BW.Common.Sites;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Xml;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 摩宝的下发接口
    /// </summary>
    public class MOPay : IWithdraw
    {
        public MOPay() : base() { }

        public MOPay(string setting) : base(setting) { }

        private const string apiName = "SINGLE_ENTRUST_SETT";

        private const string apiVersion = "1.0.0.0";

        [Description("网关")]
        public string Gateway { get; set; }

        [Description("平台ID")]
        public string platformID { get; set; }

        [Description("商户账号")]
        public string merchNo { get; set; }

        private string _merchUrl = "/handler/payment/SUCCESS";
        [Description("通知地址")]
        public string merchUrl
        {
            get
            {
                return this._merchUrl;
            }
            set
            {
                this._merchUrl = value;
            }
        }

        [Description("密钥")]
        public string KEY { get; set; }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("apiName", "SINGLE_SETT_QUERY");
            dic.Add("apiVersion", "1.0.0.0");
            dic.Add("platformID", this.platformID);
            dic.Add("merchNo", this.merchNo);
            dic.Add("orderNo", orderId);
            dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
            string signStr = string.Join("&", "apiName,apiVersion,platformID,merchNo,orderNo,tradeDate".Split(',').Select(t => string.Format("{0}={1}", t, dic.Get(t, string.Empty))));
            dic.Add("signMsg", this.Sign(signStr));
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            msg = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            try
            {
                XElement root = XElement.Parse(msg);
                string code = root.GetValue("respData[0]/respCode[0]");
                msg = root.GetValue("respData[0]/respDesc[0]", msg);
                WithdrawStatus withdrawStatus = WithdrawStatus.Error;
                if (code != "00") return withdrawStatus;
                int status = root.GetValue("respData[0]/Status[0]", -1);
                switch (status)
                {
                    case 0:
                        withdrawStatus = WithdrawStatus.Paymenting;
                        break;
                    case 1:
                        withdrawStatus = WithdrawStatus.Success;
                        break;
                    case 2:
                        withdrawStatus = WithdrawStatus.Return;
                        break;
                }
                return withdrawStatus;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return WithdrawStatus.Error;
            }
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("apiName", apiName);
            dic.Add("apiVersion", apiVersion);
            dic.Add("platformID", this.platformID);
            dic.Add("merchNo", this.merchNo);
            dic.Add("orderNo", this.OrderID);
            dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
            dic.Add("merchUrl", this.merchUrl);
            dic.Add("merchParam", DateTime.Now.Ticks.ToString());
            dic.Add("bankAccNo", this.CardNo);
            dic.Add("bankAccName", this.Account);
            dic.Add("bankCode", this.GetBankCode(this.BankCode));
            dic.Add("bankName", this.BankCode.GetDescription());
            dic.Add("province", "广东省");
            dic.Add("city", "深圳市");
            dic.Add("Amt", this.Money.ToString("0.00"));
            dic.Add("tradeSummary", this.Account);

            string signStr = string.Join("&", "apiName,apiVersion,platformID,merchNo,orderNo,tradeDate,merchUrl,merchParam,bankAccNo,bankAccName,bankCode,bankName,province,city,Amt,tradeSummary".Split(',')
                .Select(t => string.Format("{0}={1}", t, dic.Get(t, string.Empty))));
            dic.Add("signMsg", this.Sign(signStr));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            msg = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            try
            {
                XElement root = XElement.Parse(msg);
                string code = root.GetValue("respData[0]/respCode[0]");
                msg = root.GetValue("respData[0]/respDesc[0]", msg);
                return code == "00";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }


        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> _code = new Dictionary<BankType, string>();
                _code.Add(BankType.ICBC, "ICBC");
                _code.Add(BankType.ABC, "ABC");
                _code.Add(BankType.BOC, "BOC");
                _code.Add(BankType.CCB, "CCB");
                _code.Add(BankType.COMM, "COMM");
                _code.Add(BankType.CMB, "CMB");
                _code.Add(BankType.SPDB, "SPDB");
                _code.Add(BankType.CIB, "CIB");
                _code.Add(BankType.CMBC, "CMBC");
                _code.Add(BankType.GDB, "GDB");
                _code.Add(BankType.CITIC, "CNCB");
                _code.Add(BankType.CEB, "CEB");
                _code.Add(BankType.HXBANK, "HXB");
                _code.Add(BankType.PSBC, "PSBC");
                _code.Add(BankType.SPABANK, "PAB");
                return _code;
            }
        }

        private string Sign(string srcString)
        {
            string result = srcString + this.KEY;
            return SP.Studio.Security.MD5.Encryp(result).ToUpper();
        }
    }
}
