using BW.Agent;
using BW.Common.Sites;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace BW.GateWay.Payment
{
    public class EPayGG : IPayment
    {
        public EPayGG()
        {
        }

        public EPayGG(string settingString) : base(settingString)
        {
        }

        [Description("网关")]
        public string Gateway { get; set; } = "http://114.55.27.42:9001/mapi/gateway.htm";

        [Description("产品")]
        public string product { get; set; }

        [Description("seller_user")]
        public string seller_user_id { get; set; }

        [Description("商户号")]
        public string partner_id { get; set; }

        [Description("异步通知")]
        public string NotifyUrl { get; set; } = "/handler/payment/EPayGG";

        [Description("同步通知")]
        public string ReturnUrl { get; set; } = "/handler/payment/EPayGG";

        [Description("平台公钥")]
        public string public_key { get; set; }

        [Description("商户私钥")]
        public string private_key { get; set; }

        public override string ShowCallback()
        {
            return "success";
        }
        public override string GetTradeNo(out decimal money, out string systemId)
        {
            int payId = WebAgent.QS("PayID", 0);

            PaymentSetting setting = SiteAgent.Instance().GetPaymentSettingInfo(payId);
            if (setting == null) { throw new Exception("未指定支付渠道编号"); }

            EPayGG pay = new EPayGG(setting.SettingString);

            var random_key = WebAgent.QF("random_key");
            var biz_content = WebAgent.QF("biz_content");
            var sign = WebAgent.QF("sign");

            string waitSign = "biz_content" + biz_content;
            waitSign += "random_key" + random_key;

            

            bool bVefifysign = DinPay.HttpHelp.ValidateRsaSign(waitSign, DinPay.HttpHelp.RSAPublicKeyJava2DotNet(pay.public_key), sign, "sha1", "hex");
            if (bVefifysign)
            {
                string random_Src = RSAHelper.decryptData(random_key, pay.private_key);
                string biz = UTF8Encoding.UTF8.GetString(AESHelper.AESDecrypt(biz_content, random_Src, "16-Bytes--String"));
                var p = JsonConvert.DeserializeObject<EpayResultModel>(biz);
                if (p != null && p.trade_status != null && p.trade_status == "TRADE_FINISHED") //交易成功
                {
                    systemId = p.trade_no;
                    money = p.amount.GetValue<decimal>();
                    return p.out_trade_no;
                }
                else
                {
                    money = decimal.Zero;
                    return systemId = null;
                }
            }
            else
            {
                money = decimal.Zero;
                return systemId = null;
            }
        }

        public override void GoGateway()
        {
            SortedDictionary<string, string> sParaContent = new SortedDictionary<string, string>();
            sParaContent.Add("create_and_pay", "true");
            sParaContent.Add("product", this.product);
            sParaContent.Add("seller_user_id", this.seller_user_id);

            sParaContent.Add("terminal_id", "000000");
            sParaContent.Add("currency", "156");  //货币类型
            sParaContent.Add("total_fee", this.Money.ToString("0.00"));
            sParaContent.Add("summary", "PAYMENT");
            sParaContent.Add("out_trade_no", this.OrderID);
            sParaContent.Add("gmt_out_create", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            string biz_content = JsonConvert.SerializeObject(sParaContent);  // 业务逻辑 json 格式

            string charset = "utf-8";
            string format = "json";
            string method = "epaypp.trade.create";
            string notify_url = this.GetUrl(this.NotifyUrl);
            string sign_method = "rsa";
            string random_key = System.Guid.NewGuid().ToString().Substring(0, 16);
            string return_url = this.GetUrl(this.ReturnUrl);
            string timestamp = sParaContent["gmt_out_create"];
            string v = "1.1";

            //业务逻辑 加密，用随机码的明文
            byte[] data = AESHelper.AESEncrypt(biz_content, random_key, "16-Bytes--String");
            string strEncryptContent = RSAHelper.byte2HexString(data); ;
            biz_content = strEncryptContent;
            //随机码 加密
            random_key = RSAHelper.encryptData(random_key, this.public_key);

            // 签名数据
            string waitSign = "biz_content" + biz_content;
            waitSign += "charset" + charset;
            waitSign += "format" + format;
            waitSign += "method" + method;
            waitSign += "notify_url" + notify_url;
            waitSign += "partner_id" + partner_id;
            waitSign += "random_key" + random_key;  //此处用随机码的密文
            waitSign += "return_url" + return_url;
            waitSign += "sign_method" + sign_method;
            waitSign += "timestamp" + timestamp;
            waitSign += "v" + v;
            string sign = DinPay.HttpHelp.RSASign(waitSign, DinPay.HttpHelp.RSAPrivateKeyJava2DotNet(this.private_key), "sha1", "hex");  //带-----BEGIN PRIVATE KEY

            //post提交数据
            string param = "biz_content=" + biz_content;
            param += "&charset=" + charset;
            param += "&format=" + format;
            param += "&method=" + method;
            param += "&notify_url=" + notify_url;
            param += "&partner_id=" + partner_id;
            param += "&sign_method=" + sign_method;
            param += "&random_key=" + random_key;
            param += "&return_url=" + return_url;
            param += "&timestamp=" + timestamp;
            param += "&v=" + v;
            param += "&sign=" + HttpUtility.UrlEncode(sign);

            string responseData = NetAgent.UploadData(this.Gateway, param, Encoding.UTF8);

            try
            {
                var p = JsonConvert.DeserializeObject<EpayQueryModel>(responseData);
                if (p != null)
                {
                    if (p.epaypp_trade_create_response.return_type == "HTML")
                    {
                        this.context.Response.Write(p.epaypp_trade_create_response.html);
                    }
                    else if (p.epaypp_trade_create_response.return_type == "URL")
                    {
                        this.BuildForm(p.epaypp_trade_create_response.action_url);
                    }
                    else
                    {
                        this.context.Response.Write(responseData);
                    }
                }
                else
                {
                    this.context.Response.Write(responseData);
                }
            }
            catch (System.Exception ex)
            {
                this.context.Response.Write(ex.Message);
            }
        }

        public override bool Verify(VerifyCallBack callback)
        {
            decimal money;
            string systemId;
            string orderId = this.GetTradeNo(out money, out systemId);
            if (string.IsNullOrEmpty(orderId)) return false;

            callback.Invoke();
            return true;
        }

        /// <summary>
        /// DES加密/解密类。
        /// </summary>
        public class AESHelper
        {
            /// <summary>    
            /// AES加密    
            /// </summary>    
            /// <param name="Data">被加密的明文</param>    
            /// <param name="Key">密钥</param>    
            /// <param name="Vector">向量 AES/CBC/PKCS5Padding</param>    
            /// <returns>密文</returns>    
            public static byte[] AESEncrypt(String Data, String Key, String Vector)
            {
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(Key);
                byte[] ivArray = UTF8Encoding.UTF8.GetBytes(Vector);
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(Data);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.IV = ivArray;
                rDel.Mode = CipherMode.CBC;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return resultArray;
            }

            /// <summary>    
            /// AES解密    
            /// </summary>    
            /// <param name="Data">被解密的密文</param>    
            /// <param name="Key">密钥</param>    
            /// <param name="Vector">向量</param>    
            /// <returns>明文</returns>    
            public static byte[] AESDecrypt(String Data, String Key, String Vector)
            {
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(Key);
                byte[] ivArray = UTF8Encoding.UTF8.GetBytes(Vector);
                //byte[] toEncryptArray = Convert.FromBase64String(Data);
                byte[] toEncryptArray = HexStringToByteArray(Data);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.IV = ivArray;
                rDel.Mode = CipherMode.CBC;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return resultArray;
            }

            public static byte[] HexStringToByteArray(string s)
            {
                s = s.Replace(" ", "");
                byte[] buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                {
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                }
                return buffer;
            }
            /// <summary>    
            /// AES加密(无向量)    
            /// </summary>    
            /// <param name="plainBytes">被加密的明文</param>    
            /// <param name="key">密钥</param>    
            /// <returns>密文</returns>    
            public static byte[] AESEncrypt(String Data, String Key)
            {
                MemoryStream mStream = new MemoryStream();
                RijndaelManaged aes = new RijndaelManaged();

                byte[] plainBytes = Encoding.UTF8.GetBytes(Data);
                Byte[] bKey = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                //aes.Key = _key;    
                aes.Key = bKey;
                //aes.IV = _iV;    
                CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                try
                {
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    return mStream.ToArray();
                }
                finally
                {
                    cryptoStream.Close();
                    mStream.Close();
                    aes.Clear();
                }
            }


            /// <summary>    
            /// AES解密(无向量)    
            /// </summary>    
            /// <param name="encryptedBytes">被加密的明文</param>    
            /// <param name="key">密钥</param>    
            /// <returns>明文</returns>    
            public static string AESDecrypt(String Data, String Key)
            {
                Byte[] encryptedBytes = Convert.FromBase64String(Data);
                Byte[] bKey = new Byte[32];
                Array.Copy(Encoding.UTF8.GetBytes(Key.PadRight(bKey.Length)), bKey, bKey.Length);

                MemoryStream mStream = new MemoryStream(encryptedBytes);
                //mStream.Write( encryptedBytes, 0, encryptedBytes.Length );    
                //mStream.Seek( 0, SeekOrigin.Begin );    
                RijndaelManaged aes = new RijndaelManaged();
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                aes.KeySize = 128;
                aes.Key = bKey;
                //aes.IV = _iV;    
                CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                try
                {
                    byte[] tmp = new byte[encryptedBytes.Length + 32];
                    int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                    byte[] ret = new byte[len];
                    Array.Copy(tmp, 0, ret, 0, len);
                    return Encoding.UTF8.GetString(ret);
                }
                finally
                {
                    cryptoStream.Close();
                    mStream.Close();
                    aes.Clear();
                }
            }



        }

        /// <summary>
        /// RSA加密算法
        /// </summary>
        public static class RSAHelper
        {
            /// <summary>
            /// bytes转16进制大写
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


            /// <summary>
            /// rsa公钥加密
            /// </summary>
            /// <param name="source"></param>
            /// <param name="publicKey"></param>
            /// <returns></returns>
            public static String encryptData(String source, String publicKey)
            {
                //byte[] DataToEncrypt = Encoding.ASCII.GetBytes(source);
                byte[] DataToEncrypt = Encoding.UTF8.GetBytes(source);
                string result = encrypt(DataToEncrypt, publicKey, "UTF-8");
                return result;
            }

            /// <summary>
            /// 加密
            /// </summary>
            /// <param name="data"></param>
            /// <param name="publicKey"></param>
            /// <param name="input_charset"></param>
            /// <returns></returns>
            private static string encrypt(byte[] data, string publicKey, string input_charset)
            {
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                RSAalg.FromXmlString(RSAKeyConvert.RSAPublicKeyJava2DotNet(publicKey));
                byte[] signedData = RSAalg.Encrypt(data, false);
                //string str_SignedData = Convert.ToBase64String(signedData);
                return byte2HexString(signedData);
            }

            /// <summary>
            /// RSA私钥签名
            /// </summary>
            /// <param name="content"></param>
            /// <param name="privateKey"></param>
            /// <returns></returns>
            public static String sign(String content, String privateKey)
            {
                byte[] DataToSign = Encoding.UTF8.GetBytes(content);
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                RSAalg.FromXmlString(RSAKeyConvert.RSAPrivateKeyJava2DotNet(privateKey));
                byte[] signedData = RSAalg.SignData(DataToSign, new SHA1CryptoServiceProvider());
                //string str_SignedData = Convert.ToBase64String(signedData);
                return byte2HexString(signedData);
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

            /// <summary>
            /// RSA公钥验签
            /// </summary>
            /// <param name="content"></param>
            /// <param name="sign"></param>
            /// <param name="publicKey"></param>
            /// <returns></returns>
            public static bool checkSign(String content, String sign, String publicKey)
            {
                bool result = false;

                byte[] Data = Encoding.UTF8.GetBytes(content);
                byte[] data = Convert.FromBase64String(sign);
                RSAParameters paraPub = ConvertFromPublicKey(publicKey);
                RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
                rsaPub.ImportParameters(paraPub);

                SHA1 sh = new SHA1CryptoServiceProvider();
                result = rsaPub.VerifyData(Data, sh, data);
                return result;
            }
            public static bool checkSign2(String content, String sign, String publicKey)
            {
                //微支付验签专用
                bool result = false;

                byte[] Data = Encoding.UTF8.GetBytes(content);
                //byte[] data = Convert.FromBase64String(sign);
                byte[] data = HexStringToByteArray(sign);
                RSAParameters paraPub = ConvertFromPublicKey(publicKey);
                RSACryptoServiceProvider rsaPub = new RSACryptoServiceProvider();
                rsaPub.ImportParameters(paraPub);

                SHA1 sh = new SHA1CryptoServiceProvider();
                result = rsaPub.VerifyData(Data, sh, data);
                return result;
            }
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

            /// <summary>
            /// RSA私钥解密
            /// </summary>
            /// <param name="resData"></param>
            /// <param name="privateKey"></param>
            /// <returns></returns>
            public static string decryptData(string resData, string privateKey)
            {
                // byte[] DataToDecrypt = Convert.FromBase64String(resData);
                byte[] DataToDecrypt = HexStringToByteArray(resData);

                string result = "";
                //for (int j = 0; j < DataToDecrypt.Length / 128; j++)
                //{
                //    byte[] buf = new byte[128];
                //    for (int i = 0; i < 128; i++)
                //    {

                //        buf[i] = DataToDecrypt[i + 128 * j];
                //    }
                //    result += decrypt(buf, privateKey);
                //}
                result = decrypt(DataToDecrypt, privateKey);
                return result;
            }
            private static string decrypt(byte[] data, string privateKey)
            {
                string result = "";
                RSACryptoServiceProvider rsa = DecodePemPrivateKey(privateKey);
                SHA1 sh = new SHA1CryptoServiceProvider();
                byte[] source = rsa.Decrypt(data, false);
                char[] asciiChars = new char[Encoding.GetEncoding("UTF-8").GetCharCount(source, 0, source.Length)];
                Encoding.GetEncoding("UTF-8").GetChars(source, 0, source.Length, asciiChars, 0);
                result = new string(asciiChars);
                //result = ASCIIEncoding.ASCII.GetString(source);  
                return result;
            }

            public static byte[] HexStringToByteArray(string s)
            {
                s = s.Replace(" ", "");
                byte[] buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                {
                    buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
                }
                return buffer;
            }



            /**
            * RSA私钥解密 (微支付用)
            */
            public static string decryptData2(string resData, string privateKey)
            {
                //byte[] DataToDecrypt = Convert.FromBase64String(resData);
                //byte[] DataToDecrypt = HexStringToByteArray(resData);

                byte[] DataToDecrypt = Convert.FromBase64String(resData);

                string result = "";
                for (int j = 0; j < DataToDecrypt.Length / 128; j++)
                {
                    byte[] buf = new byte[128];
                    for (int i = 0; i < 128; i++)
                    {

                        buf[i] = DataToDecrypt[i + 128 * j];
                    }
                    result += decrypt(buf, privateKey);
                }
                return result;
            }
            public static string decryptData3(string resData, string privateKey)
            {
                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                RSAalg.FromXmlString(RSAKeyConvert.RSAPrivateKeyJava2DotNet(privateKey));
                ;
                byte[] DataToDecrypt = HexStringToByteArray(resData);

                byte[] signatureBytes = RSAalg.Decrypt(DataToDecrypt, false);
                return Encoding.UTF8.GetString(signatureBytes);
            }
        }

        /// <summary>
        /// DES加密/解密类。
        /// </summary>
        public class RSAKeyConvert
        {
            /// <summary>
            /// RSA私钥格式转换，java->.net
            /// </summary>
            /// <param name="privateKey">java生成的RSA私钥</param>
            /// <returns></returns>
            public static string RSAPrivateKeyJava2DotNet(string privateKey)
            {
                privateKey = privateKey.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Replace("\n", "").Replace("\r", "");
                string dummyData = privateKey.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+");
                if (dummyData.Length % 4 > 0)
                {
                    dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
                }
                RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(dummyData));

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
            /// RSA私钥格式转换，.net->java
            /// </summary>
            /// <param name="privateKey">.net生成的私钥</param>
            /// <returns></returns>
            public static string RSAPrivateKeyDotNet2Java(string privateKey)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(privateKey);
                BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
                BigInteger exp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
                BigInteger d = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("D")[0].InnerText));
                BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("P")[0].InnerText));
                BigInteger q = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Q")[0].InnerText));
                BigInteger dp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DP")[0].InnerText));
                BigInteger dq = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DQ")[0].InnerText));
                BigInteger qinv = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("InverseQ")[0].InnerText));

                RsaPrivateCrtKeyParameters privateKeyParam = new RsaPrivateCrtKeyParameters(m, exp, d, p, q, dp, dq, qinv);

                PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParam);
                byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetEncoded();
                return Convert.ToBase64String(serializedPrivateBytes);
            }

            /// <summary>
            /// RSA公钥格式转换，java->.net
            /// </summary>
            /// <param name="publicKey">java生成的公钥</param>
            /// <returns></returns>
            public static string RSAPublicKeyJava2DotNet(string publicKey)
            {
                RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
                return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                    Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                    Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
            }

            /// <summary>
            /// RSA公钥格式转换，.net->java
            /// </summary>
            /// <param name="publicKey">.net生成的公钥</param>
            /// <returns></returns>
            public static string RSAPublicKeyDotNet2Java(string publicKey)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(publicKey);
                BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
                BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
                RsaKeyParameters pub = new RsaKeyParameters(false, m, p);

                SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
                byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
                return Convert.ToBase64String(serializedPublicBytes);
            }

        }

        public class EpayQueryModel
        {
            public epayppModel epaypp_trade_create_response { get; set; }
        }
        public class epayppModel
        {
            public string trade_no { get; set; }
            public string seller_user_id { get; set; }
            public string timeout { get; set; }
            public string action_method { get; set; }
            public string action_url { get; set; }
            public string result_code { get; set; }
            public string result_code_msg { get; set; }
            public string return_type { get; set; }

            public string success { get; set; }
            public string sign { get; set; }
            public string html { get; set; }
        }

        public class EpayResultModel
        {
            public string out_trade_no { get; set; }
            public string trade_no { get; set; }
            public string trade_status { get; set; }
            public string amount { get; set; }
            public string charge_amount { get; set; }
            public string channel { get; set; }
            public string gmt_create { get; set; }
            public string gmt_pay { get; set; }
            public string terminalSn { get; set; }
        }
    }
}
