using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

using SP.Studio.Json;
using SP.Studio.Array;
using SP.Studio.Web;
using BW.Common.Sites;
using SP.Studio.Net;
using BankType = BW.Common.Sites.BankType;


namespace BW.GateWay.Payment
{
    /// <summary>
    /// 金海哲
    /// </summary>
    public class JHZ : IPayment
    {
        public JHZ() : base() { }

        public JHZ(string setting) : base(setting) { }

        [Description("商户号")]
        public string merchantNo { get; set; }

        private string _pageUrl = "/handler/payment/JHZ";
        [Description("页面返回")]
        public string pageUrl
        {
            get
            {
                return this._pageUrl;
            }
            set
            {
                this._pageUrl = value;
            }
        }

        private string _backUrl = "/handler/payment/JHZ";
        [Description("服务器返回")]
        public string backUrl
        {
            get
            {
                return this._backUrl;
            }
            set
            {
                this._backUrl = value;
            }
        }

        /// <summary>
        /// 业务号
        /// </summary>
        [Description("业务号")]
        public string payCode { get; set; }

        /// <summary>
        /// 是否是网银
        /// </summary>
        [Description("网银(1),微信(2),QQ(3)")]
        public int IsBank { get; set; }


        /// <summary>
        /// 私钥
        /// </summary>
        [Description("私钥")]
        public string privateKey { get; set; }

        /// <summary>
        /// 公钥
        /// </summary>
        [Description("公匙")]
        public string publicKey { get; set; }

        /// <summary>
        /// 分支机构号
        /// </summary>
        private const String agencyCode = "";

        /// <summary>
        /// 币种
        /// </summary>
        private const string cur = "CNY";//币种

        /// <summary>
        /// 付款方银行类型 11：私人借记卡
        /// </summary>
        private const string bankAccountType = "11";

        /// <summary>
        /// 网关地址
        /// </summary>
        private const string GATEWAY = "http://zf.szjhzxxkj.com/ownPay/pay";

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("merchantNo", this.merchantNo);
            dic.Add("requestNo", this.OrderID);
            dic.Add("amount", ((int)(this.Money * 100)).ToString());
            dic.Add("payCode", this.payCode);
            dic.Add("pageUrl", this.GetUrl(this.pageUrl));
            dic.Add("backUrl", this.GetUrl(this.backUrl));
            dic.Add("payDate", WebAgent.GetTimeStamps().ToString());
            dic.Add("agencyCode", agencyCode);
            dic.Add("cashier", "0");
            dic.Add("remark1", this.Name);
            dic.Add("remark2", this.Description);
            dic.Add("remark3", string.Empty);
            if (this.IsBank == 1)
            {
                dic.Add("cur", cur);
                dic.Add("bankAccountType", bankAccountType);
                dic.Add("bankType", this.BankValue);
            }
            //zz1491987655816Q5RVT43F9B|20170412235947884|10000|http://localhost/handler/payment/JHZ|http://localhost/handler/payment/JHZ|1492012787|0|ceshi01||
            string content = string.Join("|", "merchantNo,requestNo,amount,pageUrl,backUrl,payDate,agencyCode,remark1,remark2,remark3".Split(',').Select(t => dic.Get(t, string.Empty)));
            dic.Add("signature", this.sign(content));
            switch (this.IsBank)
            {
                case 1:
                    this.BuildForm(dic, GATEWAY, "POST");
                    break;
                case 2:
                case 3:
                    string postdata = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
                    string result = NetAgent.UploadData(GATEWAY, postdata, Encoding.UTF8);
                    Regex regex = new Regex(@"""backQrCodeUrl"":""(?<Code>[^\""]+)""");
                    if (!regex.IsMatch(result))
                    {
                        HttpContext.Current.Response.Write(result);
                    }
                    else
                    {
                        string code = regex.Match(result).Groups["Code"].Value;
                        if (this.IsBank == 2)
                        {
                            this.CreateWXCode(code);
                        }
                        else if (this.IsBank == 3)
                        {
                            this.CreateQQCode(code);
                        }
                    }
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string ret = WebAgent.GetParam("ret");
            string msg = WebAgent.GetParam("msg");
            string sign = WebAgent.GetParam("sign");
            if (!ret.Contains("1000")) return false;

            if (this.Verify(ret + "|" + msg, sign, this.RSAPublicKeyJava2DotNet(this.publicKey)))
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string ShowCallback()
        {
            return "SUCCESS";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            string msg = WebAgent.GetParam("msg");
            systemId = string.Empty;
            money = decimal.Zero;

            Hashtable ht = JsonAgent.GetJObject(msg);
            if (ht == null) return null;
            money = decimal.Parse(ht["money"].ToString()) / 100M;
            systemId = ht["payNo"].ToString();
            return ht["no"].ToString();
        }

        private Dictionary<BankType, string> _bank;
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.IsBank != 1) return null;
                _bank = new Dictionary<BankType, string>();
                _bank.Add(BankType.BOC, "1041000");
                _bank.Add(BankType.ABC, "1031000");
                _bank.Add(BankType.ICBC, "1021000");
                _bank.Add(BankType.CCB, "1051000");
                _bank.Add(BankType.COMM, "3012900");
                _bank.Add(BankType.CMB, "3085840");
                _bank.Add(BankType.CMBC, "3051000");
                _bank.Add(BankType.CIB, "3093910");
                _bank.Add(BankType.SPDB, "3102900");
                _bank.Add(BankType.GDB, "3065810");
                _bank.Add(BankType.CITIC, "3021000");
                _bank.Add(BankType.CEB, "3031000");
                _bank.Add(BankType.PSBC, "4031000");
                _bank.Add(BankType.SPABANK, "3071000");
                _bank.Add(BankType.BJBANK, "3131000");
                _bank.Add(BankType.NJCB, "3133010");
                _bank.Add(BankType.NBBANK, "3133320");
                _bank.Add(BankType.SHRCB, "3222900");
                _bank.Add(BankType.HKBEA, "5021000");
                return _bank;
            }
        }

        /// <summary>
        /// 用私钥签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string sign(string data)
        {
            string key = this.RSAPrivateKeyJava2DotNet(this.privateKey);
            RSACryptoServiceProvider rsaCsp = new RSACryptoServiceProvider();
            rsaCsp.FromXmlString(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = rsaCsp.SignData(dataBytes, "SHA1");
            return Convert.ToBase64String(signatureBytes);
        }

        private string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>
        /// 公匙转换
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        private string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>
        /// 校验源码
        /// </summary>
        /// <param name="OriginalString">源内容</param>
        /// <param name="SignatureString">服务端返回签名</param>
        /// <param name="publicKey">签名之后的公匙</param>
        /// <returns></returns>
        private bool Verify(String OriginalString, String SignatureString, String publicKey)
        {
            //将base64签名数据转码为字节   
            byte[] signedBase64 = Convert.FromBase64String(SignatureString);
            byte[] orgin = Encoding.UTF8.GetBytes(OriginalString);

            RSACryptoServiceProvider oRSA = new RSACryptoServiceProvider();
            oRSA.FromXmlString(publicKey);

            bool bVerify = oRSA.VerifyData(orgin, "SHA1", signedBase64);
            return bVerify;

        }
    }
}
