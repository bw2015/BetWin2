using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Web;

using SP.Studio.Web;
using BW.Common.Sites;
using SP.Studio.Security;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 易势支付
    /// </summary>
    public class IEPLM : IPayment
    {
        public IEPLM() : base() { }

        public IEPLM(string setting) : base(setting) { }

        private const string version = "v1";

        private const string currency = "CNY";

        /// <summary>
        /// 01：借记卡  02：贷记卡
        /// </summary>
        private const string cardType = "01";

        /// <summary>
        /// 业务代码
        /// </summary>
        private const string bizType = "01";

        /// <summary>
        /// 证书类型
        /// </summary>
        private const string signType = "PKCS12";


        [Description("商户编号")]
        public string merchantNo { get; set; }

        private string _channeNo = "03";
        /// <summary>
        /// 渠道编号
        /// </summary>
        [Description("渠道编号")]
        public string channelNo { get { return this._channeNo; } set { this._channeNo = value; } }

        private string _notifyUrl = "/handler/payment/IEPLM";
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

        private string _returnUrl = "/handler/payment/IEPLM";
        [Description("跳转地址")]
        public string returnUrl
        {
            get
            {
                return this._returnUrl;
            }
            set
            {
                this._returnUrl = value;
            }
        }

        private string _goodsName = "domain";
        [Description("商品名字")]
        public string goodsName
        {
            get
            {
                return this._goodsName;
            }
            set
            {
                this._goodsName = value;
            }
        }

        /// <summary>
        /// pfx 文件
        /// </summary>
        [Description("私钥")]
        public string privateCert { get; set; }

        [Description("私钥密码")]
        public string certPass { get; set; }

        /// <summary>
        /// cer 文件
        /// </summary>
        [Description("公钥")]
        public string publicCert { get; set; }

        [Description("商城域名")]
        public string Shop { get; set; }

        private string _payUrl = "https://cashier.ielpm.com/paygate/v1/web/pay";
        [Description("支付网关")]
        public string PayUrl
        {
            get
            {
                return this._payUrl;
            }
            set
            {
                this._payUrl = value;
            }
        }

        private string _scanUrl = "https://cashier.ielpm.com/paygate/v1/smpay";
        [Description("扫码网关")]
        public string ScanUrl
        {
            get
            {
                return this._scanUrl;
            }
            set
            {
                this._scanUrl = value;
            }
        }

        [Description("支付类型")]
        public int payType { get; set; }

        [Description("入驻ID")]
        public string bindId { get; set; }

        public override void GoGateway()
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>(new IELPMComparer());
            switch (this.payType)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    dic.Add("merchantNo", this.merchantNo);
                    dic.Add("version", version);
                    dic.Add("channelNo", "05");
                    dic.Add("tranCode", "YS1003");
                    dic.Add("tranFlow", this.OrderID);
                    dic.Add("tranDate", DateTime.Now.ToString("yyyyMMdd"));
                    dic.Add("tranTime", DateTime.Now.ToString("HHmmss"));
                    dic.Add("amount", ((int)this.Money * 100).ToString());
                    dic.Add("payType", this.payType.ToString());
                    dic.Add("bindId", this.bindId);
                    dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
                    dic.Add("bizType", "");
                    dic.Add("goodsName", this.Name);
                    break;
                default:
                    dic.Add("merchantNo", this.merchantNo);
                    dic.Add("version", version);
                    dic.Add("channelNo", this.channelNo);
                    dic.Add("tranSerialNum", this.OrderID);
                    dic.Add("bankId", this.BankValue);
                    dic.Add("tranTime", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    dic.Add("currency", currency);
                    dic.Add("amount", ((int)this.Money * 100).ToString());
                    dic.Add("bizType", bizType);
                    dic.Add("goodsName", this.goodsName);
                    dic.Add("goodsInfo", "");
                    dic.Add("goodsNum", "1");
                    dic.Add("notifyUrl", this.GetUrl(this.notifyUrl));
                    dic.Add("returnUrl", this.GetUrl(this.returnUrl));
                    dic.Add("buyerName", this.EncryptData(this.Name));
                    dic.Add("buyerId", this.Name);
                    dic.Add("contact", this.EncryptData(this.Name));
                    dic.Add("valid", "60");
                    dic.Add("cardType", cardType);
                    dic.Add("ip", IPAgent.IP);
                    dic.Add("referer", string.IsNullOrEmpty(this.Shop) ? HttpContext.Current.Request.UrlReferrer.ToString() : string.Format("http://{0}/handler/payment/Redirect", this.Shop));
                    dic.Add("remark", this.Description);
                    dic.Add("YUL1", Guid.NewGuid().ToString("N").Substring(0, 8));
                    dic.Add("sign", this.Sign(dic));
                    if (!string.IsNullOrEmpty(this.Shop)) dic.Add(_GATEWAY, this.PayUrl);
                    this.BuildForm(dic, this.GetGateway(this.Shop, this.PayUrl));

                    break;
            }

        }

        public override bool Verify(VerifyCallBack callback)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>(new IELPMComparer());
            foreach (string key in HttpContext.Current.Request.Form.AllKeys)
            {
                dic.Add(key, HttpContext.Current.Request.Form[key]);
            }
            if (dic["rtnCode"] != "0000") return false;
            if (this.Validate(dic, Encoding.UTF8))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("amount", decimal.Zero) / 100M;
            systemId = WebAgent.GetParam("paySerialNo");
            return WebAgent.GetParam("tranSerialNum");
        }

        public override string ShowCallback()
        {
            return "YYYYYY";
        }

        private Dictionary<BankType, string> _bank;
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (_bank == null)
                {
                    _bank = new Dictionary<BankType, string>();
                    _bank.Add(BankType.PSBC, "01000000");
                    _bank.Add(BankType.ICBC, "01020000");
                    _bank.Add(BankType.ABC, "01030000");
                    _bank.Add(BankType.BOC, "01040000");
                    _bank.Add(BankType.CCB, "01050000");
                    _bank.Add(BankType.COMM, "03010000");
                    _bank.Add(BankType.CITIC, "03020000");
                    _bank.Add(BankType.CEB, "03030000");
                    _bank.Add(BankType.HXBANK, "03040000");
                    _bank.Add(BankType.CMBC, "03050000");
                    _bank.Add(BankType.GDB, "03060000");
                    _bank.Add(BankType.SPABANK, "04100000");
                    _bank.Add(BankType.CMB, "03080000");
                    _bank.Add(BankType.CIB, "03090000");
                    _bank.Add(BankType.SPDB, "03100000");
                    _bank.Add(BankType.BOHAIB, "03170000");
                    _bank.Add(BankType.HKBEA, "03200000");
                    _bank.Add(BankType.SHBANK, "04012900");
                    _bank.Add(BankType.BJBANK, "04031000");
                    _bank.Add(BankType.NBBANK, "04083320");
                    _bank.Add(BankType.NJCB, "04243010");
                    _bank.Add(BankType.CDCB, "64296510");
                    _bank.Add(BankType.SHRCB, "65012900");
                }
                return _bank;
            }
        }

        #region ============ 工具方法  ===============

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private string Sign(SortedDictionary<string, string> dic)
        {
            string stringData = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            //stringData = "YUL1=" + dic["YUL1"] + "&" + stringData;

            byte[] byteSign = this.SignBySoft(GetSignProviderFromPfx(), Encoding.UTF8.GetBytes(stringData));
            return Convert.ToBase64String(byteSign);
        }

        public bool Validate(SortedDictionary<string, string> data, Encoding encoding)
        {
            string signValue = data["sign"];
            byte[] signByte = Convert.FromBase64String(signValue);
            data.Remove("sign");
            string stringData = string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            RSACryptoServiceProvider provider = this.GetValidateProviderFromPath();
            return null != provider && this.ValidateBySoft(provider, signByte, encoding.GetBytes(stringData));
        }

        /// <summary>
        /// 私钥
        /// </summary>
        /// <returns></returns>
        public RSACryptoServiceProvider GetSignProviderFromPfx()
        {
            X509Certificate2 pc = new X509Certificate2(Convert.FromBase64String(this.privateCert), this.certPass);
            return (RSACryptoServiceProvider)pc.PrivateKey;
        }

        /// <summary>
        /// 公钥
        /// </summary>
        /// <returns></returns>
        public RSACryptoServiceProvider GetValidateProviderFromPath()
        {
            X509Certificate2 pc = new X509Certificate2(Convert.FromBase64String(this.publicCert));
            return (RSACryptoServiceProvider)pc.PublicKey.Key;
        }

        public bool ValidateBySoft(RSACryptoServiceProvider provider, byte[] base64DecodingSignStr, byte[] srcByte)
        {
            HashAlgorithm hashalg = new SHA1CryptoServiceProvider();
            return provider.VerifyData(srcByte, hashalg, base64DecodingSignStr);
        }

        public byte[] SignBySoft(RSACryptoServiceProvider provider, byte[] data)
        {
            byte[] res = null;
            try
            {
                HashAlgorithm hashalg = new SHA1CryptoServiceProvider();
                res = provider.SignData(data, hashalg);
            }
            catch (Exception e)
            {
                throw e;
            }
            byte[] result;
            if (null == res)
            {
                result = null;
            }
            else
            {
                result = res;
            }
            return result;
        }

        /// <summary>
        /// 加密敏感数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string EncryptData(string data)
        {
            Encoding encoding = Encoding.UTF8;
            string result;
            if (string.IsNullOrEmpty(data))
            {
                result = "";
            }
            else
            {
                result = Convert.ToBase64String(encryptedData(encoding.GetBytes(data)));
            }
            return result;
        }

        public byte[] encryptedData(byte[] encData)
        {
            byte[] result;
            try
            {
                RSACryptoServiceProvider p = this.GetValidateProviderFromPath();
                byte[] enBytes = p.Encrypt(encData, false);
                result = enBytes;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
        private class IELPMComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                char[] v = x.ToCharArray();
                char[] v2 = y.ToCharArray();
                int len = v.Length;
                int len2 = v2.Length;
                int lim = (len > len2) ? len2 : len;
                int result;
                for (int i = 0; i < lim; i++)
                {
                    char c = v[i];
                    char c2 = v2[i];
                    if (c != c2)
                    {
                        result = (int)(c - c2);
                        return result;
                    }
                }
                result = len - len2;
                return result;
            }
        }
    }
}
