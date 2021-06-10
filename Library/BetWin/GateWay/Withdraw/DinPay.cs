using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System.ComponentModel;
using System.Web;
using System.Xml.Linq;

using SP.Studio.Array;
using BW.Common.Sites;
using SP.Studio.Web;
using SP.Studio.Net;
using SP.Studio.Xml;
using SP.Studio.Text;
using SP.Studio.Model;

using BankType = BW.Common.Sites.BankType;
namespace BW.GateWay.Withdraw
{
    public sealed class DinPay : IWithdraw
    {
        public DinPay() : base() { }

        public DinPay(string setting) : base(setting) { }

        private const string interface_version = "V3.1.0";

        /// <summary>
        /// 请求码
        /// </summary>
        private const string tran_code = "DMTI";

        /// <summary>
        /// 省份代码
        /// </summary>
        private const string recv_province = "44";

        /// <summary>
        /// 城市代码
        /// </summary>
        private const string recv_city = "5810";

        /// <summary>
        /// 签名方式
        /// </summary>
        private const string sign_type = "RSA-S";

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

        [Description("商户号")]
        public string merchant_no { get; set; }

        [Description("余额扣除方式")]
        public int tran_fee_type { get; set; }

        [Description("转账方式")]
        public int tran_type { get; set; }

        private string _gateway = "https://transfer.dinpay.com/transfer";
        [Description("网关地址")]
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

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.COMM, "BCOM");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.CMB, "CMB");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.CEB, "CEBB");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.PSBC, "PSBC");
                dic.Add(BankType.SPABANK, "SPABANK");
                dic.Add(BankType.CITIC, "ECITIC");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.HKBEA, "BEA");
                dic.Add(BankType.BJBANK, "BOB");
                dic.Add(BankType.SHBANK, "SHB");
                dic.Add(BankType.NBBANK, "NBB");
                dic.Add(BankType.BHB, "BHB");
                dic.Add(BankType.HSBANK, "HSBANK");

                return dic;
            }
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("interface_version", interface_version);
            dic.Add("mer_transfer_no", orderId);
            dic.Add("merchant_no", this.merchant_no);
            dic.Add("tran_code", "DMTQ");
            dic.Add("sign_type", "RSA-S");
            dic.Add("sign_info", HttpUtility.UrlEncode(this.GetSign(dic)));
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            string result = NetAgent.UploadData(this.GateWay, data, Encoding.UTF8);
            WithdrawStatus status = WithdrawStatus.Error;
            try
            {
                XElement root = XElement.Parse(result);
                string result_code = root.GetValue("result_code");
                string recv_code = root.GetValue("recv_code");

                msg = this.getResultCode(recv_code);
                if (string.IsNullOrEmpty(msg)) msg = result;
                switch (result_code)
                {
                    case "0":
                    case "1":
                        switch (recv_code)
                        {
                            case "0000":
                                status = WithdrawStatus.Success;
                                break;
                            case "0001":
                                status = WithdrawStatus.Paymenting;
                                break;
                            case "0002":
                            case "0003":
                            case "1003":
                            case "1004":
                            case "1007":
                            case "1008":
                            case "1009":
                                status = WithdrawStatus.Return;
                                break;
                        }
                        break;
                    case "2":
                        status = WithdrawStatus.Return;
                        break;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message + " " + result;
                status = WithdrawStatus.Error;
            }
            return status;
        }

        public override bool Remit(out string msg)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("interface_version", interface_version);
            dic.Add("mer_transfer_no", this.OrderID);
            dic.Add("merchant_no", this.merchant_no);
            dic.Add("tran_code", tran_code);
            dic.Add("recv_bank_code", this.GetBankCode(this.BankCode));
            dic.Add("recv_accno", this.CardNo);
            dic.Add("recv_name", this.Account);
            dic.Add("recv_province", recv_province);
            dic.Add("recv_city", recv_city);
            dic.Add("tran_amount", this.Money.ToString("0.00"));
            dic.Add("tran_fee_type", this.tran_fee_type.ToString());
            dic.Add("tran_type", this.tran_type.ToString());
            dic.Add("sign_type", sign_type);
            dic.Add("sign_info", HttpUtility.UrlEncode(this.GetSign(dic)));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = NetAgent.UploadData(this.GateWay, data, Encoding.UTF8);
            try
            {
                XElement root = XElement.Parse(result);
                string result_code = root.GetValue("result_code");
                string recv_code = root.GetValue("recv_code");
                msg = this.getResultCode(recv_code) ?? result;
                return result_code == "0";
            }
            catch
            {
                msg = result;
                return false;
            }
        }

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private string GetSign(Dictionary<string, string> dic)
        {
            string[] field = new string[] { "interface_version", "mer_transfer_no", "merchant_no", "recv_accno", "recv_bank_code", "recv_city", "recv_name", "recv_province", "remark", "tran_amount",
            "tran_code","tran_fee_type","tran_type"};
            string signStr = string.Join("&", field.Where(t => !string.IsNullOrEmpty(dic.Get(t, string.Empty))).Select(t => string.Format("{0}={1}", t, dic[t])));
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

        private string getResultCode(string recv_code)
        {
            string msg = null;
            switch (recv_code)
            {
                case "0000":
                    msg = "转账成功";
                    break;
                case "0001":
                    msg = "转账处理中";
                    break;
                case "0002":
                    msg = "转账失败";
                    break;
                case "0003":
                    msg = "转账取消";
                    break;
                case "0004":
                    msg = "余额查询成功";
                    break;
                case "1003":
                    msg = "查无此交易";
                    break;
                case "1004":
                    msg = "转账超限";
                    break;
                case "1005":
                    msg = "非法参数";
                    break;
                case "1006":
                    msg = "验签失败";
                    break;
                case "1007":
                    msg = "商家无直联转账权限";
                    break;
                case "1008":
                    msg = "账户余额不足";
                    break;
                case "1009":
                    msg = "商家账户不存在";
                    break;
                case "2000":
                    msg = "系统异常";
                    break;
                case "9000":
                    msg = "未知错误";
                    break;
                default:
                    msg = null;
                    break;
            }
            return msg;
        }

        public override void Remit(Action<bool, string> callback)
        {
            throw new NotImplementedException();
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
