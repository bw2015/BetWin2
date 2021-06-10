using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using BW.Agent;
using BW.Common.Wechat;
using BW.Common.Users;

using SP.Studio.Core;

using SP.Studio.Web;
using SP.Studio.Model;
using BW.Common.Permission;
namespace BW.Handler.admin
{
    public class wechat : IHandler
    {
        /// <summary>
        /// 获取当前的配置信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.公共号设置.Value)]
        private void getsetting(HttpContext context)
        {
            WechatSetting setting = WechatAgent.Instance().GetWechatSetting(SiteInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                setting.Name,
                setting.FaceShow,
                Setting = new JsonString(setting.Setting.ToJson())
            });
        }

        /// <summary>
        /// 保存微信参数设置
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.公共号设置.Value)]
        private void savesetting(HttpContext context)
        {
            WechatSetting setting = WechatAgent.Instance().GetWechatSetting(SiteInfo.ID);
            setting = context.Request.Form.Fill(setting);
            setting.Setting = context.Request.Form.Fill(setting.Setting, "Setting");

            this.ShowResult(context, WechatAgent.Instance().SaveSetting(setting, context.Request.Files[0]), "保存成功");
        }

        /// <summary>
        /// 保存机器人配置信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.机器人设置.Value)]
        private void saverebotinfo(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            WechatRebot rebot = WechatAgent.Instance().GetRebotInfo(QF("ID", 0)) ?? new WechatRebot();
            rebot.UserID = userId;
            rebot = context.Request.Form.Fill(rebot);
            rebot.Setting = context.Request.Form.Fill(rebot.Setting, "Setting");

            this.ShowResult(context, WechatAgent.Instance().SaveRebot(rebot), "保存成功");
        }

        /// <summary>
        /// 获取机器人信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.机器人设置.Value)]
        private void getrebotinfo(HttpContext context)
        {
            WechatRebot rebot = WechatAgent.Instance().GetRebotInfo(QF("ID", 0)) ?? new WechatRebot();
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                rebot.ID,
                UserName = UserAgent.Instance().GetUserName(rebot.UserID),
                rebot.IsOpen,
                rebot.Type,
                Setting = new JsonString(rebot.Setting.ToJson())
            });
        }

        /// <summary>
        /// 机器人列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.机器人设置.Value)]
        private void getrebotlist(HttpContext context)
        {
            IQueryable<WechatRebot> list = BDC.WechatRebot.Where(t => t.SiteID == SiteInfo.ID);

            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<ChatTalk.GroupType>());
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Money = UserAgent.Instance().GetUserMoney(t.UserID),
                t.IsOpen
            }));
        }

        /// <summary>
        /// 删除机器人
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.机器人设置.Value)]
        private void deleterebot(HttpContext context)
        {
            this.ShowResult(context, WechatAgent.Instance().DeleteRebot(QF("ID", 0)), "删除成功");
        }


        /// <summary>
        /// 微信群列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.微信群设置.Value)]
        private void grouplist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(WechatAgent.Instance().GetGroupList(), t => new
            {
                Type = t.Type,
                Name = t.Type.GetDescription(),
                Setting = new JsonString(t.Setting.ToJson())
            }));
        }

        /// <summary>
        /// 微信群设置修改
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.微信设置.微信参数.微信群设置.Value)]
        private void updategroupsetting(HttpContext context)
        {
            ChatTalk.GroupType type = QF("Type").ToEnum<ChatTalk.GroupType>();

            GroupSetting setting = WechatAgent.Instance().GetGroupInfo(type);

            switch (QF("Name"))
            {
                case "Setting.BetMessage":
                    setting.Setting.BetMessage = QF("Value", 0) == 1;
                    break;
                case "Setting.Chat":
                    setting.Setting.Chat = QF("Value", 0) == 1;
                    break;
            }

            this.ShowResult(context, WechatAgent.Instance().UpdateGroupSetting(setting), "保存成功");
        }

        /// <summary>
        /// 获取登录地址
        /// </summary>
        /// <param name="context"></param>
        private void getloginurl(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Url = WechatAgent.Instance().GetLoginUrl()
            });
        }
    }
}
