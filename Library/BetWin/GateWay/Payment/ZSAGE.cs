using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;

using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Model;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 圣泽支付
    /// </summary>
    public class ZSAGE : IPayment
    {
        public ZSAGE() : base() { }

        public ZSAGE(string setting) : base(setting) { }

        /// <summary>
        /// 模块名 微信扫码支付
        /// </summary>
        private const string model = "QR_CODE";

        [Description("商户号")]
        public string merchantCode { get; set; }

        private string _noticeUrl = "/handler/payment/ZSAGE";
        [Description("通知地址")]
        public string noticeUrl
        {
            get
            {
                return this._noticeUrl;
            }
            set
            {
                this._noticeUrl = value;
            }
        }

        private string _merUrl = "/handler/payment/ZSAGE";
        [Description("回调地址")]
        public string merUrl
        {
            get
            {
                return this._merUrl;
            }
            set
            {
                this._merUrl = value;
            }
        }

        [Description("渠道 网银:0 微信:21 支付宝:30 QQ扫码:31 银联扫码:38 京东扫码:39")]
        public string payChannel { get; set; }

        [Description("密钥")]
        public string KEY { get; set; }

        [Description("商城域名")]
        public string Shop { get; set; }

        [Description("备注信息")]
        public string Remark { get; set; }

        private const string GATEWAY = "http://payment.zsagepay.com/scan/entrance.do";

        private string _bankGateway = "http://payment.zsagepay.com/ebank/pay.do";
        [Description("网银网关")]
        public string BankGateway
        {
            get
            {
                return this._bankGateway;
            }
            set
            {
                this._bankGateway = value;
            }
        }

        public override bool IsWechat()
        {
            return this.payChannel == "21" && base.IsWechat();
        }

        protected override string GetMark()
        {
            return this.Remark;
        }

        public override string ShowCallback()
        {
            return "{'code':'00'}";
        }

        public override void GoGateway()
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string msg;

            switch (this.payChannel)
            {
                case "0":
                    #region ================ 网银  ================

                    dic.Add("merchantCode", this.merchantCode);
                    dic.Add("outOrderId", this.OrderID);
                    dic.Add("totalAmount", ((int)(this.Money * 100)).ToString());
                    dic.Add("orderCreateTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    dic.Add("lastPayTime", DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss"));
                    dic.Add("merUrl", this.GetUrl(this.merUrl));
                    dic.Add("noticeUrl", this.GetUrl(this.noticeUrl));
                    dic.Add("bankCode", this.BankValue);
                    dic.Add("bankCardType", "01");
                    string signStr = string.Join("&", new string[] { "lastPayTime", "merchantCode", "orderCreateTime", "outOrderId", "totalAmount" }.
                        Select(t => string.Format("{0}={1}", t, dic[t]))) + "&KEY=" + this.KEY;

                    dic.Add("sign", MD5.toMD5(signStr));
                    this.BuildForm(dic, this.BankGateway);

                    #endregion
                    break;
                case "21":
                case "30":
                case "31":
                case "38":
                case "39":
                    #region =========== 支付宝&微信&QQ扫码 ================
                    dic.Add("model", model);
                    dic.Add("merchantCode", this.merchantCode);
                    dic.Add("outOrderId", this.OrderID);
                    dic.Add("amount", ((int)(this.Money * 100)).ToString());
                    dic.Add("goodsName", this.Name);
                    dic.Add("orderCreateTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    dic.Add("lastPayTime", DateTime.Now.AddDays(1).ToString("yyyyMMddHHmmss"));
                    dic.Add("noticeUrl", this.GetUrl(this.noticeUrl));
                    dic.Add("isSupportCredit", "1");
                    dic.Add("ip", IPAgent.IP);
                    dic.Add("payChannel", this.payChannel);
                    msg = string.Join("&", new string[] { "merchantCode", "outOrderId", "amount", "orderCreateTime", "noticeUrl", "isSupportCredit" }.OrderBy(t => t).Select(t => string.Format("{0}={1}", t, dic[t]))) + string.Format("&KEY={0}", this.KEY);
                    dic.Add("sign", MD5.toMD5(msg));
                    string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
                    string result = NetAgent.UploadData(GATEWAY, data, Encoding.UTF8);
                    Regex regex = new Regex(@"""url"":""(?<Code>.+?)""");
                    if (regex.IsMatch(result))
                    {
                        string code = regex.Match(result).Groups["Code"].Value;
                        switch (this.payChannel)
                        {
                            case "21":
                                this.CreateWXCode(code);
                                break;
                            case "30":
                                this.CreateAliCode(code);
                                break;
                            case "31":
                                this.CreateQQCode(code);
                                break;
                            case "38":
                            case "39":
                                this.CreateQRCode(code);
                                break;
                        }

                    }
                    else
                    {
                        HttpContext.Current.Response.Write(false, "发生错误", new
                        {
                            data = result
                        });
                    }
                    #endregion
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string msg = string.Join("&", new string[] { "merchantCode", "instructCode", "transType", "outOrderId", "transTime", "totalAmount" }.OrderBy(t => t).Select(t => string.Format("{0}={1}", t, WebAgent.GetParam(t)))) + string.Format("&KEY={0}", this.KEY);
            string sign = MD5.toMD5(msg);
            if (sign == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("instructCode");
            money = WebAgent.GetParam("totalAmount", decimal.Zero) / 100M;
            return WebAgent.GetParam("outOrderId");
        }


        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.payChannel != "0") return null;
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
    }
}
