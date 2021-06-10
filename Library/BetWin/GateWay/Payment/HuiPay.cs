using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Array;
using BW.Common.Sites;
using SP.Studio.Net;
using SP.Studio.Json;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 新畅汇
    /// </summary>
    public class HuiPay : IPayment
    {
        public HuiPay() : base() { }

        public HuiPay(string setting) : base(setting) { }

        private const string p0_Cmd = "Buy";

        private const string p3_Cur = "CNY";


        private string _pi_Url = "/handler/payment/HuiPay";
        [Description("返回地址")]
        public string pi_Url
        {
            get
            {
                return this._pi_Url;
            }
            set
            {
                this._pi_Url = value;
            }
        }

        private string _p8_Url = "/handler/payment/HuiPay";
        [Description("通知路径")]
        public string p8_Url
        {
            get
            {
                return this._p8_Url;
            }
            set
            {
                this._p8_Url = value;
            }
        }


        /// <summary>
        /// WEIXIN 微信        OnlinePay ⽹银支付        Nocard_H5 快捷支付h5 QQ  QQ 钱包
        /// JDPAY 京东钱包  UnionPay 银联扫码   WEIXINWAP 微信WAP QQWAP QQWAP
        /// </summary>
        [Description("通道编码")]
        public string pa_FrpId { get; set; }

        [Description("商户编号")]
        public string p1_MerId { get; set; }

        private string _gateway = "https://gateway.senhuayu.com/controller.action";
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

        [Description("密钥")]
        public string merchantKey { get; set; }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("r2_TrxId");
            money = WebAgent.GetParam("r3_Amt", decimal.Zero);
            return WebAgent.GetParam("r6_Order");
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("p0_Cmd", p0_Cmd);
            dic.Add("p1_MerId", this.p1_MerId);
            dic.Add("p2_Order", this.OrderID);
            dic.Add("p3_Cur", p3_Cur);
            dic.Add("p4_Amt", this.Money.ToString("0.00"));
            dic.Add("p5_Pid", this.Name);
            dic.Add("p6_Pcat", "MONEY");
            dic.Add("p7_Pdesc", this.Description);
            dic.Add("p8_Url", this.GetUrl(this.p8_Url));
            dic.Add("p9_MP", Guid.NewGuid().ToString("N"));
            dic.Add("pa_FrpId", this.pa_FrpId);
            dic.Add("pg_BankCode", this.BankValue);
            dic.Add("ph_Ip", IPAgent.IP);
            dic.Add("pi_Url", this.GetUrl(this.pi_Url));

            string signStr = string.Join(string.Empty, dic.Select(t => t.Value));
            dic.Add("hmac", MD5.HMACMD5(signStr, this.merchantKey).ToLower());


            string url = this.Gateway + "?" + dic.ToQueryString();
            if (new string[] { "QQWAP", "WEIXINWAP" }.Contains(this.pa_FrpId))
            {
                if (WebAgent.IsMobile())
                {
                    this.BuildForm(url);
                }
                else
                {
                    switch (this.pa_FrpId)
                    {
                        case "QQWAP":
                            this.CreateQQCode(url);
                            break;
                        case "WEIXINWAP":
                            this.CreateWXCode(url);
                            break;
                    }
                }
            }
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            if (!result.StartsWith("{"))
            {
                context.Response.Write(result);
                return;
            }



            int r1_Code = JsonAgent.GetValue<int>(result, "r1_Code");
            if (r1_Code != 1)
            {
                string msg = JsonAgent.GetValue<string>(result, "r7_Desc");
                context.Response.Write(string.IsNullOrEmpty(msg) ? result : msg);
                return;
            }
            string code = JsonAgent.GetValue<string>(result, "r3_PayInfo");

            switch (this.pa_FrpId)
            {
                case "WEIXIN":
                    this.CreateWXCode(code);
                    break;
                case "QQ":
                    this.CreateQQCode(code);
                    break;
                case "JDPAY":
                case "UnionPay":
                    this.CreateQRCode(code);
                    break;
                case "Nocard_H5":
                case "OnlinePay":
                    this.BuildForm(code);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("r1_Code", 0) != 1) return false;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in new string[] { "p1_MerId", "r0_Cmd", "r1_Code", "r2_TrxId", "r3_Amt", "r4_Cur", "r5_Pid", "r6_Order", "r8_MP", "r9_BType", "ro_BankOrderId", "rp_PayDate" })
            {
                dic.Add(key, WebAgent.GetParam(key));
            }
            string signStr = string.Join(string.Empty, dic.Select(t => t.Value));
            string hmac = MD5.HMACMD5(signStr, this.merchantKey).ToLower();
            if (hmac == WebAgent.GetParam("hmac"))
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
                if (this.pa_FrpId != "OnlinePay") return null;
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.CITIC, "ECITIC");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.COMM, "BOCO");
                dic.Add(BankType.BJBANK, "BOB");
                dic.Add(BankType.SPABANK, "PAB");
                dic.Add(BankType.CMB, "CMBCHINA");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.GDB, "CGB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.PSBC, "POST");
                dic.Add(BankType.SHBANK, "SHB");
                return dic;
            }
        }
    }
}
