using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Web;

using SP.Studio.Data;
using SP.Studio.Web;
using SP.Studio.Array;

using BW.IM.Common;

namespace BW.IM.Agent
{
    public sealed class UserAgent : AgentBase<UserAgent>
    {
        /// <summary>
        /// 获取当前登录的用户
        /// </summary>
        /// <returns></returns>
        public User GetUserInfo(HttpContext context)
        {
            Guid guid;
            Regex regex = new Regex("user/(?<Guid>[a-f0-9]{32})$", RegexOptions.IgnoreCase);
            if (regex.IsMatch(context.Request.RawUrl))
            {
                if (!Guid.TryParse(regex.Match(context.Request.RawUrl).Groups["Guid"].Value, out guid)) return null;
            }
            else
            {
                guid = WebAgent.GetParam("_auth_user", Guid.Empty);
                if (guid == Guid.Empty) return null;
            }
            return this.GetUserInfo(guid);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public User GetUserInfo(Guid session)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetUserInfo",
                    NewParam("@Session", session));

                if (ds.Tables[0].Rows.Count == 0) return null;
                return new User(ds.Tables[0].Rows[0], UserType.USER);
            }
        }

        /// <summary>
        /// 获取当前登录的管理员
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public User GetAdminInfo(HttpContext context)
        {
            Guid guid;
            Regex regex = new Regex("admin/(?<Guid>[a-f0-9]{32})$", RegexOptions.IgnoreCase);
            if (regex.IsMatch(context.Request.RawUrl))
            {
                if (!Guid.TryParse(regex.Match(context.Request.RawUrl).Groups["Guid"].Value, out guid)) return null;
            }
            else
            {
                guid = WebAgent.QF("_auth_admin", Guid.Empty);
                if (guid == Guid.Empty) return null;
            }
            return this.GetAdminInfo(guid);
        }

        /// <summary>
        /// 获取管理员信息
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public User GetAdminInfo(Guid session)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetAdminInfo",
                    NewParam("@Session", session));
                if (ds.Tables[0].Rows.Count == 0) return null;
                return new User(ds.Tables[0].Rows[0], UserType.ADMIN);
            }
        }

        /// <summary>
        /// 获取用户的好友
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>好友的用户ID</returns>
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
        /// 设置用户上线或者下线
        /// </summary>
        /// <param name="userId"></param>
        public void SetUserOnlineStatus(int userId, bool online)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET IsOnline = @Online,ActiveAt = GETDATE() WHERE UserID = @UserID",
                    NewParam("@Online", online),
                    NewParam("@UserID", userId));
            }
        }

        /// <summary>
        /// 设定管理员上线或者下线
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="online"></param>
        public void SetAdminOnlineStatus(int adminId, bool online)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE site_Admin SET IsOnline = @Online WHERE AdminID = @AdminID AND IsOnline != @Online",
                    NewParam("@Online", online),
                    NewParam("@AdminID", adminId));
            }
        }

        /// <summary>
        /// 设置用户的在线状态
        /// </summary>
        /// <param name="user"></param>
        public void SetOnlineStatus(User user)
        {
            user.Error = 0;
            user.ActiveAt = DateTime.Now;
            switch (user.Type)
            {
                case UserType.USER:
                    this.SetUserOnlineStatus(user.ID, true);
                    break;
                case UserType.ADMIN:
                    this.SetAdminOnlineStatus(user.ID, true);
                    break;
            }
        }

        /// <summary>
        /// 获取未读信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<ChatLog> GetChatLogUnread(string user)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT * FROM usr_ChatLog WHERE UserID = @UserID AND IsRead = 0 ORDER BY CreateAt ASC",
                    NewParam("@UserID", user));

                return ds.ToList<ChatLog>();
            }
        }

        /// <summary>
        /// 获取在线客服管理员列表
        /// </summary>
        /// <returns></returns>
        public List<User> GetServiceList(int siteId)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetService",
                    NewParam("@SiteID", siteId));

                List<User> list = new List<User>();

                CustomerService customer = Utils.CUSTOMERSERVICE.Get(siteId, null);
                if (customer != null && customer.IsOpen)
                {
                    list.Add(new User(siteId, 0, Guid.Empty, customer.Name, customer.Sign, customer.FaceShow, UserType.ADMIN, GroupType.None, true));
                    return list;
                }

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new User(dr, UserType.ADMIN)
                    {
                        IsService = true
                    });
                }
                return list;
            }
        }

        /// <summary>
        /// 获取客服的在线状态（更新至缓存）
        /// </summary>
        public void GetServiceStatus()
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetServiceStatus");
                List<int> adminlist = new List<int>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int siteId = (int)dr["SiteID"];
                    int adminID = (int)dr["AdminID"];
                    string key = UserType.ADMIN + "-" + adminID;
                    bool isOnline = (bool)dr["IsOnline"] && Utils.SOCKETLIST.ContainsKey(key);

                    if (!Utils.SERVICELIST.ContainsKey(siteId)) Utils.SERVICELIST.Add(siteId, new Dictionary<int, bool>());

                    if (!Utils.SERVICELIST[siteId].ContainsKey(adminID))
                    {
                        Utils.SERVICELIST[siteId].Add(adminID, isOnline);
                    }
                    else if (Utils.SERVICELIST[siteId][adminID] != isOnline)
                    {
                        Utils.SERVICELIST[siteId][adminID] = isOnline;
                    }

                    adminlist.Add(adminID);
                }
                foreach (int sid in Utils.SERVICELIST.Keys.ToArray())
                {
                    foreach (int uid in Utils.SERVICELIST[sid].Keys.ToArray())
                    {
                        if (!adminlist.Contains(uid))
                        {
                            Utils.SERVICELIST[sid].Remove(uid);
                        }
                    }
                }

                foreach (DataRow dr in ds.Tables[1].Rows)
                {
                    int siteId = (int)dr["SiteID"];
                    string rebot = (string)dr["Rebot"];


                    if (!Utils.CUSTOMERSERVICE.ContainsKey(siteId))
                    {
                        Utils.CUSTOMERSERVICE.Add(siteId, new CustomerService(rebot));
                    }
                    else
                    {
                        Utils.CUSTOMERSERVICE[siteId] = new CustomerService(rebot);
                    }
                }

            }
        }

        /// <summary>
        /// 挑选出一个在线的客服
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetServiceID(int userId, int siteId)
        {
            if (Utils.SERVICELIST == null || !Utils.SERVICELIST.ContainsKey(siteId) || Utils.SERVICELIST[siteId].Count == 0)
            {
                return 0;
            }

            int adminId = Utils.USERSERVICE.Get(userId, 0);
            int lastAdminId = adminId;
            //#1 如果存在上次聊天的客服且仍然在线，则直接返回
            if (adminId != 0)
            {
                if (!Utils.SERVICELIST[siteId].ContainsKey(adminId))
                {
                    adminId = 0;
                }
                else if (Utils.SERVICELIST[siteId][adminId])
                {
                    return adminId;
                }
            }

            //#2 随机找出一个在线的客服
            int? randomId = Utils.SERVICELIST[siteId].Where(t => t.Value).OrderBy(t => Guid.NewGuid()).Select(t => (int?)t.Key).FirstOrDefault();
            //#2.1 存在在线客服
            if (randomId != null)
            {
                adminId = randomId.Value;
                this.SaveUserService(userId, adminId);
                return adminId;
            }

            //#2.2 不存在在线客服则留言给上次交流的客服
            if (adminId != 0) return adminId;

            randomId = Utils.SERVICELIST[siteId].OrderBy(t => Guid.NewGuid()).Select(t => (int?)t.Key).FirstOrDefault();
            adminId = randomId.Value;
            this.SaveUserService(userId, adminId);
            return adminId;
        }

        /// <summary>
        /// 保存用户上次聊天的客服ID
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="adminId"></param>
        private void SaveUserService(int userId, int adminId)
        {
            if (userId == 0 || adminId == 0) return;

            if (!Utils.USERSERVICE.ContainsKey(userId))
            {
                Utils.USERSERVICE.Add(userId, adminId);
            }
            else if (Utils.USERSERVICE[userId] != adminId)
            {
                Utils.USERSERVICE[userId] = adminId;
            }
        }

        /// <summary>
        /// 获取用户的上级
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetParent(int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetParent",
                    NewParam("@UserID", userId));
                if (ds.Tables[0].Rows.Count == 0) return null;
                return new User(ds.Tables[0].Rows[0], UserType.USER)
                {
                    Name = "我的上级"
                };
            }
        }

        /// <summary>
        /// 获取下级
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<User> GetChildList(int userId)
        {
            string key = string.Concat("GetChildList-", userId);

            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetChild",
                    NewParam("@UserID", userId));
                List<User> list = new List<User>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new User(dr, UserType.USER));
                }
                return list;
            }
        }

        /// <summary>
        /// 保存会话KEY值
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public void SaveTalkKey(string user1, string user2, string key, TalkType type, int siteId = 0)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.StoredProcedure, "IM_SaveTalkKey",
                    NewParam("@SiteID", siteId == 0 ? UserInfo.SiteID : siteId),
                    NewParam("@User1", user1),
                    NewParam("@User2", user2),
                    NewParam("@Key", key),
                    NewParam("@Type", type));
            }
        }

        /// <summary>
        /// 根据对话ID获取对方的IM Key
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GetTalkKey(string id, string user)
        {
            DataSet ds;
            using (DbExecutor db = NewExecutor())
            {
                ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetTalkKey",
                    NewParam("@ID", id));
                if (ds.Tables[0].Rows.Count == 0) return null;
            }
            IEnumerable<string> users = new string[] { (string)ds.Tables[0].Rows[0]["User1"], (string)ds.Tables[0].Rows[0]["User2"] }.Where(t => !string.IsNullOrEmpty(t));
            return users.Where(t => t != user).FirstOrDefault();
        }

        /// <summary>
        /// 保存信息进入数据库
        /// </summary>
        /// <param name="sendId"></param>
        /// <param name="key">会话Key值</param>
        /// <param name="sendName"></param>
        /// <param name="sendAvatar"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public int SaveMessage(string sendId, string key, string sendName, string sendAvatar, string content, out string userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                DbParameter msgId = NewParam("@MsgID", 0, DbType.Int32, 8, ParameterDirection.Output);
                DbParameter user = NewParam("@UserID", string.Empty, DbType.String, 50, ParameterDirection.Output);
                db.ExecuteNonQuery(CommandType.StoredProcedure, "IM_SaveMessage",
                    NewParam("@SendID", sendId),
                    NewParam("@Key", key),
                    NewParam("@SendName", sendName),
                    NewParam("@SendAvatar", sendAvatar),
                    NewParam("@Content", content),
                   msgId, user);
                if (msgId.Value == DBNull.Value)
                {
                    userId = null;
                    return 0;
                }
                userId = (string)user.Value;
                return (int)msgId.Value;
            }
        }

        /// <summary>
        /// 保存聊天记录（有是否和客服对话的判断）
        /// </summary>
        /// <param name="sendUser"></param>
        /// <param name="key"></param>
        /// <param name="sendName"></param>
        /// <param name="sendAvatar"></param>
        /// <param name="content"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int SaveMessage(int siteId, UserType userType, int uid, string sendId, ref string key, string sendName, string sendAvatar, string content, out string userId)
        {
            // 如果和客服说话
            if (userType == UserType.USER && key == Utils.GetTalkKey(sendId, Utils.SERVICE, siteId))
            {
                key = Utils.GetTalkKey(sendId, UserType.ADMIN + "-" + this.GetServiceID(uid, siteId), siteId);
            }
            return this.SaveMessage(sendId, key, sendName, sendAvatar, content, out userId);
        }

        /// <summary>
        /// 标记信息为已读
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msgId"></param>
        /// <returns></returns>
        public void UpdateMessageRead(User user, int msgId)
        {
            if (user == null) return;

            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE usr_ChatLog SET IsRead = 1 WHERE LogID = @LogID AND UserID = @UserID AND IsRead = 0",
                    NewParam("@LogID", msgId),
                    NewParam("@UserID", user.KEY));
            }
        }

        /// <summary>
        /// 通知标记为已读
        /// </summary>
        /// <param name="user"></param>
        /// <param name="notifyId"></param>
        public void UpdateNotifyRead(User user, int notifyId)
        {
            if (user == null) return;
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "UPDATE usr_Notify SET IsRead = 1 WHERE NotifyID = @NotifyID AND UserID = @UserID AND IsRead = 0",
                    NewParam("@NotifyID", notifyId),
                    NewParam("@UserID", user.ID));
            }
        }

        /// <summary>
        /// 保存用户的签名
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sign"></param>
        /// <returns></returns>
        public bool SaveSign(User user, string sign)
        {
            bool success = false;
            if (sign.Length > 50) sign = WebAgent.Left(sign, 40);
            using (DbExecutor db = NewExecutor())
            {
                switch (user.Type)
                {
                    case UserType.USER:
                        success = db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET [Sign] = @Sign WHERE UserID = @UserID",
                               NewParam("@Sign", sign),
                               NewParam("@UserID", user.ID)) != 0;
                        break;
                    case UserType.ADMIN:
                        success = db.ExecuteNonQuery(CommandType.Text, "UPDATE site_Admin SET [Sign] = @Sign WHERE AdminID = @UserID",
                               NewParam("@Sign", sign),
                               NewParam("@UserID", user.ID)) != 0;
                        break;
                }
            }
            return success;
        }

        /// <summary>
        /// 获取需要通知的信息列表
        /// </summary>
        /// <returns></returns>
        public List<Notify> GetNotifyList()
        {
            int[] users;
            lock (Utils.LOCK_USERLIST)
            {
                users = Utils.USERLIST.Where(t => t.Group != GroupType.None && t.Type == UserType.USER).Select(t => t.ID).ToArray();
            }
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetNotify",
                    NewParam("@Users", string.Join(",", users)));

                List<Notify> list = new List<Notify>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new Notify(dr));
                }
                return list;
            }

        }
    }
}
