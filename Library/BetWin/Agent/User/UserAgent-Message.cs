using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Data;
using SP.Studio.Web;
using BW.Common.Admins;

using BW.Common.Users;
using SP.Studio.Data.Linq;

namespace BW.Agent
{
    /// <summary>
    /// 站内信
    /// </summary>
    partial class UserAgent
    {
        /// <summary>
        /// 系统发送站内信给用户
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SendMessage(int userId, string title, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                base.Message("未输入内容");
                return false;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = WebAgent.Left(content, 20);
            }
            using (DbExecutor db = NewExecutor())
            {
                if (new UserMessage()
                {
                    SiteID = this.GetSiteID(userId, db),
                    UserID = userId,
                    Title = title,
                    Content = content,
                    CreateAt = DateTime.Now
                }.Add(db))
                {
                    if (AdminInfo != null) AdminInfo.Log(AdminLog.LogType.User, "给用户{0}发送站内信，内容：{1}", UserAgent.Instance().GetUserName(userId), WebAgent.Left(content, 100));
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取短信
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public UserMessage GetMessageInfo(int msgId)
        {
            return BDC.UserMessage.Where(t => t.SiteID == SiteInfo.ID && t.ID == msgId).FirstOrDefault();
        }

        /// <summary>
        /// 设定短信信息为已读
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public bool MessageRead(int msgId)
        {
            return BDC.UserMessage.Update(new UserMessage() { IsRead = true, ReadAt = DateTime.Now }, t => t.SiteID == SiteInfo.ID && t.ID == msgId && !t.IsRead, t => t.IsRead, t => t.ReadAt) != 0;
        }


        /// <summary>
        /// 删除站内信
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public bool MessageDelete(int msgId)
        {
            UserMessage message = this.GetMessageInfo(msgId);
            if (message == null || message.UserID != UserInfo.ID)
            {
                base.Message("编号错误");
                return false;
            }

            if (message.Delete() > 0)
            {
                UserInfo.Log("删除站内信{0}", message.Title);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取用户未读的信息数量
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetNewMessage(int userId)
        {
            return BDC.UserMessage.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && !t.IsRead && t.CreateAt > SiteInfo.StartDate).Count();
        }

        /// <summary>
        /// 给所有在线会员发送站内信（适用于非web程序）
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public int SendMessageOnline(int siteId, string title, string content)
        {
            int count = 0;
            foreach (int userId in BDC.User.Where(t => t.SiteID == siteId && t.IsOnline).Select(t => t.ID))
            {
                if (this.SendMessage(userId, title, content)) count++;
            }
            return count;
        }
    }
}
