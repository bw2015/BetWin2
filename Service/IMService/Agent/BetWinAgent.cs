using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Linq;
using SP.Studio.Data;
using System.Text.RegularExpressions;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Logs;

using SP.Studio.Core;

using IMService.Common;

using Fleck;

namespace IMService.Agent
{
    /// <summary>
    /// 与数据库打交道，避免逻辑库中的web缓存问题
    /// </summary>
    public class BetWinAgent : AgentBase<BetWinAgent>
    {
        public int SiteID { get; set; }

        public void Install()
        {
            if (this.userlist != null) return;

            this.userlist = new Dictionary<UserType, List<MessageUser>>();
            this.userlist.Add(UserType.User, new List<MessageUser>());
            this.userlist.Add(UserType.Staff, new List<MessageUser>());
        }

        /// <summary>
        /// 在线的socket对象
        /// </summary>
        private Dictionary<UserType, List<MessageUser>> userlist;

        #region ======= 数据操作方法 ========

        /// <summary>
        /// 保存日志
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public void SaveLog(string content, params object[] args)
        {
            content = string.Format(content, args);
            using (DbExecutor db = NewExecutor())
            {
                new ChatErrorLog()
                {
                    SiteID = this.SiteID,
                    Content = content,
                    CreateAt = DateTime.Now
                }.Add(db);
            }
            Console.WriteLine("[{0}]{1}", DateTime.Now, content);
        }

        /// <summary>
        /// 保存聊天记录
        /// </summary>
        /// <param name="log"></param>
        /// <returns>返回ID</returns>
        public int SaveChatLog(ChatLog log)
        {
            using (DbExecutor db = NewExecutor())
            {
                log.SiteID = this.SiteID;
                log.Add(true, db);
                return log.ID;
            }
        }

        /// <summary>
        /// 用户ID的缓存
        /// </summary>
        private Dictionary<string, int> _userid = new Dictionary<string, int>();
        /// <summary>
        /// 从信息结构中获取用户ID
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private int GetID(Message msg)
        {
            string key = string.Concat(msg.Type, msg.Session.ToString("N"));
            if (_userid.ContainsKey(key)) return _userid[key];

            int? userId = null;
            switch (msg.Type)
            {
                case UserType.User:
                    userId = BDC.UserSession.Where(t => t.SiteID == this.SiteID && t.Session == msg.Session).Select(t => (int?)t.UserID).FirstOrDefault();
                    break;
                case UserType.Staff:
                    userId = BDC.AdminSession.Where(t => t.SiteID == this.SiteID && t.Session == msg.Session).Select(t => (int?)t.AdminID).FirstOrDefault();
                    break;
            }
            if (userId == null) return 0;

            _userid.Add(key, userId.Value);
            return userId.Value;
        }

        /// <summary>
        /// 用户昵称的缓存
        /// </summary>
        private Dictionary<string, string> _username = new Dictionary<string, string>();
        /// <summary>
        /// 根据类型获取用户的名字
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetName(UserType type, int userId)
        {
            string key = string.Concat(type, userId);
            if (_username.ContainsKey(key)) return _username[key];

            string name = null;
            switch (type)
            {
                case UserType.User:
                    name = UserAgent.Instance().GetUserName(userId);
                    break;
                case UserType.Staff:
                    name = BDC.Admin.Where(t => t.SiteID == this.SiteID && t.ID == userId).Select(t => new { t.AdminName, t.NickName }).ToList().Select(t => string.IsNullOrEmpty(t.NickName) ? t.AdminName : t.NickName).FirstOrDefault();
                    break;
            }
            this._username.Add(key, name);
            return name;
        }

