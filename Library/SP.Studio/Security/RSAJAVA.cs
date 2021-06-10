using Org.BouncyCastle;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SP.Studio.Security
{
    /// <summary>
    /// 与JAVA兼容的RSA加密格式
    /// </summary>
    public static class RSAJAVA
    {
        /// <summary>
        /// 私钥格式转换
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 公钥格式转换
        /// </summary>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static string RSAPublicKeyJava2DotNet(string publicKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
                Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())
            );
        }

        /// <summary>
        /// 私钥签名
        /// </summary>
        /// <param name="signStr">明文</param>
        /// <param name="privateKey">私钥（转换过之后）</param>
        /// <returns></returns>
        public static string RSASign(string signStr, string privateKey, string halg = "SHA1")
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(privateKey);
                byte[] signBytes = rsa.SignData(Encoding.UTF8.GetBytes(signStr), halg);
                return Convert.ToBase64String(signBytes);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 公钥验签
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="publicKey">公钥（转换过之后）</param>
        /// <param name="signedData">需要验签的密文</param>
        /// <returns></returns>
        public static bool ValidSign(string plainText, string publicKey, string signedData, string halg = "SHA1")
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(publicKey);
                return rsa.VerifyData(UTF8Encoding.UTF8.GetBytes(plainText), halg, Convert.FromBase64String(signedData));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

    }
}
