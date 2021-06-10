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
using System.Net;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

using BW.Common.Sites;
using SP.Studio.Array;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Json;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Withdraw
{
    public class JHZ : IWithdraw
    {
        public JHZ() { }

        public JHZ(string setting) : base(setting) { }

        private string _api_pay = "http://zf.szjhzxxkj.com/payment/api_pay_single";
        [Description("付款网关")]
        public string api_pay
        {
            get
            {
                return this._api_pay;
            }
            set
            {
                _api_pay = value;
            }
        }

        private string _api_query = "http://zf.szjhzxxkj.com/payment/api_pay_single_query";
        [Description("查询网关")]
        public string api_query
        {
            get
            {
                return this._api_query;
            }
            set
            {
                this._api_query = value;
            }
        }

        /// <summary>
        /// 版本号
        /// </summary>
        private const string version = "1.0";

        /// <summary>
        /// 金额类型
        /// </summary>
        private const string amount_type = "CNY";

        /// <summary>
        /// 账号类型	Y	21001 借记卡
        /// </summary>
        private const string acct_type = "21001";

        [Description("商户号")]
        public string merchant_no { get; set; }

        [Description("产品类型")]
        public string product_type { get; set; }

        /// <summary>
        /// RSA公钥
        /// </summary>
        [Description("公钥")]
        public string publicKey { get; set; }

        /// <summary>
        /// RSA私钥
        /// </summary>
        [Description("私钥")]
        public string privateKey { get; set; }


        [Description("MD5密钥")]
        public string MD5Key { get; set; }


        private Dictionary<BankType, string> _bank;
        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
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

        public override bool Remit(out string msg)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("version", version);
            dic.Add("merchant_no", this.merchant_no);
            dic.Add("request_no", this.OrderID);
            dic.Add("amount", ((int)(this.Money * 100)).ToString());
            dic.Add("amount_type", amount_type);
            dic.Add("product_type", this.product_type);
            dic.Add("acct_name", this.Account);
            dic.Add("acct_id", this.CardNo);
            dic.Add("acct_type", acct_type);
            dic.Add("mobile", "138" + WebAgent.GetRandom(0, 99999999).ToString().PadLeft(8, '0'));
            dic.Add("memo", new string[] { "domain", "host", "sales" }.GetRandom());
            //待签名串&sign=rsa(待签名串&sign=md5（待签名串&key=md5秘钥）)
            string content = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string md5 = SP.Studio.Security.MD5.Encryp(content + "&key=" + this.MD5Key).ToLower();
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&sign=" + HttpUtility.UrlEncode(this.privateSign(content + "&sign=" + md5));
            msg = NetAgent.UploadData(api_pay, data, Encoding.UTF8);

            if (!msg.Contains("12000"))
            {
                return false;
            }
            return true;
        }

        public override void Remit(Action<bool, string> callback)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("version", version);
            dic.Add("merchant_no", this.merchant_no);
            dic.Add("request_no", this.OrderID);
            dic.Add("amount", ((int)(this.Money * 100)).ToString());
            dic.Add("amount_type", amount_type);
            dic.Add("product_type", this.product_type);
            dic.Add("acct_name", this.Account);
            dic.Add("acct_id", this.CardNo);
            dic.Add("acct_type", acct_type);
            dic.Add("mobile", "138" + WebAgent.GetRandom(0, 99999999).ToString().PadLeft(8, '0'));
            dic.Add("memo", new string[] { "domain", "host", "sales" }.GetRandom());
            //待签名串&sign=rsa(待签名串&sign=md5（待签名串&key=md5秘钥）)
            string content = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string md5 = SP.Studio.Security.MD5.Encryp(content + "&key=" + this.MD5Key).ToLower();
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&sign=" + HttpUtility.UrlEncode(this.privateSign(content + "&sign=" + md5));

            UploadDataCompletedEventHandler handler = (object sender, UploadDataCompletedEventArgs e) =>
            {
                string msg = Encoding.UTF8.GetString((byte[])e.Result);
                callback.Invoke(msg.Contains("success"), msg);
            };

            NetAgent.UploadDataSync(api_pay, data, handler, Encoding.UTF8);
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            SortedDictionary<string, string> dic = new SortedDictionary<string, string>();
            dic.Add("version", version);
            dic.Add("merchant_no", this.merchant_no);
            dic.Add("sp_request_no", orderId);
            dic.Add("sp_reqtime", DateTime.Now.ToString("yyyyMMddHHmmss"));

            string content = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string md5 = SP.Studio.Security.MD5.Encryp(content + "&key=" + this.MD5Key).ToLower();
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&sign=" + HttpUtility.UrlEncode(this.privateSign(content + "&sign=" + md5));
            msg = NetAgent.UploadData(api_query, data, Encoding.UTF8);
            if (!msg.Contains("12000"))
            {
                return WithdrawStatus.Error;
            }

            Hashtable ht = JsonAgent.GetJObject(msg);
            if (ht == null || !ht.ContainsKey("cipher_data")) return WithdrawStatus.Error;
            string cipher_data = HttpUtility.UrlDecode(ht["cipher_data"].ToString());
            byte[] source = Convert.FromBase64String(cipher_data);
            List<byte[]> list = new List<byte[]>();

            WithdrawStatus status = WithdrawStatus.Error;
            try
            {
                int page = 128;
                int index = 0;
                while (index < (source.Length % page == 0 ? source.Length / page : source.Length / page + 1))
                {
                    list.Add(source.Skip(index * page).Take(page).ToArray());
                    index++;
                }
                StringBuilder sb = new StringBuilder();
                list.ForEach(t =>
                {
                    string decrypt = this.RSADecrypt(t);
                    sb.Append(decrypt);
                });

                ht = JsonAgent.GetJObject(sb.ToString());
                if (ht != null && ht.ContainsKey("status"))
                {
                    msg = ht["status"].ToString();
                    switch (msg)
                    {
                        case "1":
                            status = WithdrawStatus.Paymenting;
                            break;
                        case "2":
                            status = WithdrawStatus.Success;
                            break;
                        case "3":
                        case "4":
                            status = WithdrawStatus.Return;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message + "\n" + cipher_data;
            }
            return status;
        }

        #region ========= 工具类  ===========

        /// <summary>
        /// 私钥转换
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        private string RSAPrivateKeyJava2DotNet()
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(this.privateKey));

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
        /// 公式转换
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        private string RSAPublicKeyJava2DotNet()
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(this.publicKey));
            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        /// <summary>
        /// 使用私钥签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string privateSign(string data)
        {
            string key = this.RSAPrivateKeyJava2DotNet();
            RSACryptoServiceProvider rsaCsp = new RSACryptoServiceProvider();
            rsaCsp.FromXmlString(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = rsaCsp.SignData(dataBytes, "SHA1");
            return Convert.ToBase64String(signatureBytes);
        }

        /// <summary>
        /// 使用私钥进行解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string RSADecrypt(byte[] rgb)
        {
            string str2;
            try
            {
                RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
                provider.FromXmlString(this.RSAPrivateKeyJava2DotNet());
                byte[] buffer2 = provider.Decrypt(rgb, false);
                str2 = Encoding.UTF8.GetString(buffer2);
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return str2;
        }
        #endregion
    }
}