        private Dictionary<string, string> _face = new Dictionary<string, string>();
        /// <summary>
        /// 获取用户的头像
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetFace(UserType type, int userId)
        {
            string key = string.Concat(type, userId);
            if (_face.ContainsKey(key)) return _face[key];

            string face = null;
            switch (type)
            {
                case UserType.User:
                    face = BDC.User.Where(t => t.SiteID == this.SiteID && t.ID == userId).Select(t => t.Face).FirstOrDefault();
                    if (string.IsNullOrEmpty(face)) face = "/images/user.png";
                    break;
                case UserType.Staff:
                    face = BDC.Admin.Where(t => t.SiteID == this.SiteID && t.ID == userId).Select(t => t.Face).FirstOrDefault();
                    if (string.IsNullOrEmpty(face)) face = "/images/staff.png";
                    break;
            }
            if (string.IsNullOrEmpty(face)) return null;
            face = BW.Framework.SysSetting.GetSetting().imgServer + face;
            this._face.Add(key, face);
            return face;
        }

        /// <summary>
        /// 更新用户的在线状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <param name="online"></param>
        public void UpdateOnlineStatus(UserType type, int userId, bool online)
        {
            using (DbExecutor db = NewExecutor())
            {
                string table = null;
                string field = null;
                switch (type)
                {
                    case UserType.User:
                        table = "Users";
                        field = "UserID";
                        break;
                    case UserType.Staff:
                        table = "site_Admin";
                        field = "AdminID";
                        break;
                }
                if (!string.IsNullOrEmpty(table))
                {
                    if (db.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE {0} SET IsOnline = @Online WHERE SiteID = @SiteID AND {1} = @UserID AND IsOnline != @Online", table, field),
                         NewParam("@SiteID", this.SiteID),
                         NewParam("@UserID", userId),
                         NewParam("@Online", online)) != 0)
                    {

                        Hashtable ht = new Hashtable();
                        ht.Add("UserID", userId);
                        string key = string.Concat(type, userId);
                        switch (type)
                        {
                            // 用户上线
                            case UserType.User:
                                foreach (MessageUser chatUser in this.userlist[UserType.User].Where(t => this.GetUserFriends(userId).Contains(t.UserID)))
                                {
                                    chatUser.Send(new Message(type, online ? ActionType.Online : ActionType.Offline, ht));
                                }
                                break;
                            // 管理员上线
                            case UserType.Staff:
                                foreach (MessageUser chatUser in this.userlist[UserType.User])
                                {
                                    chatUser.Send(new Message(type, online ? ActionType.Online : ActionType.Offline, ht));
                                }
                                break;
                        }
                        if (!online)
                        {
                            if (this._face.ContainsKey(key))
                                this._face.Remove(key);
                            if (this._username.ContainsKey(key))
                                this._username.Remove(key);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 获取用户的上级和下级所有的好友ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetUserFriends(int userId)
        {
            foreach (int userid in BDC.User.Where(t => t.SiteID == this.SiteID && (t.AgentID == userId || BDC.User.Where(p => p.ID == userId).Select(p => p.AgentID).Contains(t.ID))).Select(t => t.ID))
                yield return userid;
        }

        #endregion

        /// <summary>
        /// 用户上线
        /// </summary>
        /// <param name="msg">信息对象</param>
        /// <returns></returns>
        public MessageUser Online(Message msg, IWebSocketConnection socket)
        {
            int userId = this.GetID(msg);
            if (userId == 0) return null;
            List<MessageUser> list = this.userlist[msg.Type];

            //MessageUser user = list.Find(t => t.UserID == userId && t.Socket == socket);
            //if (user != null) return user;
            MessageUser user = new MessageUser(userId, socket);
            list.Add(user);
            this.SaveLog(string.Format("{0}-{1} 上线", msg.Type.GetDescription(), this.GetName(msg.Type, userId)));
            this.SendUnReadMessage(msg.Type, user);

            this.UpdateOnlineStatus(msg.Type, user.UserID, true);

            return user;
        }

        /// <summary>
        /// 用户上线的时候发送全部未读信息
        /// </summary>
        /// <param name="type">用户类型</param>
        /// <param name="user">用户名</param>
        private void SendUnReadMessage(UserType type, MessageUser user)
        {
            var list = BDC.ChatLog.Where(t => t.SiteID == this.SiteID && !t.IsRead);
            switch (type)
            {
                case UserType.User:
                    list = list.Where(t => t.UserID == user.UserID);
                    break;
                case UserType.Staff:
                    list = list.Where(t => t.UserID == 0 && t.AdminID == user.UserID);
                    break;
            }
            foreach (ChatLog log in list.OrderBy(t => t.ID))
            {
                Console.WriteLine("发现未读信息：{0}", log.ID);
                user.Send(new Message(log));
            }
        }

        /// <summary>
        /// 获取在线的用户对象 userid和adminid不能同时赋值
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private List<MessageUser> GetUserList(int userId, int adminId)
        {
            if (userId != 0) return this.userlist[UserType.User].FindAll(t => t.UserID == userId);
            if (adminId != 0) return this.userlist[UserType.Staff].FindAll(t => t.UserID == adminId);
            return new List<MessageUser>();
        }

        /// <summary>
        /// 接受信息 ，并且转发
        /// </summary>
        /// <param name="msg">发出的信息 接收者to 信息内容content</param>
        public void Receive(Message msg)
        {
            // 信息的接收者
            string to = msg.data["to"].ToString();
            if (to == "group0")
            {
                this.RecevieGroup(msg);
                return;
            }

            int sendId = this.GetID(msg);
            int adminId = 0;
            int userId = 0;


            Regex regex = new Regex(@"staff(?<ID>\d+)");
            if (regex.IsMatch(to))
            {
                // 接收者是管理员
                adminId = int.Parse(regex.Match(to).Groups["ID"].Value);
            }
            else
            {
                // 接收者是用户
                userId = int.Parse(to);
                // 如果是管理员发出则标记管理员ID
                if (msg.Type == UserType.Staff)
                {
                    adminId = sendId;
                    sendId = 0;
                }
            }

            ChatLog log = new ChatLog(userId, adminId, sendId, msg.data["content"].ToString());

            int lodId = this.SaveChatLog(log);

            this.GetUserList(userId, adminId).ForEach(t =>
            {
                t.Send(new Message(log));
            });
        }

        /// <summary>
        /// 发送管理员内部沟通的信息
        /// </summary>
        /// <param name="msg"></param>
        private void RecevieGroup(Message msg)
        {
            int sendId = this.GetID(msg);
            ChatLog log = new ChatLog(0, sendId, 0, msg.data["content"].ToString());
            foreach (MessageUser user in this.userlist[UserType.Staff].Where(t => t.UserID != sendId))
            {
                msg = new Message(log);
                msg.data["User"] = "group0";
                user.Send(msg);
            }
        }

        /// <summary>
        /// 用户主动下线触发
        /// </summary>
        /// <param name="user"></param>
        public void Close(MessageUser user)
        {
            UserType type = UserType.None;

            if (this.userlist[UserType.User].Contains(user))
            {
                type = UserType.User;
            }
            else if (this.userlist[UserType.Staff].Contains(user))
            {
                type = UserType.Staff;
            }
            if (type == UserType.None) return;

            this.userlist[type].Remove(user);
            this.UpdateOnlineStatus(type, user.UserID, this.userlist[type].Exists(t => t.UserID == user.UserID));
            this.SaveLog("{0}-{1} 下线", type.GetDescription(), this.GetName(type, user.UserID));
        }

        /// <summary>
        /// 设置为已读
        /// </summary>
        /// <param name="msg"></param>
        public void Read(Message msg, MessageUser user)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE usr_ChatLog SET IsRead = 1 WHERE SiteID = @SiteID AND LogID = @LogID AND IsRead = 0 AND {0}",
                    msg.Type == UserType.User ? "UserID = @UserID" : "AdminID = @UserID AND UserID = 0"),
                    NewParam("@SiteID", this.SiteID),
                    NewParam("@LogID", msg.data["LogID"]),
                    NewParam("@UserID", user.UserID));
            }
        }


        #region ========  命令行触发  ============

        /// <summary>
        /// 查看当前用户列表
        /// </summary>
        public void RunUserList()
        {
            foreach (UserType type in this.userlist.Select(t => t.Key))
            {
                Console.WriteLine("{0}在线{1}人", type.GetDescription(), this.userlist[type].Count);

                foreach (MessageUser user in this.userlist[type])
                {
                    Console.WriteLine("[{0}]\tServer:{1}", this.GetName(type, user.UserID), user.SocketInfo());
                }
            }
        }


        #endregion
    }
}
