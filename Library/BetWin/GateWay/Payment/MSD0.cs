using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;

using SP.Studio.Json;
using SP.Studio.Security;
using SP.Studio.Net;

namespace BW.GateWay.Payment
{
    public class MSD0 : IPayment
    {
        public MSD0() : base() { }

        public MSD0(string setting) : base(setting) { }

        /// <summary>
        /// 密钥
        /// </summary>
        [Description("密钥")]
        public string Key { get; set; }

        [Description("商戶号")]
        public string customerId { get; set; }

        [Description("支付渠道")]
        public string channelFlag { get; set; }

        private string _notifyUrl = "/handler/payment/MSD0";
        [Description("通知url")]
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

        [Description("后台商户号")]
        public string userid { get; set; }

        private string _gateway = "http://extman.kefupay.cn/newWechats/newWeChatpayment_mobile.action";
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

        /// <summary>
        /// 命令（扫码支付）
        /// </summary>
        private const string msBank_ScanPay = "msBank_ScanPay";

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            throw new NotImplementedException();
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("customerId", this.customerId);
            dic.Add("channelFlag", this.channelFlag);
            dic.Add("amount", this.Money.ToString("0.00"));
            dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
            dic.Add("goodsName", this.Name);
            dic.Add("userid", this.userid);
            string signstr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
            dic.Add("sign", MD5.toMD5(signstr).ToLower());
            dic.Add("pay_number", this.OrderID);
            dic.Add("orderCode", msBank_ScanPay);

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = NetAgent.UploadData(this.GateWay, data, Encoding.UTF8);

            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("codeUrl"))
            {
                HttpContext.Current.Response.Write(result);
                return;
            }

            string code = ht["codeUrl"].ToString();

            switch (this.channelFlag)
            {
                case "00":
                    this.CreateWXCode(code);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }
    }
}
