using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SP.Studio.Json;
using SP.Studio.Net;

namespace SP.Studio.GateWay.WeChat
{
    public static class WX
    {
        /// <summary>
        /// 授权码存储
        /// </summary>
        public static Dictionary<string, Token> token = new Dictionary<string, Token>();

        #region ================== 网关地址 ======================

        /// <summary>
        /// 获取access_token
        /// </summary>
        private const string _client_credential = "http://weixin.api.a8.to/cgi-bin/token?grant_type=client_credential&appid=APPID&secret=APPSECRET";

        /// <summary>
        /// 网页授权路径
        /// </summary>
        private const string _authorize = "https://open.weixin.qq.com/connect/oauth2/authorize?appid=APPID&redirect_uri=REDIRECT_URI&response_type=code&scope=SCOPE&state=STATE#wechat_redirect";

        /// <summary>
        /// 获取用户的OPENID
        /// </summary>
        private const string _client_openid = "http://weixin.api.a8.to/sns/oauth2/access_token?appid=APPID&secret=APPSECRET&code=CODE&grant_type=authorization_code";

        /// <summary>
        /// 获取用户的资料信息
        /// </summary>
        private const string _user_info = "http://weixin.api.a8.to/sns/userinfo?access_token=TOKEN&openid=OPENID";

        #endregion

        /// <summary>
        /// 替换应用ID和密钥
        /// </summary>
        /// <param name="url"></param>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        private static string Get(this string url, string appId, string appSecret)
        {
            return url.Replace("APPID", appId).Replace("APPSECRET", appSecret);
        }

        /// <summary>
        /// 获取授权码
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="expires">超时时间（秒）</param>
        public static string GetAccessToken(string appId, string appSecret, out DateTime expires)
        {
            string result = NetAgent.DownloadData(_client_credential.Get(appId, appSecret), Encoding.UTF8);
            if (token.ContainsKey(appId)) token.Remove(appId);
            Token t = new Token(result);
            expires = t.expireAt;
            return t.access_token;
        }

        /// <summary>
        /// 获取用户的token信息
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static UserToken GetUserToken(string appId, string appSecret, string code)
        {
            string url = _client_openid.Replace("APPID", appId).Replace("APPSECRET", appSecret).Replace("CODE", code);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            UserToken token = new UserToken(result);
            return token;
        }

        /// <summary>
        /// 获取微信账号的信息
        /// </summary>
        /// <returns></returns>
        public static SNSInfo GetSNSInfo(string userToken, string openId)
        {
            string url = _user_info.Replace("TOKEN", userToken).Replace("OPENID", openId);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            return new SNSInfo(result);
        }

        /// <summary>
        /// 获取授权url
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="redirect">授权之后要跳转的路径</param>
        /// <param name="state">场景</param>
        /// <param name="scope">授权类型</param>
        /// <returns></returns>
        public static string GetAuthorizeUrl(string appId, string redirect, string state, string scope = "snsapi_base")
        {
            return _authorize.Replace("APPID", appId)
                .Replace("REDIRECT_URI", HttpUtility.UrlEncode(redirect))
                .Replace("SCOPE", scope)
                .Replace("STATE", HttpUtility.UrlEncode(state));
        }


        /// <summary>
        /// 接入地址检查
        /// </summary>
        /// <param name="token">密钥</param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool checkSignature(string token, string timestamp, string nonce, string signature)
        {
            ///handler/wechat.ashx?signature=bb2ebe51965cb4e26b49e80f6cf6e627b328a4cf&echostr=2083580646355107007&timestamp=1524477075&nonce=3046370706
            List<string> list = new List<string>();
            list.Add(token);
            list.Add(timestamp);
            list.Add(nonce);

            string signStr = string.Join(string.Empty, list.OrderBy(t => t));

            return signature.Equals(Security.MD5.toSHA1(signStr), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
