using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Security;
using BW.Common.Sites;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 国付宝
    /// </summary>
    public class GoPay : IPayment
    {
        public GoPay() : base() { }

        public GoPay(string setting) : base(setting) { }

        /// <summary>
        /// 网关版本号
        /// </summary>
        private const string version = "2.2";

        /// <summary>
        /// 字符集（UTF-8）
        /// </summary>
        private const string charset = "2";

        /// <summary>
        /// 中文
        /// </summary>
        private const string language = "1";

        /// <summary>
        /// 加密方式 MD5
        /// </summary>
        private const string signType = "1";

        /// <summary>
        /// 交易代码
        /// </summary>
        private const string tranCode = "8888";

        /// <summary>
        /// 币种（人民币）
        /// </summary>
        private const string currencyType = "156";

        [Description("商户代码")]
        public string merchantID { get; set; }

        private string _frontMerUrl = "/handler/payment/GoPay";
        [Description("前台通知")]
        public string frontMerUrl
        {
            get
            {
                return this._frontMerUrl;
            }
            set
            {
                this._frontMerUrl = value;
            }
        }

        private string _backgroundMerUrl = "/handler/payment/GoPay";
        [Description("后台通知")]
        public string backgroundMerUrl
        {
            get
            {
                return this._backgroundMerUrl;
            }
            set
            {
                this._backgroundMerUrl = value;
            }
        }

        [Description("转入账户")]
        public string virCardNoIn { get; set; }

        [Description("商城域名")]
        public string Shop { get; set; }

        [Description("密钥")]
        public string VerficationCode { get; set; }

        private string _gateway = "https://gateway.gopay.com.cn/Trans/WebClientAction.do";
        [Description("支付网关")]
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

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("tranAmt", decimal.Zero);
            systemId = WebAgent.GetParam("orderId");
            return WebAgent.GetParam("merOrderNum");
        }

        public override void GoGateway()
        {
            string gopayServerTime = NetAgent.DownloadData("https://gateway.gopay.com.cn/time.do", Encoding.UTF8);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("version", version);
            dic.Add("charset", charset);
            dic.Add("language", language);
            dic.Add("signType", signType);
            dic.Add("tranCode", tranCode);
            dic.Add("merchantID", this.merchantID);
            dic.Add("merOrderNum", this.OrderID);
            dic.Add("tranAmt", this.Money.ToString("0.00"));
            dic.Add("currencyType", currencyType);
            dic.Add("frontMerUrl", this.GetUrl(this.frontMerUrl));
            dic.Add("backgroundMerUrl", this.GetUrl(this.backgroundMerUrl));
            dic.Add("tranDateTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("virCardNoIn", this.virCardNoIn);
            dic.Add("tranIP", IPAgent.IP);
            dic.Add("VerficationCode", this.VerficationCode);
            string signValue = string.Join(string.Empty, new string[] {
                "version", "tranCode","merchantID","merOrderNum","tranAmt","feeAmt","tranDateTime","frontMerUrl","backgroundMerUrl","orderId","gopayOutOrderId","tranIP",
                "respCode","gopayServerTime","VerficationCode" }.Select(t => string.Format("{0}=[{1}]", t, dic.ContainsKey(t) ? dic[t] : string.Empty)));
            dic.Add("signValue", MD5.toMD5(signValue).ToLower());
            dic.Remove("VerficationCode");
            dic.Add(_GATEWAY, this.Gateway);
            dic.Add("bankCode", this.BankValue);
            dic.Add("userType", "1");

            this.BuildForm(dic, this.GetGateway(this.Shop, this.Gateway), "POST");
        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("respCode") != "0000") return false;
            string sign = WebAgent.GetParam("signValue");
            string plain = string.Join(string.Empty, "version,tranCode,merchantID,merOrderNum,tranAmt,feeAmt,tranDateTime,frontMerUrl,backgroundMerUrl,orderId,gopayOutOrderId,tranIP,respCode,gopayServerTime".Split(',').Select(t => string.Format("{0}=[{1}]", t, WebAgent.GetParam(t))));
            plain += "VerficationCode=[" + this.VerficationCode + "]";

            if (sign == MD5.toMD5(plain).ToLower())
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string ShowCallback()
        {
            return "RespCode=0000|JumpURL=" + this.GetUrl("/handler/payment/SUCCESS");
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                Dictionary<BankType, string> bank = new Dictionary<BankType, string>();
                bank.Add(BankType.CCB, "CCB");
                bank.Add(BankType.CMB, "CMB");
                bank.Add(BankType.ICBC, "ICBC");
                bank.Add(BankType.BOC, "BOC");
                bank.Add(BankType.ABC, "ABC");
                bank.Add(BankType.COMM, "BCOMM");
                bank.Add(BankType.CMBC, "CMBC");
                bank.Add(BankType.HXBANK, "HXBC");
                bank.Add(BankType.CIB, "CIB");
                bank.Add(BankType.SPDB, "SPDB");
                bank.Add(BankType.GDB, "GDB");
                bank.Add(BankType.CITIC, "CITIC");
                bank.Add(BankType.CEB, "CEB");
                bank.Add(BankType.PSBC, "PSBC");
                bank.Add(BankType.BJBANK, "BJBANK");
                bank.Add(BankType.SHBANK, "BOS");
                bank.Add(BankType.SPABANK, "PAB");
                bank.Add(BankType.NBBANK, "NBCB");
                bank.Add(BankType.NJCB, "NJCB");
                return bank;
            }
        }
    }
}
