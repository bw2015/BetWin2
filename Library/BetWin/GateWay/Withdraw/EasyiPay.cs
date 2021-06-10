using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using BW.Common.Sites;
using SP.Studio.Core;
using SP.Studio.Array;
using SP.Studio.Security;
using System.ComponentModel;
using SP.Studio.Net;
using BW.Agent;
using SP.Studio.Json;

namespace BW.GateWay.Withdraw
{
    public class EasyiPay : IWithdraw
    {
        public EasyiPay() : base() { }

        public EasyiPay(string setting) : base(setting) { }

        private string _gateway = "https://transfer.easyipay.com/interface/transfer/index.aspx";
        [Description("代付网关")]
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

        public string _query = "https://transfer.easyipay.com/interface/transfer/query.aspx";
        [Description("查询网关")]
        public string QueryGateway
        {
            get
            {
                return this._query;
            }
            set
            {
                this._query = value;
            }
        }

        [Description("商户ID")]
        public string parter { get; set; }

        [Description("密钥")]
        public string Key { get; set; }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "中国工商银行");
                dic.Add(BankType.ABC, "中国农业银行");
                dic.Add(BankType.CCB, "中国建设银行");
                dic.Add(BankType.COMM, "中国交通银行");
                dic.Add(BankType.BOC, "中国银行");
                dic.Add(BankType.CMB, "招商银行");
                dic.Add(BankType.PSBC, "中国邮政储蓄银行");
                dic.Add(BankType.CMBC, "中国民生银行");
                dic.Add(BankType.HXBANK, "华夏银行");
                dic.Add(BankType.CIB, "兴业银行");
                dic.Add(BankType.GDB, "广发银行");
                dic.Add(BankType.SPDB, "浦发银行");
                dic.Add(BankType.CEB, "光大银行");
                dic.Add(BankType.CITIC, "中信银行");
                dic.Add(BankType.SPABANK, "平安银行");
                return dic;
            }
        }
        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("settleid", UserAgent.Instance().GetWithdrawOrderSystemID(int.Parse(orderId)));
            dic.Add("parter", this.parter);
            string signStr = dic.ToQueryString() + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());
            string result = NetAgent.UploadData(this.QueryGateway, dic.ToQueryString(), Encoding.UTF8);

            //{"parter":"666595","settleid":"","status":"fail","settlestatus":"null","amount":"null","msg":"缺少参数","sign":"456292df31a579d8a279615d3b2bd4f5"}
            msg = JsonAgent.GetValue<string>(result, "msg");
            if (JsonAgent.GetValue<string>(result, "status") != "success") return WithdrawStatus.Error;

            WithdrawStatus status = WithdrawStatus.Error;

            msg = JsonAgent.GetValue<string>(result, "settlestatus");
            switch (msg)
            {
                case "审核中":
                case "支付中":
                    status = WithdrawStatus.Paymenting;
                    break;
                case "已拒绝":
                case "已拒绝(已退款)":
                case "代付失败":
                case "代付失败(未冲正请咨询客服)":
                    status = WithdrawStatus.Return;
                    break;
                case "已支付":
                case "平台已支付(未到账请咨询客服)":
                    status = WithdrawStatus.Success;
                    break;
                default:
                    msg += " " + JsonAgent.GetValue<string>(result, "msg");
                    break;
            }
            if (string.IsNullOrEmpty(msg)) msg = result;
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("parter", this.parter);
            dic.Add("orderid", this.OrderID);
            dic.Add("value", ((int)this.Money).ToString());
            dic.Add("payeebank", this.GetBankCode(this.BankCode));
            dic.Add("account", this.CardNo);
            dic.Add("cardname", this.Account);
            string signStr = dic.ToQueryString() + this.Key;
            dic.Add("sign", MD5.toMD5(signStr).ToLower());

            string data = dic.ToQueryString();

            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            msg = result;

            if (result.StartsWith("error"))
            {
                msg = result.Substring("error&".Length);
                return false;
            }
            if (result.StartsWith("success"))
            {
                NameValueCollection coll = HttpUtility.ParseQueryString(result);
                string systemId = coll["settleid"];
                UserAgent.Instance().UpdateWithdrawOrderSystemID(int.Parse(OrderID), systemId);
                msg = coll["msg"];
                return true;
            }

            //success&settleid=结算序号&orderid=商户代付的订单号&msg=提现请求已经

            return false;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
