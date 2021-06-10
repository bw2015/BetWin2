using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;

using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Core;
using SP.Studio.Json;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 和支付
    /// </summary>
    public class HePay : IPayment
    {
        public HePay() : base() { }

        public HePay(string setting) : base(setting) { }

        [Description("商户ID")]
        public string seller_id { get; set; }


        [Description("订单类型 微信二维码:2701;支付宝二维码:2702;银联快捷:2703;网银网关:2704;QQ钱包:2705;微信H5:2706;QQH5:2707;支付宝H5:2708;京东钱包:2709;百度钱包:2710")]
        public string order_type { get; set; }

        private string _notify_url = "/handler/payment/HePay";
        [Description("回调地址")]
        public string notify_url { get { return this._notify_url; } set { this._notify_url = value; } }

        private string _return_urll = "/handler/payment/HePay";
        [Description("回跳地址")]
        public string return_url { get { return this._return_urll; } set { this._return_urll = value; } }

        [Description("平台公钥")]
        public string web_public_key { get; set; }

        [Description("商户私钥")]
        public string merchant_private_key { get; set; }

        private string _gateway = "http://api.xueyuplus.com/wbsp/unifiedorder";
        [Description("网关地址")]
        public string Gateway { get { return this._gateway; } set { this._gateway = value; } }

        public override string ShowCallback()
        {
            return "success";
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            //{"opay_status":0,"order_type":"2703","out_trade_no":"2017112xxxxxxxxxxx8364","pay_status":1,"seller_id":"125020","sign":"ce229622f6a9b8c71xxxxxee002091441","total_fee":2000000}
            string json = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            money = JsonAgent.GetValue<int>(json, "total_fee") / 100M;
            systemId = JsonAgent.GetValue<string>(json, "out_trade_no");
            return systemId;
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("seller_id", this.seller_id);
            dic.Add("order_type", this.order_type);
            dic.Add("pay_body", this.Name);
            dic.Add("out_trade_no", this.OrderID);
            dic.Add("total_fee", ((int)this.Money * 100).ToString());
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("return_url", this.GetUrl(this.return_url));
            dic.Add("spbill_create_ip", IPAgent.IP);
            dic.Add("spbill_times", WebAgent.GetTimeStamp().ToString());
            dic.Add("noncestr", Guid.NewGuid().ToString("N").Substring(0, 32));
            dic.Add("remark", "PAY");
            string signStr = string.Join("&", dic.OrderBy(t => t.Key).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string sign = HttpHelp.RSASign(signStr, HttpHelp.RSAPrivateKeyJava2DotNet(this.merchant_private_key));
            dic.Add("sign", sign);

            string json = dic.ToJson();
            string result = NetAgent.UploadData(this.Gateway, Encoding.UTF8, new Dictionary<string, string>(), Encoding.UTF8.GetBytes(json));

            int state = JsonAgent.GetValue<int>(result, "state");
            string msg = JsonAgent.GetValue<string>(result, "return_msg");
            if (state != 0)
            {
                context.Response.Write(msg);
                return;
            }
            string url = JsonAgent.GetValue<string>(result, "pay_url");
            this.BuildForm(url, "GET");
        }

        public override bool Verify(VerifyCallBack callback)
        {
            string json = Encoding.UTF8.GetString(WebAgent.GetInputSteam(this.context));
            Hashtable ht = JsonAgent.GetJObject(json);
            if (ht == null || ht["pay_status"].ToString() != "1") return false;
            //{"opay_status":0,"order_type":"2703","out_trade_no":"2017112xxxxxxxxxxx8364","pay_status":1,"seller_id":"125020","sign":"ce229622f6a9b8c71xxxxxee002091441","total_fee":2000000}
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (DictionaryEntry de in ht)
            {
                if (de.Key.ToString() == "sign") continue;
                dic.Add(de.Key.ToString(), de.Value.ToString());
            }
            string signStr = string.Join("&", dic.Where(t => !string.IsNullOrEmpty(t.Value)).Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string key = HttpHelp.RSAPublicKeyJava2DotNet(this.web_public_key);
            bool result = HttpHelp.ValidateRsaSign(signStr, key, ht["sign"].ToString());
            if (result)
            {
                callback.Invoke();
                return true;
            }
            return false;

        }


        private static class HttpHelp
        {

            //商户私钥签名
            public static string RSASign(string signStr, string privateKey)
            {
                try
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(privateKey);
                    byte[] signBytes = rsa.SignData(UTF8Encoding.UTF8.GetBytes(signStr), "md5");
                    return Convert.ToBase64String(signBytes);
                }
                catch (Exception e)
                {
                    throw e;
                }
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
            public static bool ValidateRsaSign(string plainText, string publicKey, string signedData)
            {
                try
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    rsa.FromXmlString(publicKey);
                    return rsa.VerifyData(UTF8Encoding.UTF8.GetBytes(plainText), "md5", Convert.FromBase64String(signedData));
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
