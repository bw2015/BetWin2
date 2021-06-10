using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Common.Sites;
using SP.Studio.Core;
using System.ComponentModel;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Json;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 泽圣支付的代付
    /// </summary>
    public class ZSAGE : IWithdraw
    {
        public ZSAGE() : base() { }

        public ZSAGE(string setting) : base(setting) { }


        private string _gateway = "http://spayment.zsagepay.com/payment/payment.do";
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


        private string _queryGate = "http://expand.clpayment.com/payment/queryState.do";
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

        [Description("商户号")]
        public string merchantCode { get; set; }

        [Description("密钥")]
        public string KEY { get; set; }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.COMM, "BCM");
                dic.Add(BankType.CMB, "CMB");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.BJBANK, "BCCB");
                dic.Add(BankType.PSBC, "PSBC");
                dic.Add(BankType.SHBANK, "BOS");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.CITIC, "CITIC");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.SPABANK, "PAB");
                return dic;
            }
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merchantCode", this.merchantCode);
            dic.Add("outOrderId", orderId);
            dic.Add("nonceStr", Guid.NewGuid().ToString("N"));
            string data = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&KEY=" + this.KEY;
            dic.Add("sign", MD5.toMD5(data));

            //{"code":"00","data":{"errorMsg":"操作完成","fee":200,"merchantCode":"1000002679","orderId":"2018053000050708735","outOrderId":"812737","sign":"1DF6EE1979E9C5A78C4E3BF63D9D2CA4","state":"00","totalAmount":10000,"transTime":"20180530110600"},"msg":"成功"}
            //{"code":"00","data":{"errorMsg":"根据银行code查询Unionbank失败","fee":200,"merchantCode":"1000002679","orderId":"2018052800050576746","outOrderId":"812614","sign":"A1009FAB0C27409B6D8B8C0CFA228DA7","state":"02","totalAmount":20000,"transTime":"20180528113101"},"msg":"成功"}
            string result = NetAgent.UploadData(this.QueryGate, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8);
            string code = JsonAgent.GetValue<string>(result, "code");

            if (code != "00")
            {
                msg = result;
                return WithdrawStatus.Error;
            }

            string state = JsonAgent.GetValue<string>(result, "data", "state");
            WithdrawStatus status = WithdrawStatus.Error;
            msg = result;
            switch (state)
            {
                case "00":
                    status = WithdrawStatus.Success;
                    msg = JsonAgent.GetValue<string>(result, "msg");
                    break;
                case "01":
                    status = WithdrawStatus.Paymenting;
                    msg = JsonAgent.GetValue<string>(result, "msg");
                    break;
                case "02":
                    status = WithdrawStatus.Return;
                    msg = JsonAgent.GetValue<string>(result, "data", "errorMsg");
                    break;
            }
            return status;
        }

        public override bool Remit(out string msg)
        {
            string bankCode = this.GetBankCode(this.BankCode);
            if (string.IsNullOrEmpty(bankCode))
            {
                msg = string.Format("系统不支持{0}", this.BankCode.GetDescription());
                return false;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merchantCode", this.merchantCode);
            dic.Add("nonceStr", Guid.NewGuid().ToString("N"));
            dic.Add("outOrderId", this.OrderID);
            dic.Add("totalAmount", ((int)(this.Money * 100)).ToString());
            dic.Add("intoCardNo", this.CardNo);
            dic.Add("intoCardName", this.Account);
            dic.Add("intoCardType", "2");
            dic.Add("bankCode", "");
            dic.Add("type", "04");
            dic.Add("bankName", "");
            dic.Add("remark", this.Account);

            string sign = string.Join("&",
                new string[] { "bankCode", "bankName", "intoCardName", "intoCardNo", "intoCardType", "merchantCode", "nonceStr", "outOrderId", "totalAmount", "type" }.OrderBy(t => t).Select(t => string.Format("{0}={1}", t, dic[t]))) +
                "&KEY=" + this.KEY;
            dic.Add("sign", MD5.toMD5(sign));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);

            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null)
            {
                msg = result;
                return false;
            }

            if (ht["code"].ToString() == "00")
            {
                msg = result;
                return true;
            }

            msg = ht.ContainsKey("msg") ? ht["msg"].ToString() : result;
            return false;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
        }
    }
}
