using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.GateWay.WeChat;

namespace Web.GateWay.App_Code
{
    public class WXAgent : AgentBase
    {
        public WXSetting GetWXSetting(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                string setting = (string)db.ExecuteScalar(CommandType.Text, "SELECT OpenSetting FROM wx_Setting WHERE SiteID = @SiteID",
                    NewParam("@SiteID", siteId));

                return new WXSetting(setting);
            }
        }

        /// <summary>
        /// 获取微信的token
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public string GetToken(int siteId, WXSetting setting)
        {
            string token;
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT Token,ExpireAt FROM wx_Setting WHERE SiteID = @SiteID",
                    NewParam("@SiteID", siteId));
                if (ds.Tables[0].Rows.Count == 1)
                {
                    token = (string)ds.Tables[0].Rows[0]["Token"];
                    DateTime expireAt = (DateTime)ds.Tables[0].Rows[0]["ExpireAt"];
                    if (expireAt > DateTime.Now) return token;
                }
                DateTime expire = DateTime.MinValue;
                token = WX.GetAccessToken(setting.AppId, setting.AppSecret, out expire);
                if (string.IsNullOrEmpty(token)) return null;
                db.ExecuteNonQuery(CommandType.Text, "UPDATE wx_Setting SET Token = @Token,ExpireAt = @ExpireAt WHERE SiteID = @SiteID",
                    NewParam("@SiteID", siteId),
                    NewParam("@Token", token),
                    NewParam("@ExpireAt", expire));
                return token;
            }
        }

        /// <summary>
        /// 保存用户的token授权信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="token"></param>
        public bool SaveUserToken(int siteId, UserToken token)
        {
            if (string.IsNullOrEmpty(token.openid)) return false;
            DateTime time = token.expires;
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, @"IF EXISTS(SELECT 0 FROM usr_Wechat WHERE SiteID = @SiteID AND OpenID = @OpenID) BEGIN
	UPDATE usr_Wechat SET Token = @Token,TokenExpire = @Time WHERE SiteID = @SiteID AND OpenID = @OpenID
END ELSE BEGIN
	INSERT INTO usr_Wechat(UserID,SiteID,OpenID,Guid,Token,TokenExpire) VALUES(0,@SiteID,@OpenID,NEWID(),@Token,@Time)
END",
                NewParam("@SiteID", siteId),
                NewParam("@OpenID", token.openid),
                NewParam("@Token", token.access_token),
                NewParam("@Time", time));
            }
            return true;
        }

        /// <summary>
        /// 保存微信的用户信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="openId"></param>
        /// <returns>返回获取是否成功</returns>
        public bool SaveUserInfo(int siteId, string openId)
        {
            if (string.IsNullOrEmpty(openId)) return false;

            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT * FROM usr_Wechat WHERE SiteID = @SiteID AND OpenID = @OpenID",
                    NewParam("@SiteID", siteId),
                    NewParam("@OpenID", openId));
                if (ds.Tables[0].Rows.Count == 0) return false;

                UserToken token = new UserToken(ds.Tables[0].Rows[0]);
                if (!string.IsNullOrEmpty(token.errmsg)) return false;


                SNSInfo info = WX.GetSNSInfo(token.access_token, token.openid);
                if (string.IsNullOrEmpty(info.openid)) return false;

                db.ExecuteNonQuery(CommandType.Text, "UPDATE usr_Wechat SET UserInfo = @Info WHERE SiteID = @SiteID AND OpenID = @OpenID",
                    NewParam("@Info", info.ToString()),
                    NewParam("@SiteID", siteId),
                    NewParam("@OpenID", openId));

                return true;
            }
        }
    }

    /// <summary>
    /// 微信接口的对接参数
    /// </summary>
    public class WXSetting : SettingBase
    {
        public WXSetting(string setting) : base(setting) { }

        //AppId=wx326ee06def73d98b&AppSecret=2dc9f8be4ff53c891dc58a1b9d7cd012&Token=isis&Domain=
        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string Token { get; set; }
    }
}