using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Data.Linq;
using SP.Studio.GateWay.WeChat;

using BW.Common.Users;
using BW.Common.Wechat;
using BW.Common.Admins;

namespace BW.Agent
{
    /// <summary>
    /// 微信管理
    /// </summary>
    public partial class WechatAgent : AgentBase<WechatAgent>
    {
        /// <summary>
        /// 获取微信帐号参数设定
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public WechatSetting GetWechatSetting(int siteId)
        {
            return BDC.WechatSetting.Where(t => t.SiteID == siteId).FirstOrDefault() ?? new WechatSetting()
            {
                SiteID = SiteInfo.ID,
                Name = SiteInfo.Name
            };
        }



        /// <summary>
        /// 保存微信公共号设置
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool SaveSetting(WechatSetting setting, HttpPostedFile file = null)
        {
            setting.SiteID = SiteInfo.ID;
            if (file != null)
            {
                string image = UserAgent.Instance().UploadImage(file, "upload");
                if (!string.IsNullOrEmpty(image))
                {
                    setting.Face = image;
                }
            }

            AdminInfo.Log(AdminLog.LogType.Wechat, "保存公共号设置：{0}", setting.ToJson());

            if (setting.Exists(null, t => t.SiteID))
            {
                return setting.Update() > 0;
            }
            else
            {
                return setting.Add();
            }
        }


        /// <summary>
        /// 获取机器人信息
        /// </summary>
        /// <param name="rebotId"></param>
        /// <returns></returns>
        public WechatRebot GetRebotInfo(int rebotId)
        {
            if (rebotId == 0) return null;
            return BDC.WechatRebot.Where(t => t.SiteID == SiteInfo.ID && t.ID == rebotId).FirstOrDefault();
        }

        /// <summary>
        /// 保存机器人
        /// </summary>
        /// <param name="rebot"></param>
        /// <returns></returns>
        public bool SaveRebot(WechatRebot rebot)
        {
            if (rebot.UserID == 0)
            {
                base.Message("用户名不存在");
                return false;
            }
            rebot.SiteID = SiteInfo.ID;
            User user = UserAgent.Instance().GetUserInfo(rebot.UserID);
            if (user == null || !user.IsTest)
            {
                base.Message("该用户没有标记测试账户");
                return false;
            }

            string key = UserAgent.Instance().GetTalkKey(user.IMID, rebot.Type);
            if (rebot.ID == 0)
            {
                if (rebot.Add(true))
                {
                    AdminInfo.Log(AdminLog.LogType.Wechat, "添加机器人：{0}", rebot.ToJson());
                    return true;
                }
            }
            else
            {
                if (rebot.Update() != 0)
                {
                    AdminInfo.Log(AdminLog.LogType.Wechat, "修改机器人：{0}", rebot.ToJson());
                    return true;
                }
            }
            return true;
        }

        /// <summary>
        /// 删除机器人
        /// </summary>
        /// <param name="rebotId"></param>
        /// <returns></returns>
        public bool DeleteRebot(int rebotId)
        {
            if (BDC.WechatRebot.Remove(t => t.SiteID == SiteInfo.ID && t.ID == rebotId) > 0)
            {
                AdminInfo.Log(AdminLog.LogType.Wechat, "删除机器人，编号：{0}", rebotId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取系统中所有状态为打开的机器人(非web程序）
        /// </summary>
        /// <returns></returns>
        public List<WechatRebot> GetRebotList()
        {
            return BDC.WechatRebot.Where(t => t.IsOpen).ToList();
        }

        /// <summary>
        /// 获取微信群的设置
        /// </summary>
        /// <returns></returns>
        public List<GroupSetting> GetGroupList()
        {
            List<GroupSetting> list = BDC.GroupSetting.Where(t => t.SiteID == SiteInfo.ID).ToList();

            foreach (ChatTalk.GroupType type in Enum.GetValues(typeof(ChatTalk.GroupType)))
            {
                if (!list.Exists(t => t.Type == type))
                {
                    list.Add(new GroupSetting(type));
                }
            }
            return list;
        }

        /// <summary>
        /// 获取微信群的设置
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GroupSetting GetGroupInfo(ChatTalk.GroupType type)
        {
            return BDC.GroupSetting.Where(t => t.Type == type && t.SiteID == SiteInfo.ID).FirstOrDefault() ?? new GroupSetting(type);
        }

        /// <summary>
        /// 更新群设置
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool UpdateGroupSetting(GroupSetting setting)
        {
            setting.SiteID = SiteInfo.ID;
            setting.SettingString = setting.Setting;

            if (setting.Exists())
            {
                return setting.Update() == 1;
            }
            else
            {
                return setting.Add();
            }
        }

        /// <summary>
        /// 获取所有的微信群设置（非web程序）
        /// </summary>
        /// <returns></returns>
        public List<GroupSetting> GetWechatGroupList()
        {
            return BDC.GroupSetting.ToList();
        }

        /// <summary>
        /// 获取微信授权登录的地址
        /// </summary>
        /// <returns></returns>
        public string GetLoginUrl()
        {
            WechatSetting wx = this.GetWechatSetting(SiteInfo.ID);

            string url = string.Format("{0}/wx/login.html", wx.Setting.Domain);

            return WX.GetAuthorizeUrl(wx.Setting.AppId, url, "login");
        }
    }
}
