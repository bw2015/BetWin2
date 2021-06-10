using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Security;

namespace SP.Studio.Security
{
    /// <summary>
    /// 不可逆的加密算法
    /// </summary>
    public class MD5
    {

        /// <summary>
        /// MD5与SHA1的双重加密算法（40位密文）
        /// </summary>
        /// <param name="text">要加密的明文</param>
        /// <returns>加密之后的字串符</returns>
        public static string Encrypto(string text)
        {
            return toSHA1(toMD5(text));
        }

        /// <summary>
        /// 系统自带的MD5加密（大写）
        /// </summary>
        /// <param name="text"></param>
        /// <param name="compatible">兼容模式 可选 php</param>
        /// <returns>返回的是大写</returns>
        public static string toMD5(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }

        public static string toMD5(byte[] data)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            data = md5Hasher.ComputeHash(data);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString().ToUpper();
        }

        /// <summary>
        /// 使用系统自带的SHA1加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns>大写</returns>
        public static string toSHA1(string text)
        {
            SHA1 algorithm = SHA1.Create();
            byte[] data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
            string sh1 = "";
            for (int i = 0; i < data.Length; i++)
            {
                sh1 += data[i].ToString("x2").ToUpperInvariant();
            }
            return sh1.ToUpper();
        }

        /// <summary>
        /// 兼容PHP的 sha1 加密算法
        /// </summary>
        /// <param name="data"></param>
        /// <returns>小写</returns>
        public static string toSHA1Sign(string data)
        {
            byte[] temp1 = Encoding.UTF8.GetBytes(data);
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            byte[] temp2 = sha.ComputeHash(temp1);
            sha.Clear();
            // 注意， 不能用这个
            // string output = Convert.ToBase64String(temp2);// 不能直接转换成base64string
            var output = BitConverter.ToString(temp2);
            output = output.Replace("-", "");
            output = output.ToLower();
            return output;
        }


        /// <summary>
        /// 标准的MD5加密，截取前多少位
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns>大写</returns>
        public static string Encrypto(string text, int length)
        {
            return toMD5(text).Substring(0, length);
        }

        /// <summary>
        /// 与ASP兼容的MD5加密算法
        /// </summary>
        /// <param name="text">要加密的明文</param>
        /// <param name="charset">编码</param>
        /// <returns>小写</returns>
        public static string Encryp(string text, string charset)
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] t = md5.ComputeHash(Encoding.GetEncoding(charset).GetBytes(text));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 与ASP兼容的MD5加密算法
        /// </summary>
        /// <param name="text">要加密的明文</param>
        /// <returns></returns>
        public static string Encryp(string text)
        {
            return Encryp(text, "utf-8");
        }

        /// <summary>
        /// 判断一个值是不是MD5
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public static bool IsMD5(string pass)
        {
            return Regex.IsMatch(pass, @"^[0-9a-f]{32}$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 验证密码是否正确
        /// </summary>
        /// <param name="text">密码明文</param>
        /// <param name="pass">密码密文</param>
        /// <returns>比较是否正确</returns>
        public static bool Check(string text, string pass)
        {
            return Encrypto(text).ToLower().Equals(pass.ToLower());
        }

        /// <summary>
        /// 一个简易版的数字不可逆算法
        /// <param name="id">要加密的ID</param>
        /// <param name="key">加密因子</param>
        /// </summary>
        public static int EasyEncryp(int id, int key)
        {
            int i = 0;
            byte[] bytes = Encoding.ASCII.GetBytes(id.ToString());
            foreach (byte b in bytes) i += key == 0 ? b : b % key;
            return i;
        }

        /// <summary>
        /// 一个简易的加密
        /// </summary>
        public static int EasyEncryp(int id)
        {
            return EasyEncryp(id, 0);
        }

        /// <summary>
        /// 通过使用计算基于哈希的消息身份验证代码 (HMAC) MD5 哈希函数。
        /// </summary>
        /// <param name="signStr"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static String HMACMD5(String signStr, String key)
        {
            using (HMACMD5 hmac = new HMACMD5(Encoding.UTF8.GetBytes(key)))
            {
                byte[] hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(signStr));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashedPassword.Length; i++)
                {
                    sb.Append(hashedPassword[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }


    }
}
