using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Common.Users;
using SP.Studio.Data;

namespace BW.Agent
{
    /// <summary>
    /// 微信相关
    /// </summary>
    partial class UserAgent
    {
        /// <summary>
        /// 生成一个随机码
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Guid GetWechatKey(int userId)
        {
            UserWechat wx = BDC.UserWechat.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).FirstOrDefault();

            if (wx == null || string.IsNullOrEmpty(wx.OpenId))
            {
                UserWechatKey key = BDC.UserWechatKey.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId).FirstOrDefault();
                if (key != null)
                {
                    if (key.CreateAt.AddMinutes(5) > DateTime.Now) return key.Key;
                    key.Key = Guid.NewGuid();
                    key.Update(null, t => t.Key);
                }
                else
                {
                    key = new UserWechatKey()
                    {
                        Key = Guid.NewGuid(),
                        SiteID = this.GetSiteID(userId),
                        UserID = userId,
                        CreateAt = DateTime.Now
                    };
                    key.Add();
                }
                return key.Key;
            }
            return Guid.Empty;
        }

        /// <summary>
        /// 根据OpenID获取用户绑定的微信帐号信息
        /// </summary>
        /// <param name="openId"></param>
        /// <returns></returns>
        public UserWechat GetUserWechat(string openId)
        {
            return BDC.UserWechat.Where(t => t.SiteID == SiteInfo.ID && t.OpenId == openId).FirstOrDefault();
        }
    }
}
