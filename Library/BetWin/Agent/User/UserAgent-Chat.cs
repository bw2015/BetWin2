using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using SP.Studio.Data;
using SP.Studio.Security;
using BW.Common.Users;
using System.Net;
using System.Net.WebSockets;

namespace BW.Agent
{
    /// <summary>
    /// 站内聊天
    /// </summary>
    partial class UserAgent
    {
        private Regex chatTypeRegex = new Regex(@"(?<Type>ADMIN|USER|GUEST|GROUP)\-");

        private Regex chatUserRegex = new Regex(@"(?<Type>ADMIN|USER|GUEST|GROUP)\-(?<UserID>\d+)$");

        /// <summary>
        /// 用户的标识前缀
        /// </summary>
        public const string IM_USER = "USER";

        /// <summary>
        /// 管理员的标识前缀
        /// </summary>
        public const string IM_ADMIN = "ADMIN";

        /// <summary>
        /// 客服的标识
        /// </summary>
        public const string IM_ADMIN_SERVICE = "ADMIN-0";

        /// <summary>
        /// 游客的标识前缀
        /// </summary>
        public const string IM_GUEST = "GUEST";

        /// <summary>
        /// 群聊的前缀
        /// </summary>
        public const string IM_GROUP = "GROUP";

        /// <summary>
        /// 从聊天标识中获取用户ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetChatUserID(string id)
        {
            if (!chatUserRegex.IsMatch(id)) return 0;
            return int.Parse(chatUserRegex.Match(id).Groups["UserID"].Value);
        }

        /// <summary>
        /// 获取用户类型以及ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetChatUserID(string id, out string type)
        {
            type = null;
            if (!chatUserRegex.IsMatch(id)) return 0;
            type = chatUserRegex.Match(id).Groups["Type"].Value;
            return int.Parse(chatUserRegex.Match(id).Groups["UserID"].Value);
        }

        /// <summary>
        /// 获取用户类型
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GetTalkType(string user)
        {
            if (!chatTypeRegex.IsMatch(user)) return null;
            return chatTypeRegex.Match(user).Groups["Type"].Value;
        }

        /// <summary>
        /// 获取两个用户的对话类型
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <returns></returns>
        public ChatTalk.TalkType GetTalkType(string user1, string user2)
        {
            string key = string.Join("-", new string[] { user1, user2 }.OrderBy(t => t).Select(t => this.GetTalkType(t)));

            ChatTalk.TalkType type = ChatTalk.TalkType.None;
            switch (key)
            {
                case "ADMIN-USER":
                    type = ChatTalk.TalkType.AdminUser;
                    break;
                case "ADMIN-GUEST":
                    type = ChatTalk.TalkType.AdminGuest;
                    break;
                case "USER-USER":
                    type = ChatTalk.TalkType.User2User;
                    break;
                case "ADMIN-ADMIN":
                    type = ChatTalk.TalkType.Admin2Admin;
                    break;
                case "ADMIN-GROUP":
                    type = ChatTalk.TalkType.Group;
                    break;
            }
            return type;
        }

        private Dictionary<string, int> _talkSiteID = new Dictionary<string, int>();

        /// <summary>
        /// 根据会话值获取站点ID（适用于非web程序）
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetSiteIDByTalkKey(string key, DbExecutor db)
        {
            if (_talkSiteID.ContainsKey(key)) return this._talkSiteID[key];
            int siteId = (int)db.ExecuteScalar(CommandType.Text, "SELECT SiteID FROM usr_ChatTalk WHERE TalkKey = @Key",
                NewParam("@Key", key));
            this._talkSiteID.Add(key, siteId);
            return siteId;
        }

        /// <summary>
        /// 保存聊天记录（适用于非web程序）
        /// </summary>
        /// <param name="log">返回记录</param>
        public int SaveChatLog(ChatLog log)
        {
            if (string.IsNullOrEmpty(log.Content))
            {
                base.Message("内容不能为空");
                return 0;
            }

            using (DbExecutor db = NewExecutor())
            {
                ChatTalk talk = new ChatTalk() { Key = log.Key }.Info(db);

                if (this.GetTalkType(log.SendID) == IM_ADMIN)
                {
                    log.UserID = (talk.User1 == log.SendID || talk.User1 == "ADMIN-0") ? talk.User2 : talk.User1;
                }
                else
                {
                    log.UserID = talk.User1 == log.SendID ? talk.User2 : talk.User1;
                }
                log.SiteID = talk.SiteID;
                log.CreateAt = DateTime.Now;
                log.Add(true, db);

                talk.Count++;
                talk.LastAt = DateTime.Now;
                talk.Update(db, t => t.Count, t => t.LastAt);

                return log.ID;
            }
        }

        /// <summary>
        /// 根据发送人的信息获取接收者的信息
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sendId"></param>
        /// <returns></returns>
        public string GetChatUserID(string key, string sendId)
        {
            ChatTalk talk = new ChatTalk() { Key = key }.Info();
            if (talk == null) return null;
            if (talk.User1 == sendId) return talk.User2;
            if (talk.User2 == sendId) return talk.User1;
            return null;
        }

