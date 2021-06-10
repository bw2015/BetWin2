using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Web;
using System.IO;
using System.Security.Cryptography;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;

using BW.Common.Sites;

using SP.Studio.Web;
using SP.Studio.Security;
using SP.Studio.Json;

using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Payment
{
    public class LeFu : IPayment
    {
        public LeFu() : base() { }

        public LeFu(string setting) : base(setting) { }

        private const string input_charset = "UTF-8";

        [Description("商户号")]
        public string partner { get; set; }

        /// <summary>
        /// gateway_pay ali_pay wx_pay
        /// </summary>
        [Description("网关类型")]
        public string service { get; set; }

        private string _return_url = "/handler/payment/LeFu";
        [Description("后台通知地址")]
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


        private string _redirect_url = "/handler/payment/LeFu";
        [Description("前台通知地址")]
        public string redirect_url
        {
            get
            {
                return this._redirect_url;
            }
            set
            {
                this._redirect_url = value;
            }
        }

        [Description("MD5 Key")]
        public string verfication_code { get; set; }

        [Description("私钥")]
        public string merPriKey { get; set; }

        [Description("公钥")]
        public string KBF_PUBLIC_KEY { get; set; }

        private string _payGateway = "http://service.lepayle.com/gateway/pay";
        [Description("支付网关")]
        public string PayGateway
        {
            get
            {
                return this._payGateway;
            }
            set
            {
                this._payGateway = value;
            }
        }

        public override string ShowCallback()
        {
            return "success";
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string param;
            switch (this.service)
            {
                case "gateway_pay":
                    dic.Add("partner", this.partner);
                    dic.Add("service", this.service);
                    dic.Add("out_trade_no", this.OrderID);
                    dic.Add("amount_str", this.Money.ToString("0.00"));
                    dic.Add("tran_ip", IPAgent.IP);
                    dic.Add("buyer_name", this.Name);
                    dic.Add("buyer_contact", this.Name);
                    dic.Add("good_name", this.Name);
                    dic.Add("request_time", DateTime.Now.ToString("yyyyMMddHHmmss"));
                    dic.Add("return_url", this.GetUrl(this.return_url));
                    param = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value))) + "&verfication_code=" + this.verfication_code;
                    dic.Add("input_charset", "UTF-8");
                    dic.Add("sign", SP.Studio.Security.MD5.toMD5(param).ToLower());
                    dic.Add("sign_type", "MD5");
                    dic.Add("bank_code", this.BankValue);
                    this.BuildForm(dic, this.PayGateway);
                    break;
                case "ali_pay":
                    string sign_type = "SHA1WITHRSA";
                    dic.Add("amount_str", this.Money.ToString("0.00"));
                    dic.Add("out_trade_no", this.OrderID);
                    dic.Add("partner", this.partner);
                    dic.Add("remark", this.Name);
                    dic.Add("service", this.service);
                    dic.Add("sub_body", this.Name);
                    dic.Add("subject", this.Name);
                    dic.Add("ali_pay_type", "ali_sm");
                    dic.Add("return_url", this.GetUrl(this.return_url));
                    param = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
                    string sign = RSAFromPkcs8.sign(param, merPriKey, input_charset);
                    string content = RSAFromPkcs8.RSAPublicKeyJava2DotNet(KBF_PUBLIC_KEY, param);
                    dic.Clear();
                    dic.Add("content", content);
                    dic.Add("input_charset", input_charset);
                    dic.Add("partner", partner);
                    dic.Add("sign", sign);
                    dic.Add("sign_type", sign_type);
                    //string data = "content=" + content + "&input_charset=" + input_charset + "&partner=" + partner + "&sign=" + sign + "&sign_type=" + sign_type;

                    this.BuildForm(dic, this.PayGateway);
                    break;
            }

        }

        public override bool Verify(VerifyCallBack callback)
        {
            if (WebAgent.GetParam("status", 0) != 1) return false;
            string content = HttpUtility.UrlDecode(WebAgent.GetParam("content"));
            string sign = HttpUtility.UrlDecode(WebAgent.GetParam("sign"));
            string param_ming = RSAFromPkcs8.decryptData(content, merPriKey, "utf-8");  //解析content值
            bool ck_sign = RSAFromPkcs8.verify(param_ming, sign, KBF_PUBLIC_KEY, "utf-8");
            if (ck_sign)
            {
                callback.Invoke();
                return true;
            }
            return false;
        }

        public override string GetTradeNo(out decimal money, out string systemId)
        {
            money = decimal.Zero;
            systemId = string.Empty;

            long orderId = WebAgent.GetParam("out_trade_no", (long)0);
            var order = BW.Agent.UserAgent.Instance().GetRechargeOrderInfo(orderId);
            if (order == null) return null;
            PaymentSetting setting = BW.Agent.SiteAgent.Instance().GetPaymentSettingInfo(order.PayID);
            string content = HttpUtility.UrlDecode(WebAgent.GetParam("content"));
            string param_ming = RSAFromPkcs8.decryptData(content, ((LeFu)setting.PaymentObject).merPriKey, "utf-8");  //解析content值

            Hashtable ht = JsonAgent.GetJObject(param_ming);
            systemId = ht["trade_id"].ToString();
            money = decimal.Parse(ht["amount_str"].ToString());
            return ht["out_trade_no"].ToString();
        }

        private Dictionary<BankType, string> _bankCode;
        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                if (this.service != "gateway_pay") return null;
                if (_bankCode == null)
                {
                    _bankCode = new Dictionary<BankType, string>();
                    _bankCode.Add(BankType.ICBC, "ICBC");
                    _bankCode.Add(BankType.ABC, "ABC");
                    _bankCode.Add(BankType.CCB, "CCB");
                    _bankCode.Add(BankType.COMM, "BOCM");
                    _bankCode.Add(BankType.BOC, "BOC");
                    _bankCode.Add(BankType.CMB, "CMB");
                    _bankCode.Add(BankType.PSBC, "PSBC");
                    _bankCode.Add(BankType.HXBANK, "HXB");
                    _bankCode.Add(BankType.CIB, "CIB");
                    _bankCode.Add(BankType.GDB, "CGB");
                    _bankCode.Add(BankType.CITIC, "CITIC");
                }
                return this._bankCode;
            }
        }


        private sealed class RSAFromPkcs8
        {
            public static string RSAPublicKeyJava2DotNet(string javaPublicString, string data)
            {
                byte[] publicInfoByte = Convert.FromBase64String(javaPublicString);

                Asn1Object pubKeyObj = Asn1Object.FromByteArray(publicInfoByte);
                AsymmetricKeyParameter pubKey = PublicKeyFactory.CreateKey(publicInfoByte);
                BufferedAsymmetricBlockCipher cipher = (BufferedAsymmetricBlockCipher)CipherUtilities.GetCipher("RSA/NONE/PKCS1PADDING");
                // /NONE/PKCS1PADDING
                cipher.Init(true, pubKey);

                byte[] textBytes = Encoding.UTF8.GetBytes(data);
                List<byte> lst = textBytes.ToList();
                byte[] dataReturn = null;

                for (int i = 0; i < lst.Count; i += 100)
                {
                    byte[] enData = cipher.DoFinal(Encoding.UTF8.GetBytes(data), i, (i + 100 > lst.Count ? lst.Count - i : 100));
                    if (dataReturn != null)
                    {
                        dataReturn = dataReturn.Concat(enData).ToArray();
                    }
                    else
                    {
                        dataReturn = enData;
                    }
                }

                return Convert.ToBase64String(dataReturn);
            }

            /// <summary>
            /// 签名
            /// </summary>
            /// <param name="content">待签名字符串</param>
            /// <param name="privateKey">私钥</param>
            /// <param name="input_charset">编码格式</param>
            /// <returns>签名后字符串</returns>
            public static string sign(string content, string privateKey, string input_charset)
            {
                byte[] Data = Encoding.GetEncoding(input_charset).GetBytes(content);
                RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
                SHA1 sh = new SHA1CryptoServiceProvider();
                byte[] signData = rsa.SignData(Data, sh);
                return Convert.ToBase64String(signData);
            }

            /// <summary>
            /// 验签
            /// </summary>
            /// <param name="content">待验签字符串</param>
            /// <param name="signedString">签名</param>
            /// <param name="publicKey">公钥</param>
            /// <param name="input_charset">编码格式</param>
            /// <returns>true(通过)，false(不通过)</returns>
            public static bool verify(string content, string signedString, string publicKey, string input_charset)
            {
                bool result = false;
                byte[] Data = Encoding.GetEncoding(input_charset).GetBytes(content);
                byte[] data = Convert.FromBase64String(signedString);
                RSAParameters paraPub = ConvertFromPublicKey(publicKey);
                RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
                rsaPub.ImportParameters(paraPub);
                SHA1 sh = new SHA1CryptoServiceProvider();
                result = rsaPub.VerifyData(Data, sh, data);
                return result;
            }

            /// <summary>
            /// 加密
            /// </summary>
            /// <param name="resData">需要加密的字符串</param>
            /// <param name="publicKey">公钥</param>
            /// <param name="input_charset">编码格式</param>
            /// <returns>明文</returns>
            public static string encryptData(string resData, string publicKey, string input_charset)
            {
                byte[] DataToEncrypt = Encoding.ASCII.GetBytes(resData);
                string result = encrypt(DataToEncrypt, publicKey, input_charset);
                return result;
            }


            /// <summary>
            /// 解密
            /// </summary>
            /// <param name="resData">加密字符串</param>
            /// <param name="privateKey">私钥</param>
            /// <param name="input_charset">编码格式</param>
            /// <returns>明文</returns>
            public static string decryptData(string resData, string privateKey, string input_charset)
            {
                byte[] DataToDecrypt = Convert.FromBase64String(resData);
                string result = "";
                for (int j = 0; j < DataToDecrypt.Length / 128; j++)
                {
                    byte[] buf = new byte[128];
                    for (int i = 0; i < 128; i++)
                    {

                        buf[i] = DataToDecrypt[i + 128 * j];
                    }
                    result += decrypt(buf, privateKey, input_charset);
                }
                return result;
            }

            #region 内部方法

            private static string encrypt(byte[] data, string publicKey, string input_charset)
            {
                RSACryptoServiceProvider rsa = DecodePemPublicKey(publicKey);
                SHA1 sh = new SHA1CryptoServiceProvider();
                byte[] result = rsa.Encrypt(data, false);

                return Convert.ToBase64String(result);
            }

            private static string decrypt(byte[] data, string privateKey, string input_charset)
            {
                string result = "";
                RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
                SHA1 sh = new SHA1CryptoServiceProvider();
                byte[] source = rsa.Decrypt(data, false);
                char[] asciiChars = new char[Encoding.GetEncoding(input_charset).GetCharCount(source, 0, source.Length)];
                Encoding.GetEncoding(input_charset).GetChars(source, 0, source.Length, asciiChars, 0);
                result = new string(asciiChars);
                //result = ASCIIEncoding.ASCII.GetString(source);
                return result;
            }

            private static RSACryptoServiceProvider DecodePemPublicKey(String pemstr)
            {
                byte[] pkcs8publickkey;
                pkcs8publickkey = Convert.FromBase64String(pemstr);
                if (pkcs8publickkey != null)
                {
                    RSACryptoServiceProvider rsa = DecodeRSAPublicKey(pkcs8publickkey);
                    return rsa;
                }
                else
                    return null;
            }

            private static RSACryptoServiceProvider DecodePemPrivateKey(String pemstr)
            {
                byte[] pkcs8privatekey;
                pkcs8privatekey = Convert.FromBase64String(pemstr);
                if (pkcs8privatekey != null)
                {
                    RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                    return rsa;
                }
                else
                    return null;
            }

            private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
            {
                byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
                byte[] seq = new byte[15];

                MemoryStream mem = new MemoryStream(pkcs8);
                int lenstream = (int)mem.Length;
                BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;

                try
                {
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();    //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x02)
                        return null;

                    twobytes = binr.ReadUInt16();

                    if (twobytes != 0x0001)
                        return null;

                    seq = binr.ReadBytes(15);        //read the Sequence OID
                    if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x04)    //expect an Octet string
                        return null;

                    bt = binr.ReadByte();        //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count
                    if (bt == 0x81)
                        binr.ReadByte();
                    else
                        if (bt == 0x82)
                            binr.ReadUInt16();
                    //------ at this stage, the remaining sequence should be the RSA private key

                    byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                    RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                    return rsacsp;
                }

                catch (Exception)
                {
                    return null;
                }

                finally { binr.Close(); }

            }

            private static bool CompareBytearrays(byte[] a, byte[] b)
            {
                if (a.Length != b.Length)
                    return false;
                int i = 0;
                foreach (byte c in a)
                {
                    if (c != b[i])
                        return false;
                    i++;
                }
                return true;
            }

            private static RSACryptoServiceProvider DecodeRSAPublicKey(byte[] publickey)
            {
                // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
                byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
                byte[] seq = new byte[15];
                // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
                MemoryStream mem = new MemoryStream(publickey);
                BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;

                try
                {

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    seq = binr.ReadBytes(15);       //read the Sequence OID
                    if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8203)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    bt = binr.ReadByte();
                    if (bt != 0x00)     //expect null byte next
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();   //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                        lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                        return null;
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    byte firstbyte = binr.ReadByte();
                    binr.BaseStream.Seek(-1, SeekOrigin.Current);

                    if (firstbyte == 0x00)
                    {   //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte();    //skip this null byte
                        modsize -= 1;   //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                    if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                        return null;
                    int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSAParameters RSAKeyInfo = new RSAParameters();
                    RSAKeyInfo.Modulus = modulus;
                    RSAKeyInfo.Exponent = exponent;
                    RSA.ImportParameters(RSAKeyInfo);
                    return RSA;
                }
                catch (Exception)
                {
                    return null;
                }

                finally { binr.Close(); }

            }

            private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
            {
                byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

                // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------
                MemoryStream mem = new MemoryStream(privkey);
                BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;
                int elems = 0;
                try
                {
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();    //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102)    //version number
                        return null;
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;


                    //------  all private key components are Integer sequences ----
                    elems = GetIntegerSize(binr);
                    MODULUS = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    E = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    D = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    P = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    Q = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DP = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    DQ = binr.ReadBytes(elems);

                    elems = GetIntegerSize(binr);
                    IQ = binr.ReadBytes(elems);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSAParameters RSAparams = new RSAParameters();
                    RSAparams.Modulus = MODULUS;
                    RSAparams.Exponent = E;
                    RSAparams.D = D;
                    RSAparams.P = P;
                    RSAparams.Q = Q;
                    RSAparams.DP = DP;
                    RSAparams.DQ = DQ;
                    RSAparams.InverseQ = IQ;
                    RSA.ImportParameters(RSAparams);
                    return RSA;
                }
                catch (Exception)
                {
                    return null;
                }
                finally { binr.Close(); }
            }

            private static int GetIntegerSize(BinaryReader binr)
            {
                byte bt = 0;
                byte lowbyte = 0x00;
                byte highbyte = 0x00;
                int count = 0;
                bt = binr.ReadByte();
                if (bt != 0x02)        //expect integer
                    return 0;
                bt = binr.ReadByte();

                if (bt == 0x81)
                    count = binr.ReadByte();    // data size in next byte
                else
                    if (bt == 0x82)
                    {
                        highbyte = binr.ReadByte();    // data size in next 2 bytes
                        lowbyte = binr.ReadByte();
                        byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                        count = BitConverter.ToInt32(modint, 0);
                    }
                    else
                    {
                        count = bt;        // we already have the data size
                    }



                while (binr.ReadByte() == 0x00)
                {    //remove high order zeros in data
                    count -= 1;
                }
                binr.BaseStream.Seek(-1, SeekOrigin.Current);        //last ReadByte wasn't a removed zero, so back up a byte
                return count;
            }

            #endregion

            #region 解析.net 生成的Pem
            private static RSAParameters ConvertFromPublicKey(string pemFileConent)
            {

                byte[] keyData = Convert.FromBase64String(pemFileConent);
                if (keyData.Length < 162)
                {
                    throw new ArgumentException("pem file content is incorrect.");
                }
                byte[] pemModulus = new byte[128];
                byte[] pemPublicExponent = new byte[3];
                Array.Copy(keyData, 29, pemModulus, 0, 128);
                Array.Copy(keyData, 159, pemPublicExponent, 0, 3);
                RSAParameters para = new RSAParameters();
                para.Modulus = pemModulus;
                para.Exponent = pemPublicExponent;
                return para;
            }

            private static RSAParameters ConvertFromPrivateKey(string pemFileConent)
            {
                byte[] keyData = Convert.FromBase64String(pemFileConent);
                if (keyData.Length < 609)
                {
                    throw new ArgumentException("pem file content is incorrect.");
                }

                int index = 11;
                byte[] pemModulus = new byte[128];
                Array.Copy(keyData, index, pemModulus, 0, 128);

                index += 128;
                index += 2;//141
                byte[] pemPublicExponent = new byte[3];
                Array.Copy(keyData, index, pemPublicExponent, 0, 3);

                index += 3;
                index += 4;//148
                byte[] pemPrivateExponent = new byte[128];
                Array.Copy(keyData, index, pemPrivateExponent, 0, 128);

                index += 128;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//279
                byte[] pemPrime1 = new byte[64];
                Array.Copy(keyData, index, pemPrime1, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//346
                byte[] pemPrime2 = new byte[64];
                Array.Copy(keyData, index, pemPrime2, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//412/413
                byte[] pemExponent1 = new byte[64];
                Array.Copy(keyData, index, pemExponent1, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//479/480
                byte[] pemExponent2 = new byte[64];
                Array.Copy(keyData, index, pemExponent2, 0, 64);

                index += 64;
                index += ((int)keyData[index + 1] == 64 ? 2 : 3);//545/546
                byte[] pemCoefficient = new byte[64];
                Array.Copy(keyData, index, pemCoefficient, 0, 64);

                RSAParameters para = new RSAParameters();
                para.Modulus = pemModulus;
                para.Exponent = pemPublicExponent;
                para.D = pemPrivateExponent;
                para.P = pemPrime1;
                para.Q = pemPrime2;
                para.DP = pemExponent1;
                para.DQ = pemExponent2;
                para.InverseQ = pemCoefficient;
                return para;
            }
            #endregion

        }
    }
}
