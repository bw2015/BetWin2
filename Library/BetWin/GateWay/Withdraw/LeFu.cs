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
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Json;
using BankType = BW.Common.Sites.BankType;

namespace BW.GateWay.Withdraw
{
    /// <summary>
    /// 乐付
    /// </summary>
    public class LeFu : IWithdraw
    {
        public LeFu() : base() { }

        public LeFu(string setting) : base(setting) { }

        private const string input_charset = "UTF-8";

        private const string sign_type = "SHA1WITHRSA";

        private const string service = "pay";

        [Description("商户号")]
        public string partner { get; set; }

        [Description("回调地址")]
        public string return_url { get; set; }

        [Description("私钥")]
        public string merPriKey { get; set; }

        [Description("公钥")]
        public string KBF_PUBLIC_KEY { get; set; }

        private string _gateway = "https://service.lepayle.com/api/quickdraw";
        [Description("代付网关")]
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

        private string _queryUrl = "https://service.lepayle.com/api/gateway";
        [Description("查询网关")]
        public string QueryURL
        {
            get
            {
                return this._queryUrl;
            }
            set
            {
                this._queryUrl = value;
            }
        }

        protected override Dictionary<BankType, string> InterfaceCode
        {
            get
            {
                Dictionary<BankType, string> _bankCode = new Dictionary<BankType, string>();
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
                return _bankCode;
            }
        }

        public override bool Remit(out string msg)
        {
            string bankCode = this.GetBankCode(this.BankCode);
            if (string.IsNullOrEmpty(bankCode))
            {
                msg = string.Format("系统不支持{0}", this.BankCode.GetDescription());
                return false;
            }

            SortedDictionary<string, string> contentData = new SortedDictionary<string, string>();
            contentData.Add("partner", this.partner);
            contentData.Add("service", service);
            contentData.Add("out_trade_no", this.OrderID);
            contentData.Add("amount_str", this.Money.ToString("0.00"));
            contentData.Add("bank_sn", bankCode);
            contentData.Add("bank_site_name", "广州市支行");
            contentData.Add("bank_account_name", this.Account);
            contentData.Add("bank_account_no", this.CardNo);
            contentData.Add("bus_type", "11");
            contentData.Add("bank_mobile_no", "1868888" + WebAgent.GetRandom(1000, 9999));
            contentData.Add("bank_province", "广东省");
            contentData.Add("bank_city", "广州市");
            contentData.Add("user_agreement", "");
            contentData.Add("remark", "");
            contentData.Add("return_url", this.return_url);
            string content = string.Join("&", contentData.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string contentSign = RSAFromPkcs8.RSAPublicKeyJava2DotNet(KBF_PUBLIC_KEY, content);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("partner", this.partner);
            dic.Add("input_charset", input_charset);
            dic.Add("sign_type", sign_type);
            dic.Add("request_time", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("content", contentSign);
            dic.Add("sign", RSAFromPkcs8.sign(content, merPriKey, input_charset));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
            string result = NetAgent.UploadData(this.Gateway, data, Encoding.UTF8);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("is_succ"))
            {
                msg = result;
                return false;
            }
            if (ht["is_succ"].ToString() == "F")
            {
                msg = ht["fault_reason"].ToString();
                return false;
            }
            if (ht["is_succ"].ToString() == "T")
            {
                msg = "提交成功";
                return true;
            }
            msg = result;
            return false;
        }

        public override void Remit(Action<bool, string> callback)
        {
            string msg;
            bool result = this.Remit(out msg);
            callback.Invoke(result, msg);
        }

        public override WithdrawStatus Query(string orderId, out string msg)
        {
            SortedDictionary<string, string> content = new SortedDictionary<string, string>();
            content.Add("service", "find_trade");
            content.Add("partner", this.partner);
            content.Add("out_trade_no", orderId);
            string contentStr = string.Join("&", content.Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("partner", this.partner);
            dic.Add("input_charset", input_charset);
            dic.Add("sign_type", sign_type);
            dic.Add("request_time", DateTime.Now.ToString("yyyyMMddHHmmss"));
            dic.Add("content", RSAFromPkcs8.RSAPublicKeyJava2DotNet(KBF_PUBLIC_KEY, contentStr));
            dic.Add("sign", RSAFromPkcs8.sign(contentStr, merPriKey, input_charset));

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, HttpUtility.UrlEncode(t.Value))));
            string result = NetAgent.UploadData(this.QueryURL, data, Encoding.UTF8);
            Hashtable ht = JsonAgent.GetJObject(result);
            if (ht == null || !ht.ContainsKey("is_succ"))
            {
                msg = result;
                return WithdrawStatus.Error;
            }
            if (ht["is_succ"].ToString() != "T")
            {
                msg = ht.ContainsKey("fault_reason") ? ht["fault_reason"].ToString() : result;
                return WithdrawStatus.Error;
            }
            string response = ht.ContainsKey("response") ? ht["response"].ToString() : string.Empty;
            if (string.IsNullOrEmpty(response))
            {
                msg = result;
                return WithdrawStatus.Error;
            }
            response = RSAFromPkcs8.decryptData(response, this.merPriKey, "utf-8");  //解析content值
            //{"trade_id":"TT2017052012186932","out_trade_no":"215237","amount_str":100.000000,"amount_fee":1.000000,"status":1,"for_trade_id":null,"business_type":0,"create_time":"2017-05-20 12:58:11","modified_time":"2017-05-20 13:01:34","remark":null}
            ht = JsonAgent.GetJObject(response);
            if (ht["business_type"].ToString() != "0")
            {
                msg = response;
                return WithdrawStatus.Error;
            }

            WithdrawStatus status = WithdrawStatus.Error;
            switch (ht["status"].ToString())
            {
                case "0":
                    msg = "处理中";
                    status = WithdrawStatus.Paymenting;
                    break;
                case "1":
                    msg = "已完成";
                    status = WithdrawStatus.Success;
                    break;
                case "2":
                    msg = "失败";
                    status = WithdrawStatus.Return;
                    break;
                default:
                    msg = string.Format("status:{0}", ht["status"]);
                    break;
            }
            return status;

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
