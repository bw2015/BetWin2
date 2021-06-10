using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP.Studio.Security
{
    /// <summary>
    /// BASE64操作类-乐清 2012.12.9
    /// </summary>
    public class BASE64
    {
        /// <summary>
        /// BASE64加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Base64Encoder(string Enstr)
        {
            byte[] bytes = Encoding.Default.GetBytes(Enstr);
            return Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// BASE64解密
        /// </summary>
        /// <param name="Destr"></param>
        /// <returns></returns>
        public static string Base64Decoder(string Destr)
        {
            byte[] outputb = Convert.FromBase64String(Destr);
            return Encoding.Default.GetString(outputb);

        }
    }
}
