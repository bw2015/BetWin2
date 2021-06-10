using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;
using SP.Studio.Text;
using BW.Agent;
using BW.Common.Sites;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Net;
using SP.Studio.Model;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class ASO : IPayment
    {
        public ASO() : base() { }

        public ASO(string setting) : base(setting) { }

        /// <summary>
        /// 网银服务
        /// </summary>
        private const string service_bank = "TRADE.B2C";

        /// <summary>
        /// 扫描服务
        /// </summary>
        private const string service_scan = "TRADE.SCANPAY";

        /// <summary>
        /// H5支付
        /// </summary>
        private const string service_h5 = "TRADE.H5PAY";

        private const string version = "1.0.0.0";


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

        /// <summary>
        /// 商户帐号
        /// </summary>
        [Description("商户帐号")]
        public string merId { get; set; }

        private string _notifyUrl = "/handler/payment/ASO";
        [Description("通知地址")]
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

        private int _typeId = 0;
        [Description("类型ID")]
        public int typeId
        {
            get
            {
                return _typeId;
            }
            set
            {
                _typeId = value;
            }
        }

        [Description("密钥")]
        public string Key { get; set; }

        [Description("备注")]
        public string Mark { get; set; }

        protected override string GetMark()
        {
            return this.Mark;
        }

        public override bool IsWechat()
        {
            return base.IsWechat() && (this.typeId == 1 || this.typeId == 2);
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string sign;
            string result;
            string code = null;
            switch (this.typeId)
            {
                case 0:
                case 100:
                    dic.Add("service", service_bank);
                    dic.Add("version", version);
                    dic.Add("merId", this.merId);
                    dic.Add("tradeNo", this.OrderID);
                    dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
                    dic.Add("amount", this.Money.ToString("0.00"));
                    dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
                    dic.Add("extra", this.Name);
                    dic.Add("summary", this.Name);
                    dic.Add("expireTime", "7200");
                    dic.Add("clientIp", IPAgent.IP);
                    dic.Add("bankId", this.BankValue);
                    sign = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
                    dic.Add("sign", MD5.Encryp(sign).ToUpper());
                    this.BuildForm(dic, Gateway);
                    break;
                case 11:
                case 12:
                case 13:
                case 15:
                    dic.Add("service", service_h5);
                    dic.Add("version", version);
                    dic.Add("merId", this.merId);
                    dic.Add("typeId", (this.typeId - 10).ToString());
                    dic.Add("tradeNo", this.OrderID);
                    dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
                    dic.Add("amount", this.Money.ToString("0.00"));
                    dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
                    dic.Add("extra", this.Name);
                    dic.Add("summary", this.Name);
                    dic.Add("expireTime", "7200");
                    dic.Add("clientIp", IPAgent.IP);
                    sign = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
                    dic.Add("sign", MD5.Encryp(sign).ToUpper());
                    if (WebAgent.IsMobile())
                    {
                        this.BuildForm(dic, this.Gateway);
                    }
                    else
                    {
                        code = string.Format("{0}{1}", SystemAgent.Instance().GetInviteDomain().FirstOrDefault().Domain,
                            SiteAgent.Instance().SaveCache(SiteCache.CacheType.Payment, dic, _GATEWAY, this.Gateway).ToString("N"));
                    }
                    break;
                default:
                    dic.Add("service", service_scan);
                    dic.Add("version", version);
                    dic.Add("merId", this.merId);
                    dic.Add("typeId", this.typeId.ToString());
                    dic.Add("tradeNo", this.OrderID);
                    dic.Add("tradeDate", DateTime.Now.ToString("yyyyMMdd"));
                    dic.Add("amount", this.Money.ToString("0.00"));
                    dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
                    dic.Add("extra", this.Name);
                    dic.Add("summary", this.Name);
                    dic.Add("expireTime", "7200");
                    dic.Add("clientIp", IPAgent.IP);
                    sign = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + this.Key;
                    dic.Add("sign", MD5.Encryp(sign).ToUpper());
                    result = NetAgent.UploadData(this.Gateway, string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8);
                    code = StringAgent.GetString(result, "<qrCode>", "</qrCode>");
                    if (string.IsNullOrEmpty(code))
                    {
                        HttpContext.Current.Response.Write(false, result);
                    }
                    byte[] bytes = Convert.FromBase64String(code);
                    code = Encoding.UTF8.GetString(bytes);
                    break;
            }

            switch (this.typeId)
            {
                case 1:
                case 11:
                    this.CreateAliCode(code);
                    break;
                case 2:
                case 12:
                    this.CreateWXCode(code);
                    break;
                case 3:
                case 13:
                    this.CreateQQCode(code);
                    break;
                default:
                    this.context.Response.Write(code);
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string[] fields = new string[] { "service", "merId", "tradeNo", "tradeDate", "opeNo", "opeDate", "amount", "status", "extra", "payTime" };
            string queryString = string.Join("&", fields.Select(t => string.Format("{0}={1}", t, WebAgent.GetParam(t)))) + this.Key;
            if (MD5.Encryp(queryString).ToUpper() == WebAgent.GetParam("sign"))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            systemId = WebAgent.GetParam("opeNo");
            money = WebAgent.GetParam("amount", decimal.Zero);
            return WebAgent.GetParam("tradeNo");
        }

        private Dictionary<BankType, string> _bankCode;

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.typeId != 0) return null;
                _bankCode = new Dictionary<BankType, string>();
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
    }
}
