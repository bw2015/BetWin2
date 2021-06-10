using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Fleck;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Logs;
using IMService.Common;
using SP.Studio.Web;

using SP.Studio.Data;

namespace IMService.Agent
{
    /// <summary>
    /// 客服逻辑
    /// </summary>
    partial class StaffAgent : AgentBase<StaffAgent>
    {
        /// <summary>
        /// 当前在线的客服
        /// </summary>
        private List<ChatUser> userlist = new List<ChatUser>();

        public int GetAdminID(Hashtable ht, out Guid session)
        {
            session = Guid.Empty;
            if (!ht.ContainsKey("session")) return 0;
            string guid = ht["session"].ToString();
            if (!Guid.TryParse(guid, out session)) return 0;
            return AdminAgent.Instance().GetAdminID(session);
        }

        /// <summary>
        /// 客服上线
        /// </summary>
        /// <param name="ht"></param>
        /// <param name="message"></param>
        /// <param name="socket"></param>
        public ChatUser StaffOnline(Hashtable ht, string message, IWebSocketConnection socket)
        {
            Guid session;
            int adminId = this.GetAdminID(ht, out session);
            if (adminId == 0)
            {
                this.Close(socket);
                this.SaveLog("上线错误，客服账号未授权。{0}", message);
                return null;
            }
            ChatUser user = this.AddUser(adminId, session, socket);
            this.SaveLog("客服{0}上线", AdminAgent.Instance().GetAdminName(user.UserID));
            AdminAgent.Instance().SetOnlineStatus(user.UserID, true);
            this.SendOnlineNotify(user.UserID, true);

            // 发送离线未读信息
            foreach (ChatLog log in AdminAgent.Instance().GetChatLogUnread(user.UserID))
            {
                this.Send(log);
            }
            return user;
        }

        /// <summary>
        /// 给所有在线会员发送客服在线或者离线的信息
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="online"></param>
        public void SendOnlineNotify(int adminId, bool online)
        {
            this.userlist.ForEach(t =>
            {
                t.Socket.Send("{\"action\":\"online\",\"id\":\"staff" + adminId + "\",\"online\":" + (online ? 1 : 0) + "}");
            });
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="log"></param>
        public void Send(ChatLog log)
        {
            foreach (ChatUser user in this.userlist.Where(t => t.UserID == log.UserID))
            {
                user.Send(log.ToString());
            }
            // do chatlog is reading
        }

        /// <summary>
        /// 添加一个在线用户进入缓存
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="session">当前用户的授权值</param>
        /// <param name="socket">当前连接</param>
        /// <returns></returns>
        public ChatUser AddUser(int userId, Guid session, IWebSocketConnection socket)
        {
            if (userId == 0) userId = UserAgent.Instance().GetUserID(session);
            if (userId == 0) return null;
            ChatUser user = this.userlist.Where(t => t.Socket == socket).FirstOrDefault();
            if (user != null) return user;

            user = new ChatUser(userId, session, socket);
            this.userlist.Add(user);
            return user;
        }

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="content"></param>
        public void SaveLog(string content, params object[] args)
        {
            using (DbExecutor db = NewExecutor())
            {
                new ChatErrorLog()
                {
                    SiteID = SiteInfo.ID,
                    CreateAt = DateTime.Now,
                    Content = string.Format(content, args)
                }.Add(db);
            }

            Console.WriteLine("[{0}] {1}", DateTime.Now, string.Format(content, args));
        }

        /// <summary>
        /// 发送一条关闭指令
        /// </summary>
        /// <param name="socket"></param>
        public void Close(IWebSocketConnection socket)
        {
            socket.Send("{\"action\":\"close\"}");
        }
    }
}
