using BW.Common.Sites;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using SP.Studio.Array;
using SP.Studio.Model;
using SP.Studio.Net;
using SP.Studio.Text;
using SP.Studio.Web;
using SP.Studio.Xml;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 智付
    /// </summary>
    public class DinPay : IPayment
    {
        public DinPay() { }

        public DinPay(string setting)
            : base(setting)
        {

        }

        private string _interface_version = "V3.0";
        /// <summary>
        /// 接口版本
        /// </summary>
        [Description("版本号")]
        public string interface_version
        {
            get
            {
                return this._interface_version;
            }
            set
            {
                this._interface_version = value;
            }
        }

        /// <summary>
        /// 参数编码字符集
        /// </summary>
        private const string input_charset = "UTF-8";

        /// <summary>
        /// 签名方式
        /// </summary>
        private const string sign_type = "RSA-S";

        [Description("商家号")]
        public string merchant_code { get; set; }


        private string _service_type = "direct_pay";
        [Description("服务类型")]
        public string service_type
        {
            get
            {
                return this._service_type;
            }
            set
            {
                this._service_type = value;
            }
        }

        /// <summary>
        /// 商户私钥
        /// </summary>
        [Description("商户私钥")]
        public string merchant_private_key { get; set; }

        /// <summary>
        /// 回调验签（智付公钥）
        /// </summary>
        [Description("智付公钥")]
        public string dinpay_public_key { get; set; }

        private string _notify_url = "/handler/payment/DinPay";
        [Description("异步通知地址")]
        public string notify_url
        {
            get
            {
                return this._notify_url;
            }
            set
            {
                this._notify_url = value;
            }
        }

        private string _return_url = "/handler/payment/DinPay";

        [Description("同步通知地址")]
        public string return_url
        {
            get
            {
                return this._return_url;
            }
            set
            {
                this._return_url = value;
            }
        }

        /// <summary>
        /// 支付类型
        /// </summary>
        private string _pay_type = "b2c,plateform,dcard,express,weixin";
        [Description("支付类型")]
        public string pay_type
        {
            get
            {
                return this._pay_type;
            }
            set
            {
                _pay_type = value;
            }
        }

        /// <summary>
        /// 商城网关
        /// </summary>
        [Description("商城网关")]
        public string Shop { get; set; }

        /// <summary>
        /// 网银网关
        /// </summary>
        private string _gateway = "https://pay.dinpay.com/gateway?input_charset=UTF-8";
        [Description("网银网关")]
        public string BankPay
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
        /// 扫码网关
        /// </summary>
        private string _scanpay = "https://api.dinpay.com/gateway/api/scanpay";
        [Description("扫码网关")]
        public string ScanPay
        {
            get
            {
                return this._scanpay;
            }
            set
            {
                this._scanpay = value;
            }
        }

        [Description("备注信息")]
        public string Remark { get; set; }

        protected override string GetMark()
        {
            return this.Remark;
        }

        public override void GoGateway()
        {
            string order_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Dictionary<string, string> dic = new Dictionary<string, string>();

            dic.Add("merchant_code", this.merchant_code);
            dic.Add("service_type", service_type);
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("interface_version", this.interface_version);
            dic.Add("input_charset", input_charset);
            dic.Add("sign_type", sign_type);
            dic.Add("return_url", this.GetUrl(this.return_url));
            dic.Add("order_no", this.OrderID);
            dic.Add("order_time", order_time);
            dic.Add("order_amount", this.Money.ToString("0.00"));
            dic.Add("product_name", this.Name);

            string data, result;
            switch (this.service_type)
            {
                case "direct_pay":
                case "wxpub_pay":
                    dic.Add("pay_type", this.pay_type);
                    dic.Add("bank_code", this.BankValue);
                    dic.Add("sign", this.GetSign(dic));
                    dic.Add(_GATEWAY, _gateway);
                    this.BuildForm(dic, this.GetGateway(this.Shop, _gateway));
                    break;
                case "weixin_scan":
                case "alipay_scan":
                case "tenpay_scan":
                case "qq_scan":
                    dic.AddOrUpdate("interface_version", "V3.1");
                    dic.AddOrUpdate("client_ip", IPAgent.IP);
                    dic.Add("sign", this.GetSign(dic));

                    data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
                    result = null;
                    result = NetAgent.UploadData(this.ScanPay, data, Encoding.UTF8);
                    string code = HttpUtility.UrlDecode(StringAgent.GetString(result, "<qrcode>", "</qrcode>"));
                    if (string.IsNullOrEmpty(code))
                    {
                        HttpContext.Current.Response.ContentType = "text/xml";
                        HttpContext.Current.Response.Write(result);
                        return;
                    }
                    code = code.Replace("&amp;", "&");
                    switch (this.service_type)
                    {
                        case "weixin_scan":
                            this.CreateWXCode(code);
                            break;
                        case "alipay_scan":
                            this.CreateAliCode(code);
                            break;
                        case "tenpay_scan":
                        case "qq_scan":
                            this.CreateQQCode(code);
                            break;
                    }
                    break;
                case "h5_qq":
                case "h5_wx":
                case "h5_ali":
                case "alipay_h5api":
                    // dic.AddOrUpdate("interface_version", "V3.3");
                    dic.AddOrUpdate("client_ip", IPAgent.IP);
                    dic.Add("sign", this.GetSign(dic));
                    this.BuildForm(dic, this.GetGateway(this.Shop, _gateway));
                    break;
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            //获取智付反馈信息
            string merchant_code = WebAgent.GetParam("merchant_code");
            string notify_type = WebAgent.GetParam("notify_type");
            string notify_id = WebAgent.GetParam("notify_id");
            string interface_version = WebAgent.GetParam("interface_version");
            string sign_type = WebAgent.GetParam("sign_type");
            string dinpaysign = WebAgent.GetParam("sign");
            string order_no = WebAgent.GetParam("order_no");
            string order_time = WebAgent.GetParam("order_time");
            string order_amount = WebAgent.GetParam("order_amount");
            string extra_return_param = WebAgent.GetParam("extra_return_param");
            string trade_no = WebAgent.GetParam("trade_no");
            string trade_time = WebAgent.GetParam("trade_time");
            string trade_status = WebAgent.GetParam("trade_status");
            string bank_seq_no = WebAgent.GetParam("bank_seq_no");

            SortedDictionary<string, string> data = new SortedDictionary<string, string>();
            data.Add("bank_seq_no", bank_seq_no);
            data.Add("extra_return_param", extra_return_param);
            data.Add("interface_version", interface_version);
            data.Add("merchant_code", merchant_code);
            data.Add("notify_id", notify_id);
            data.Add("notify_type", notify_type);
            data.Add("order_amount", order_amount);
            data.Add("order_no", order_no);
            data.Add("order_time", order_time);
            data.Add("trade_no", trade_no);
            data.Add("trade_status", trade_status);
            data.Add("trade_time", trade_time);

            string signStr = string.Join("&", data.Where(t => !string.IsNullOrEmpty(t.Value)).Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            //string signStr = "";
            //if (!string.IsNullOrEmpty(bank_seq_no)) signStr = signStr + "bank_seq_no=" + bank_seq_no.ToString().Trim() + "&";
            //if (!string.IsNullOrEmpty(extra_return_param)) signStr = signStr + "extra_return_param=" + extra_return_param + "&";
            //signStr = signStr + "interface_version=V3.0" + "&";
            //signStr = signStr + "merchant_code=" + merchant_code + "&";
            //if (!string.IsNullOrEmpty(notify_id)) signStr = signStr + "notify_id=" + notify_id + "&notify_type=" + notify_type + "&";
            //signStr = signStr + "order_amount=" + order_amount + "&";
            //signStr = signStr + "order_no=" + order_no + "&";
            //signStr = signStr + "order_time=" + order_time + "&";
            //signStr = signStr + "trade_no=" + trade_no + "&";
            //signStr = signStr + "trade_status=" + trade_status + "&";
            //if (!string.IsNullOrEmpty(trade_time)) signStr = signStr + "trade_time=" + trade_time;

            string key = HttpHelp.RSAPublicKeyJava2DotNet(dinpay_public_key);

            bool result = HttpHelp.ValidateRsaSign(signStr, key, dinpaysign);
            if (result)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = WebAgent.GetParam("order_amount", decimal.Zero);
            systemId = WebAgent.GetParam("trade_no");
            return WebAgent.GetParam("order_no");
        }

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private string GetSign(Dictionary<string, string> dic)
        {

            string[] fiels = new string[] { "bank_code", "client_ip", "extend_param", "extra_return_param", "input_charset", "interface_version", "merchant_code", "notify_url", "order_amount", "order_no", "order_time", "pay_type", "product_code", "product_desc", "product_name", "product_num", "redo_flag", "return_url", "service_type", "show_url" };
            string signStr = string.Join("&", fiels.Where(t => dic.ContainsKey(t)).Select(t => string.Format("{0}={1}", t, dic.Get(t, string.Empty))));
            try
            {
                string key = HttpHelp.RSAPrivateKeyJava2DotNet(merchant_private_key);
                return HttpHelp.RSASign(signStr, key);
            }
            catch (Exception ex)
            {
                BW.Agent.SystemAgent.Instance().AddErrorLog(0, ex, ex.Message);
                return string.Empty;
            }
        }

        private Dictionary<BankType, string> _code;
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.service_type != "direct_pay") return null;
                if (this._code == null)
                {
                    this._code = new Dictionary<BankType, string>();
                    //农业银行ABC
                    this._code.Add(BankType.ABC, "ABC");
                    //工商银行ICBC
                    this._code.Add(BankType.ICBC, "ICBC");
                    //建设银行CCB
                    this._code.Add(BankType.CCB, "CCB");
                    //交通银行BCOM
                    this._code.Add(BankType.COMM, "BCOM");
                    //中国银行BOC
                    this._code.Add(BankType.BOC, "BOC");
                    //招商银行CMB
                    this._code.Add(BankType.CMB, "CMB");
                    //民生银行CMBC
                    this._code.Add(BankType.CMBC, "CMBC");
                    //光大银行CEBB
                    this._code.Add(BankType.CEB, "CEBB");
                    //北京银行BOB
                    this._code.Add(BankType.BJBANK, "BOB");
                    //上海银行SHB
                    this._code.Add(BankType.SHBANK, "SHB");
                    //宁波银行NBB
                    this._code.Add(BankType.NBBANK, "NBB");
                    //华夏银行HXB
                    this._code.Add(BankType.HXBANK, "HXB");
                    //兴业银行CIB
                    this._code.Add(BankType.CIB, "CIB");
                    //中国邮政银行PSBC
                    this._code.Add(BankType.PSBC, "PSBC");
                    //平安银行SPABANK
                    this._code.Add(BankType.SPABANK, "SPABANK");
                    //浦发银行SPDB
                    this._code.Add(BankType.SPDB, "SPDB");
                    //中信银行ECITIC
                    this._code.Add(BankType.CITIC, "ECITIC");
                    //杭州银行HZB
                    this._code.Add(BankType.HZCB, "HZB");
                    //广发银行GDB
                    this._code.Add(BankType.GDB, "GDB");
                }
                return _code;
            }
        }


        internal static class HttpHelp
        {

            //商户私钥签名
            public static string RSASign(string signStr, string privateKey, string halg = "md5", string type = "base64")
            {
                string sign = string.Empty;
                try
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(privateKey);
                    byte[] signBytes = rsa.SignData(UTF8Encoding.UTF8.GetBytes(signStr), halg);
                    switch (type)
                    {
                        case "hex":
                            sign = byte2HexString(signBytes);
                            break;
                        default:
                            sign = Convert.ToBase64String(signBytes);
                            break;
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                return sign;
            }

            /// <summary>
            /// 转16进制
            /// </summary>
            /// <param name="bytes"></param>
            /// <returns></returns>
            public static string byte2HexString(byte[] bytes)
            {
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                return sb.ToString();
            }

            public static byte[] hexString2byte(string str)
            {
                str = str.Replace(" ", "");
                byte[] buffer = new byte[str.Length / 2];
                for (int i = 0; i < str.Length; i += 2)
                {
                    buffer[i / 2] = (byte)Convert.ToByte(str.Substring(i, 2), 16);
                }
                return buffer;
            }

            //RSA私钥格式转换
            public static string RSAPrivateKeyJava2DotNet(string privateKey)
            {
                RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));
                return string.Format(
                    "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                    Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                    Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned())
                );
            }

            //使用智付公钥验签
            public static bool ValidateRsaSign(string plainText, string publicKey, string signedData, string flag = "md5", string convert = "base64")
            {
                try
                {
                    byte[] signData = null;
                    switch (convert)
                    {
                        case "hex":
                            signData = hexString2byte(signedData);
                            break;
                        default:
                            signData = Convert.FromBase64String(signedData);
                            break;
                    }

                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(publicKey);
                    return rsa.VerifyData(UTF8Encoding.UTF8.GetBytes(plainText), flag, signData);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            //智付公钥格式转换
            public static string RSAPublicKeyJava2DotNet(string publicKey)
            {
                RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
                return string.Format(
                    "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                    Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
                );
            }
        }
    }
}
