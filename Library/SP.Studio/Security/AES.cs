using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace SP.Studio.Security
{
    /// <summary>
    /// AES加密
    /// </summary>
    public class AES
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Data">加密内容</param>
        /// <param name="Key">加密密钥</param>
        /// <returns></returns>
        public static string AESEncrypts(String Data, String Key)
        {
            // 256-AES key        
            byte[] keyArray = HexStringToBytes(Key);//UTF8Encoding.ASCII.GetBytes(Key);  
            byte[] toEncryptArray = UTF8Encoding.ASCII.GetBytes(Data);

            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0,
                    toEncryptArray.Length);

            return BytesToHexString(resultArray);
        }

        /// <summary>  
        /// Byte array to convert 16 hex string  
        /// </summary>  
        /// <param name="bytes">byte array</param>  
        /// <returns>16 hex string</returns>  
        private static string BytesToHexString(byte[] bytes)
        {
            StringBuilder returnStr = new StringBuilder();
            if (bytes != null || bytes.Length == 0)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr.Append(bytes[i].ToString("X2"));
                }
            }
            return returnStr.ToString();
        }

        /// <summary>  
        /// 16 hex string converted to byte array  
        /// </summary>  
        /// <param name="hexString">16 hex string</param>  
        /// <returns>byte array</returns>  
        private static byte[] HexStringToBytes(String hexString)
        {
            if (hexString == null || hexString.Equals(""))
            {
                return null;
            }
            int length = hexString.Length / 2;
            if (hexString.Length % 2 != 0)
            {
                return null;
            }
            byte[] d = new byte[length];
            for (int i = 0; i < length; i++)
            {
                d[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return d;
        }
    }
}
