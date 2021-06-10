using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace SP.Studio.Security
{
    /// <summary>
    /// .NET 自带的RSA加密方式（与java不兼容）
    /// </summary>
    public class RSA
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="xmlPublicKey">公钥</param>
        /// <param name="EncryptString">明文</param>
        /// <returns>密文</returns>
        public static string RSAEncrypt(string xmlPublicKey, string EncryptString)
        {
            byte[] PlainTextBArray;
            byte[] CypherTextBArray;
            string Result = String.Empty;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPublicKey);
            int t = (int)(Math.Ceiling((double)EncryptString.Length / (double)50));
            //分割明文  
            for (int i = 0; i <= t - 1; i++)
            {

                PlainTextBArray = (new UnicodeEncoding()).GetBytes(EncryptString.Substring(i * 50, EncryptString.Length - (i * 50) > 50 ? 50 : EncryptString.Length - (i * 50)));
                CypherTextBArray = rsa.Encrypt(PlainTextBArray, false);
                Result += Convert.ToBase64String(CypherTextBArray) + "ThisIsSplit";
            }
            return Result;
        }


        /// <summary>  
        /// RAS解密  
        /// </summary>  
        /// <param name="xmlPrivateKey">私钥</param>  
        /// <param name="DecryptString">密文</param>  
        /// <returns>明文</returns>  
        public static string RSADecrypt(string xmlPrivateKey, string DecryptString)
        {
            byte[] PlainTextBArray;
            byte[] DypherTextBArray;
            string Result = String.Empty;
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(xmlPrivateKey);
            string[] Split = new string[1];
            Split[0] = "ThisIsSplit";
            //分割密文  
            string[] mis = DecryptString.Split(Split, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < mis.Length; i++)
            {
                PlainTextBArray = Convert.FromBase64String(mis[i]);
                DypherTextBArray = rsa.Decrypt(PlainTextBArray, false);
                Result += (new UnicodeEncoding()).GetString(DypherTextBArray);
            }
            return Result;
        }


        /// <summary>  
        /// 产生公钥和私钥对  
        /// </summary>  
        /// <returns>string[] 0:私钥;1:公钥</returns>  
        public static string[] RSAKey()
        {
            string[] keys = new string[2];
            System.Security.Cryptography.RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            keys[0] = rsa.ToXmlString(true);
            keys[1] = rsa.ToXmlString(false);
            return keys;
        }
    }
}
