using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SP.Studio.Json;
using SP.Studio.Text;
namespace SP.Studio.GateWay.WeChat
{
    /// <summary>
    /// 获取的token
    /// </summary>
    public struct Token
    {
        public Token(string result)
        {
            Hashtable ht = JsonAgent.GetJObject(result);
            if (!ht.ContainsKey("access_token"))
            {
                access_token = string.Empty;
                expireAt = DateTime.MinValue;
                return;
            }
            access_token = ht["access_token"].ToString();
            expireAt = DateTime.Now.AddMinutes(-15).AddSeconds(int.Parse(ht["expires_in"].ToString()));
        }

        /// <summary>
        /// 授权码
        /// </summary>
        public string access_token;

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime expireAt;
    }
}