        /// <summary>
        /// 获取未读的信息（管理员、用户、游客均可使用该方法）
        /// </summary>
        /// <param name="userId">信息接收者ID</param>
        /// <returns></returns>
        public List<ChatLog> GetChatLogUnread(int siteId, string userId)
        {
            IQueryable<ChatLog> list = BDC.ChatLog.Where(t => t.SiteID == siteId && !t.IsRead);
            bool service = false;
            string type = this.GetTalkType(userId);
            switch (type)
            {
                case IM_GUEST:
                case IM_USER:
                    list = list.Where(t => t.UserID == userId);
                    break;
                case IM_ADMIN:
                    if (AdminAgent.Instance().GetServiceAdmin(siteId).Select(t => string.Concat(IM_ADMIN, "-", t)).Contains(userId))
                    {
                        list = list.Where(t => (t.UserID == userId || t.UserID == "ADMIN-0"));
                        service = true;
                    }
                    else
                    {
                        list = list.Where(t => t.UserID == userId);
                    }
                    break;
            }
            List<ChatLog> chatLog = list.OrderBy(t => t.ID).ToList();
            if (service)
            {
                using (DbExecutor db = NewExecutor())
                {
                    foreach (ChatLog log in chatLog.Where(t => t.UserID == string.Concat(IM_ADMIN, "-", 0)))
                    {
                        log.UserID = userId;
                        log.Update(db, t => t.UserID);
                    }
                }
            }
            return chatLog;
        }



        /// <summary>
        /// 设置信息为已读
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="userId">信息的接收者</param>
        /// <returns></returns>
        public void UpdateChatLogRead(int logId, string userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text,
                    string.Format("UPDATE {0} SET IsRead = 1 WHERE LogID = @LogID AND UserID = @UserID", typeof(ChatLog).GetTableName()),
                    NewParam("@LogID", logId),
                    NewParam("@UserID", userId));
            }
        }

        /// <summary>
        /// 批量设置信息为已读
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="logs"></param>
        public void UpdateChatLogRead(string userId, params int[] logs)
        {
            if (logs.Length == 0) return;
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text,
                    string.Format("UPDATE {0} SET IsRead = 1 WHERE LogID IN ({1}) AND UserID = @UserID AND IsRead = 0", typeof(ChatLog).GetTableName(), string.Join(",", logs)),
                    NewParam("@UserID", userId));
            }
        }

        /// <summary>
        /// 批量设置为已读
        /// </summary>
        /// <param name="logs"></param>
        public void UpdateChatLogRead(params int[] logs)
        {
            if (logs.Length == 0) return;
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text,
                    string.Format("UPDATE {0} SET IsRead = 1 WHERE LogID IN ({1}) AND SiteID = @SiteID AND IsRead = 0", typeof(ChatLog).GetTableName(), string.Join(",", logs)),
                    NewParam("@SiteID", SiteInfo.ID));
            }
        }

        /// <summary>
        /// 当前会话KEY是否已经写入数据库
        /// </summary>
        private Dictionary<string, bool> _talkKey = new Dictionary<string, bool>();

        /// <summary>
        /// 获取会话KEY（没有则新建）
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <returns></returns>
        public string GetTalkKey(string user1, string user2)
        {
            if (user1 == user2 || string.IsNullOrEmpty(user1) || string.IsNullOrEmpty(user2)) return null;

            string[] user = new string[] { user1, user2 }.OrderBy(t => t).ToArray();
            user1 = user[0];
            user2 = user[1];

            string key = MD5.toMD5(string.Join(",", user));
            if (_talkKey.ContainsKey(key)) return key;

            if (!new ChatTalk() { Key = key }.Exists())
            {
                new ChatTalk()
                {
                    Count = 0,
                    Key = key,
                    SiteID = SiteInfo.ID,
                    User1 = user1,
                    User2 = user2,
                    Type = this.GetTalkType(user1, user2)
                }.Add();
            }
            _talkKey.Add(key, true);
            return key;
        }

        /// <summary>
        /// 获取群聊的会话KEY（没有则新建）
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTalkKey(string user1, ChatTalk.GroupType type)
        {
            string user2 = string.Concat(UserAgent.IM_GROUP, "-", (byte)type);
            return this.GetTalkKey(user1, user2);
        }

        /// <summary>
        /// 获取用户的上下级
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<int> GetFriends(int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetFriend",
                    NewParam("@UserID", userId));
                return ds.ToList<int>();
            }
        }

        /// <summary>
        /// 修改信息的接收人（只有发给客服的信息才能修改）
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool UpdateChatLogUserID(int logId, string userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.ExecuteNonQuery(CommandType.Text, "UPDATE usr_ChatLog SET UserID = @UserID WHERE LogID = @LogID AND IsRead = 0 AND UserID = 'ADMIN-0'",
                     NewParam("@UserID", userId),
                     NewParam("@LogID", logId)) == 1;
            }
        }

        #region ============== WebSocket服务端交互  ================

        /// <summary>
        /// 获取所有在线用户的未读信息
        /// </summary>
        /// <returns></returns>
        public List<ChatLog> GetChatLogUnread()
        {
            List<ChatLog> list = BDC.ChatLog.Where(t => !t.IsRead).OrderBy(t => t.CreateAt).ToList();
            return list;
        }

        /// <summary>
        /// 获取指定用户的未读消息数量
        /// </summary>
        /// <param name="users"></param>
        /// <returns></returns>
        public IEnumerable<ChatLog> GetChatLogUnread(params string[] users)
        {
            List<string> userid = users.ToList();
            foreach (ChatLog log in BDC.ChatLog.Where(t => !t.IsRead && t.SiteID == SiteInfo.ID && userid.Contains(t.UserID)).OrderBy(t => t.CreateAt))
            {
                yield return log;
            }
        }

        #endregion
    }
}
