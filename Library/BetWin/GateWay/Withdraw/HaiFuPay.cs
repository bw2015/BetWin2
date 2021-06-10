using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BW.Common.Sites;
using SP.Studio.Core;
using SP.Studio.Array;
using SP.Studio.Security;
using System.Net;
using SP.Studio.Json;
using BW.Agent;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 海付盛通
    /// </summary>
    public class HaiFuPay : IWithdraw
    {
        public HaiFuPay() : base() { }

        public HaiFuPay(string setting) : base(setting) { }

        private string _gateway = "http://haifu.cloudlock.cc:8082/paying/withdraw/submit";
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

        private string _querygateway = "http://haifu.cloudlock.cc:8082/paying/withdraw/getOrderState";
        [Description("查询")]
        public string QueryGateway
        {
            get
            {
                return this._querygateway;
            }
            set
            {
                this._querygateway = value;
            }
        }

        [Description("操作员")]
        public string op_user_id { get; set; }

        [Description("密钥")]
        public string appSecret { get; set; }

        [Description("回调URL")]
        public string notify_url { get; set; }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ICBC, "中国工商银行");
                dic.Add(BankType.CMB, "中国招商银行");
                dic.Add(BankType.ABC, "中国农业银行");
                dic.Add(BankType.CCB, "中国建设银行");
                dic.Add(BankType.COMM, "中国交通银行");
                dic.Add(BankType.CIB, "兴业银行");
                dic.Add(BankType.CMBC, "民生银行");
                dic.Add(BankType.BOC, "中国银行");
                dic.Add(BankType.SPABANK, "平安银行");
                dic.Add(BankType.CITIC, "中信银行");
                dic.Add(BankType.GDB, "广东发展银行");
                dic.Add(BankType.PSBC, "中国邮政储蓄银行");
                dic.Add(BankType.CEB, "中国光大银行");
                return dic;
            }
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("op_user_id", this.op_user_id);
            dic.Add("nonce_str", Guid.NewGuid().ToString("N").Substring(0, 8).ToLower());
            dic.Add("spbill_create_ip", "192.168.1.165");
            dic.Add("product_id", orderId);
            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.appSecret;
            dic.Add("sign", MD5.toSHA1(signStr));

            string data = dic.ToJson();
            string result;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                result = Encoding.UTF8.GetString(wc.UploadData(this.QueryGateway, "POST", Encoding.UTF8.GetBytes(data)));
            }
            //{"info": "获取成功", "notifyUrl": "http://www.xlai.co/handler/payment/SUCCESS", "tradeNum": "a846a18807ae525c7fba22e1c2a3752a", "errcode": 200, "sign": "7FC85E9D727CA6E4D971CAA51168C54A08B1F3DD", "state": 1, "totalFee": 300, "productId": "215317"}
            string code = JsonAgent.GetValue<string>(result, "errcode");
            if (code != "200")
            {
                msg = result; return WithdrawStatus.Error;
            }

            WithdrawStatus status = WithdrawStatus.Error;

            switch (JsonAgent.GetValue<int>(result, "state"))
            {
                case 0:
                    status = WithdrawStatus.Paymenting;
                    break;
                case 1:
                    status = WithdrawStatus.Success;
                    break;
                case 2:
                    status = WithdrawStatus.Return;
                    break;
            }
            msg = JsonAgent.GetValue<string>(result, "info");
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("op_user_id", this.op_user_id);
            dic.Add("nonce_str", Guid.NewGuid().ToString("N").Substring(0, 8).ToLower());
            dic.Add("spbill_create_ip", "192.168.1.165");
            dic.Add("total_fee", ((int)this.Money * 100).ToString());
            dic.Add("card_no", this.CardNo);
            dic.Add("acct_name", this.Account);
            dic.Add("describe", string.Format("给{0}转钱", this.Account));
            dic.Add("notify_url", this.notify_url);
            dic.Add("bank_name", this.BankCode.GetDescription());
            dic.Add("pay_type", "0");
            dic.Add("product_id", this.OrderID);

            string signStr = dic.OrderBy(t => t.Key).ToQueryString() + this.appSecret;
            dic.Add("sign", MD5.toSHA1(signStr));

            string data = dic.ToJson();
            string result;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                result = Encoding.UTF8.GetString(wc.UploadData(this.Gateway, "POST", Encoding.UTF8.GetBytes(data)));
            }
            //{"info": "\u6210\u529f\u63d0\u4ea4\u8ba2\u5355", "tradeNum": "a846a18807ae525c7fba22e1c2a3752a", "errcode": 200, "sign": "0F67EC8DDAC6F04D873B6E16B1F05B6C5961071A", "time": "2018-04-16 22:36:15.762010", "totalFee": 100, "productId": "215317"}

            msg = JsonAgent.GetValue<string>(result, "info");
            string tradeNum = JsonAgent.GetValue<string>(result, "tradeNum");
            if (!string.IsNullOrEmpty(tradeNum)) UserAgent.Instance().UpdateWithdrawOrderSystemID(int.Parse(this.OrderID), tradeNum);
            string code = JsonAgent.GetValue<string>(result, "errcode");
            return code == "200";
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
