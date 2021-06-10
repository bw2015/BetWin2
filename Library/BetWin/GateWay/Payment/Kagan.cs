using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Security.Cryptography;
using System.IO;

using SP.Studio.Array;
using SP.Studio.Security;

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using BW.Common.Sites;

namespace BW.GateWay.Payment
{
    /// <summary>
    /// 桃宝支付
    /// </summary>
    public class TAOBAO : IPayment
    {
        public TAOBAO() : base() { }

        public TAOBAO(string setting) : base(setting) { }

        private const string currency = "CNY";

        private const string user_type = "1";

        [Description("MD5密钥")]
        public string Key { get; set; }

        [Description("公匙")]
        public string PublicKey { get; set; }

        [Description("私匙")]
        public string PrivateKey { get; set; }

        private string _version = "v1.0";
        [Description("版本号")]
        public string version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value;
            }
        }

        [Description("商户号")]
        public string userid { get; set; }

        private string _card_type = "1";
        [Description("银行卡类型")]
        public string card_type
        {
            get
            {
                return this._card_type;
            }
            set
            {
                this._card_type = value;
            }
        }

        private string _return_url = "/handler/payment/TAOBAO";
        [Description("页面回调")]
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


        private string _notify_url = "/handler/payment/TAOBAO";
        [Description("后台回调")]
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

        private string _gateway = "http://kagan.top:30008/CFAPI/pay_online";
        [Description("网关")]
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
            throw new NotImplementedException();
        }

        public override void GoGateway()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("version", this.version);
            dic.Add("userid", this.userid);
            dic.Add("orderSerialNum", this.OrderID);
            dic.Add("amount", ((int)(this.Money * 100M)).ToString());
            dic.Add("currency", currency);
            dic.Add("card_type", this.card_type);
            dic.Add("bank_segment", this.BankValue);
            dic.Add("user_type", user_type);
            dic.Add("buyerName", this.Name);
            dic.Add("buyerId", this.Name);
            dic.Add("contact", "13800138000");
            dic.Add("return_url", this.GetUrl(this.return_url));
            dic.Add("notify_url", this.GetUrl(this.notify_url));
            dic.Add("info", "BUY");

            string data = dic.OrderBy(t => t.Key).ToQueryString();
            string sign = SP.Studio.Security.MD5.toMD5(data + "&key=" + this.Key).ToLower();
            dic.Add("sign", sign);
            data = dic.ToQueryString();
            dic.Clear();
            dic.Add("singStr", data);
            dic.Add("data", this.Sign(data));

            this.BuildForm(dic, this.Gateway);
        }

        public override bool Verify(VerifyCallBack callback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 使用公钥进行加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string Sign(string data)
        {
            return HttpHelp.RSAEncrypt(data, DinPay.HttpHelp.RSAPublicKeyJava2DotNet(this.PublicKey));
            //string publicKey = string.Format("-----BEGIN PUBLIC KEY-----\n{0}\n-----END PUBLIC KEY-----", this.PublicKey);
            //RSACryptoServiceProvider _publicKeyRsaProvider = CreateRsaProviderFromPublicKey(PublicKey);
            //return Convert.ToBase64String(_publicKeyRsaProvider.Encrypt(Encoding.UTF8.GetBytes(data), false));

            //string key = DinPay.HttpHelp.RSAPrivateKeyJava2DotNet(this.PrivateKey);
            //return DinPay.HttpHelp.RSASign(data, key);
        }

        protected override Dictionary<BankType, string> BankCode
        {
            get
            {
                Dictionary<BankType, string> dic = new Dictionary<BankType, string>();
                dic.Add(BankType.CCB, "CCB");
                dic.Add(BankType.ABC, "ABC");
                dic.Add(BankType.ICBC, "ICBC");
                dic.Add(BankType.BOC, "BOC");
                dic.Add(BankType.SPDB, "SPDB");
                dic.Add(BankType.CEB, "CEB");
                dic.Add(BankType.SPABANK, "PABC");
                dic.Add(BankType.CIB, "CIB");
                dic.Add(BankType.PSBC, "PSBC");
                dic.Add(BankType.CITIC, "CITIC");
                dic.Add(BankType.HXBANK, "HXB");
                dic.Add(BankType.GDB, "GDB");
                dic.Add(BankType.CMB, "CMB");
                dic.Add(BankType.BJBANK, "BCCB");
                dic.Add(BankType.SHBANK, "SHB");
                dic.Add(BankType.CMBC, "CMBC");
                dic.Add(BankType.COMM, "BCOM");
                dic.Add(BankType.BJRCB, "BJRCB");
                return dic;
            }
        }

        #region =========== 加密  =============

        private class HttpHelp
        {
            /// <summary>
            /// 签名
            /// </summary>
            /// <param name="data">待加密的字符串</param>
            /// <param name="privateKey">私钥</param>
            /// <returns></returns>
            public static string Sign(string data, string privateKey)
            {
                RSACryptoServiceProvider rsaCsp = LoadCertificate(privateKey);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = rsaCsp.SignData(dataBytes, "SHA1");
                return Hex_2To16(signatureBytes);
            }

            private static RSACryptoServiceProvider LoadCertificate(string privateKey)
            {
                byte[] res = res = Convert.FromBase64String(privateKey);
                try
                {
                    RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(res);
                    return rsa;
                }
                catch
                {
                    return null;
                }
            }

            private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
            {
                byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

                // --------- Set up stream to decode the asn.1 encoded RSA private key ------
                MemoryStream mem = new MemoryStream(privkey);
                BinaryReader binr = new BinaryReader(mem);  //wrap Memory Stream with BinaryReader for easy reading
                byte bt = 0;
                ushort twobytes = 0;
                int elems = 0;
                try
                {
                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                        binr.ReadByte();    //advance 1 byte
                    else if (twobytes == 0x8230)
                        binr.ReadInt16();    //advance 2 bytes
                    else
                        return null;

                    twobytes = binr.ReadUInt16();
                    if (twobytes != 0x0102) //version number
                        return null;
                    bt = binr.ReadByte();
                    if (bt != 0x00)
                        return null;


                    //------ all private key components are Integer sequences ----
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
                    CspParameters CspParameters = new CspParameters();
                    CspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(1024, CspParameters);
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
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    binr.Close();
                }
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


            /// <summary>
            /// 2进制转16进制
            /// </summary>
            public static String Hex_2To16(Byte[] bytes)
            {
                String hexString = String.Empty;
                Int32 iLength = 65535;
                if (bytes != null)
                {
                    StringBuilder strB = new StringBuilder();

                    if (bytes.Length < iLength)
                    {
                        iLength = bytes.Length;
                    }

                    for (int i = 0; i < iLength; i++)
                    {
                        strB.Append(bytes[i].ToString("X2"));
                    }
                    hexString = strB.ToString();
                }
                return hexString;
            }

            /// <summary>
            /// 公钥加密
            /// </summary>
            /// <param name="encryptInfo"></param>
            /// <param name="publicKey"></param>
            /// <returns></returns>
            public static string RSAEncrypt(string encryptInfo, string publicKey)
            {
                try
                {
                    byte[] dataBytes = Encoding.UTF8.GetBytes(encryptInfo);
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    //RSAParameters para = new RSAParameters();
                    rsa.FromXmlString(publicKey);
                    int keySize = rsa.KeySize / 8;
                    int bufferSize = keySize - 11;
                    byte[] buffer = new byte[bufferSize];
                    MemoryStream msInput = new MemoryStream(dataBytes);
                    MemoryStream msOutput = new MemoryStream();
                    int readLen = msInput.Read(buffer, 0, bufferSize);
                    while (readLen > 0)
                    {
                        byte[] dataToEnc = new byte[readLen];
                        Array.Copy(buffer, 0, dataToEnc, 0, readLen);
                        byte[] encData = rsa.Encrypt(dataToEnc, false);
                        msOutput.Write(encData, 0, encData.Length);
                        readLen = msInput.Read(buffer, 0, bufferSize);
                    }
                    msInput.Close();
                    byte[] result = msOutput.ToArray();    //得到加密结果
                    msOutput.Close();
                    rsa.Clear();
                    return Convert.ToBase64String(result);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            /// <summary>
            /// 私钥解密
            /// </summary>
            /// <param name="encryptedData"></param>
            /// <param name="privateKey"></param>
            /// <returns></returns>
            public static string RSADecrypt(string encryptedData, string privateKey)
            {
                try
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);
                    //RSAParameters para = new RSAParameters();
                    rsa.FromXmlString(privateKey);
                    byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                    int keySize = rsa.KeySize / 8;
                    byte[] buffer = new byte[keySize];
                    MemoryStream msInput = new MemoryStream(encryptedBytes);
                    MemoryStream msOutput = new MemoryStream();
                    int readLen = msInput.Read(buffer, 0, keySize);
                    while (readLen > 0)
                    {
                        byte[] dataToDec = new byte[readLen];
                        Array.Copy(buffer, 0, dataToDec, 0, readLen);
                        byte[] decData = rsa.Decrypt(dataToDec, false);
                        msOutput.Write(decData, 0, decData.Length);
                        readLen = msInput.Read(buffer, 0, keySize);
                    }
                    msInput.Close();
                    byte[] result = msOutput.ToArray();    //得到解密结果
                    msOutput.Close();
                    rsa.Clear();
                    return Encoding.UTF8.GetString(result);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        #endregion
    }
}
