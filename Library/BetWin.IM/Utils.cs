using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using Timer = System.Timers.Timer;
using SP.Studio.Json;
using BW.IM.Common;
using SP.Studio.Core;
using SP.Studio.Security;
using BW.IM.Factory.Receive;
using BW.IM.Factory.Message;
using BW.IM.Framework;
using System.Web;
using BW.IM.Agent;
using SP.Studio.Model;
using SP.Studio.Web;
using SP.Studio.Net;
using System.Net;
using System.Web.WebSockets;

namespace BW.IM
{
    public static class Utils
    {
        #region ========== 常量 ===========

        /// <summary>
        /// 站点信息
        /// </summary>
        public const string SITEINFO = "SITEINFO";

        public const string USERINFO = "USERINFO";

        public const string ADMININFO = "ADMININFO";

        /// <summary>
        /// 用户登录的cookie值保存字段
        /// </summary>
        internal const string USERKEY = "USER";

        /// <summary>
        /// 管理员登录的cookie保存字段
        /// </summary>
        internal const string ADMINKEY = "ADMIN";

        /// <summary>
        /// 客服的标记
        /// </summary>
        internal const string SERVICE = "ADMIN-0";

        /// <summary>
        /// 使用POST传输的用户键值
        /// </summary>
        internal const string TOKEN_USER = "_token_user";

        /// <summary>
        /// 使用POST传输的管理员键值
        /// </summary>
        internal const string TOKEN_ADMIN = "_token_admin";

        internal const string SYSTEM_NAME = "系统通知";

        internal const string SYSTEM_ID = "SYSTEM";

        internal readonly static string SYSTEM_FACE = "/images/system.png";

        #endregion

        /// <summary>
        /// 静态构造
        /// </summary>
        static Utils()
        {
            SYSTEM_FACE = GetFace(SYSTEM_FACE);
            Timer timer = new Timer(1000);
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            // 封单通知
            Timer stopTimer = new Timer(3000);
            stopTimer.Elapsed += (sender, e) =>
            {
                RunStopNotify();
            };
            stopTimer.Start();
        }

        private static int timerIndex = 0;

        private static bool timerRunStatus = false;

        /// <summary>
        /// 定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (timerRunStatus) return;
            try
            {
                timerRunStatus = true;
                // 10分钟加载一次系统配置
                if (timerIndex % 600 == 0)
                {
                    GROUPSETTING = SiteAgent.Instance().GetGroupSetting().ToDictionary(t => t.Key, t => t);
                    REBOT = SiteAgent.Instance().GetRebot();
                }

                // 10s 计算一次获奖通知
                if (timerIndex % 10 == 0)
                {
                    UserAgent.Instance().GetNotifyList().ForEach(t =>
                    {
                        Utils.SendNotify(t);
                    });

                    RunRebot();
                }

                // 1分钟加载一次客服的在线状态
                if (timerIndex % 60 == 0)
                {
                    UserAgent.Instance().GetServiceStatus();
                }
            }
            catch (Exception ex)
            {
                SiteAgent.Instance().AddErrorLog(8080, ex, "定时器运行错误");
            }
            finally
            {
                timerIndex++;
                timerRunStatus = false;
            }
        }

        /// <summary>
        /// 记录当前用户是否已经投注过
        /// </summary>
        private static Dictionary<string, DateTime> _rebotHistory = new Dictionary<string, DateTime>();
        /// <summary>
        /// 运行机器人
        /// </summary>
        private static void RunRebot()
        {
            // 记录群组当前是否可以投注
            Dictionary<GroupType, string> group = new Dictionary<GroupType, string>();
            string url = string.Format("{0}/handler/game/wechat/isbettime", SysSetting.GetSetting().handlerServer);
            foreach (GroupType type in Enum.GetValues(typeof(GroupType)))
            {
                if (type == GroupType.None) continue;
                string result = NetAgent.UploadData(url, string.Format("Type={0}", type), Encoding.UTF8);
                Hashtable ht = JsonAgent.GetJObject(result);
                if (ht == null || ht["success"].ToString() == "0")
                {
                    continue;
                }
                group.Add(type, ht["msg"].ToString());
            }

            int[] sites = REBOT.GroupBy(t => t.SiteID).Select(t => t.Key).ToArray();

            foreach (int siteId in sites)
            {
                foreach (GroupType type in REBOT.Where(t => t.SiteID == siteId).GroupBy(t => t.Type).Select(t => t.Key))
                {
                    if (!IsBet(siteId, type)) continue;
                    Rebot rebot = REBOT.Where(t => t.IsTime() && t.SiteID == siteId && t.Type == type).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
                    if (rebot == null) continue;
                    string key = string.Format("{0}-{1}", type, rebot.ID);
                    if (!_rebotHistory.ContainsKey(key)) _rebotHistory.Add(key, DateTime.MinValue);
                    if (_rebotHistory[key] > DateTime.Now.AddMinutes(-5)) continue;

                    string command = rebot.GetCommand();
                    if (string.IsNullOrEmpty(command)) continue;

                    SendGroup(type, command, siteId, rebot.UserID, rebot.Face, rebot.Name, rebot.IMKey, true);
                    _rebotHistory[key] = DateTime.Now;
                }
            }
        }


        /// <summary>
        /// 封单通知
        /// </summary>
        private static void RunStopNotify()
        {
            Tuple<int, GroupType>[] sites = null;
            lock (LOCK_USERLIST)
            {
                sites = Utils.USERLIST.Where(t => t.Group != GroupType.None).GroupBy(t => new { t.SiteID, t.Group }).Select(t => new Tuple<int, GroupType>(t.Key.SiteID, t.Key.Group)).ToArray();
            }
            foreach (var site in sites)
            {
                int siteId = site.Item1;
                GroupType group = site.Item2;

                int groupId = GetGroupID(siteId, group);
                if (!STOPSTATUS.ContainsKey(groupId)) STOPSTATUS.Add(groupId, false);
                bool isOpen = IsBet(siteId, group);
                if (isOpen == STOPSTATUS[groupId]) return;
                string content = isOpen ? "当前已开放投注" : "当前期已封单";
                STOPSTATUS[groupId] = isOpen;

                SendGroup(group, content, siteId, 1, SYSTEM_FACE, SYSTEM_NAME, SYSTEM_ID, true);
            }
        }

        /// <summary>
        /// 是否可以投注
        /// </summary>
        /// <param name="siteId">站点</param>
        /// <param name="type">彩种</param>
        /// <returns></returns>
        private static bool IsBet(int siteId, GroupType type)
        {
            if (type == GroupType.None) return false;
            string url = string.Format("{0}/handler/game/wechat/isbettime", SysSetting.GetSetting().handlerServer);
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("SITEID", siteId.ToString());
                string result = NetAgent.UploadData(url, string.Format("Type={0}", type), Encoding.UTF8, wc);
                string success = "\"success\" : 1";
                return result.Contains(success);
            }
        }

        /// <summary>
        /// 获取群的ID表达方式
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetGroupID(int siteId, GroupType type)
        {
            return siteId * 100 + (int)type;
        }

        #region ======== 静态缓存对象  ===========

        /// <summary>
        /// 操作用户列表的进程锁
        /// </summary>
        internal const string LOCK_USERLIST = "LOCK_USERLIST";

        private static List<User> _userlist = new List<User>();
        /// <summary>
        /// 在线的用户列表
        /// </summary>
        internal static List<User> USERLIST
        {
            get
            {
                return _userlist;
            }
        }

        /// <summary>
        /// 当前有效的socket链接
        /// </summary>
        internal readonly static ConcurrentDictionary<string, WebSocket> SOCKETLIST = new ConcurrentDictionary<string, WebSocket>();

        /// <summary>
        /// 被禁言的用户
        /// </summary>
        internal readonly static Dictionary<int, DateTime> BLOCKUSER = new Dictionary<int, DateTime>();

        /// <summary>
        /// 群聊参数设定
        /// </summary>
        internal static Dictionary<int, GroupSetting> GROUPSETTING = new Dictionary<int, GroupSetting>();

        /// <summary>
        /// 机器人列表
        /// </summary>
        internal static List<Rebot> REBOT = new List<Rebot>();

        /// <summary>
        /// 封单状态
        /// </summary>
        internal static Dictionary<int, bool> STOPSTATUS = new Dictionary<int, bool>();

        /// <summary>
        /// 客服在线状态列表
        /// </summary>
        internal readonly static Dictionary<int, Dictionary<int, bool>> SERVICELIST = new Dictionary<int, Dictionary<int, bool>>();

        /// <summary>
        /// 用户与上次对话的客服
        /// </summary>
        internal readonly static Dictionary<int, int> USERSERVICE = new Dictionary<int, int>();

        /// <summary>
        /// 客服合并设置
        /// </summary>
        internal readonly static Dictionary<int, CustomerService> CUSTOMERSERVICE = new Dictionary<int, CustomerService>();

        #endregion

        #region ====== 处理信息 ========


        private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        /// <summary>
        /// 收到信息
        /// </summary>
        /// <param name="user">信息的发送者</param>
        /// <param name="message">信息内容（JSON格式）</param>
        public static void GetMessage(User user, string message)
        {
            //SiteAgent.Instance().AddSystemLog(8080, message + "\n" + user.SiteID + "," + user.ID);
            if (string.IsNullOrEmpty(message)) return;
            Hashtable ht = JsonAgent.GetJObject(message);
            if (ht == null || !ht.ContainsKey("Action"))
            {
                return;
            }
            string action = ht["Action"].ToString();
            string typeName = "BW.IM.Factory.Receive." + action;
            Type type = null;
            if (typeCache.ContainsKey(typeName))
            {
                type = typeCache[typeName];
            }
            else
            {
                lock (typeCache)
                {
                    if (typeCache.ContainsKey(typeName))
                    {
                        type = typeCache[typeName];
                    }
                    else
                    {
                        type = typeof(Utils).Assembly.GetType(typeName);
                        typeCache.Add(typeName, type);
                    }
                }
            }

            if (type == null)
            {
                SiteAgent.Instance().AddSystemLog(0, string.Format("没有解析类，内容：{0}", message));
                return;
            }
            IReceive receive = (IReceive)Activator.CreateInstance(type, new object[] { user, ht });
            receive.Run();
        }

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="key">信息接收者的Key值</param>
        /// <param name="message">信息结构</param>
        /// <returns>是否发送成功（接收方是否在线）</returns>
        public static bool Send(string key, IMessage message)
        {
            if (!SOCKETLIST.ContainsKey(key)) return false;
            User user = USERLIST.Find(t => t.KEY == key);
            bool success = false;
            try
            {
                WebSocket socket = SOCKETLIST[key];
                Task.Run(() => Send(socket, message.ToString())).Wait();
                success = true;
                if (user != null)
                {
                    user.Error = 0;
                    user.ActiveAt = DateTime.Now;
                }
            }
            catch
            {
                success = false;
            }

            if (user != null)
            {
                if (success)
                {
                    user.Error = 0;
                }
                else
                {
                    user.Error++;
                    // 如果连续错误5次则关闭掉链接
                    if (user.Error > 5)
                    {
                        Utils.Close(user.KEY);
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// 在群内发送中奖通知
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        public static bool SendNotify(Notify notify)
        {
            User user = null;
            lock (LOCK_USERLIST)
            {
                user = USERLIST.Find(t => t.ID == notify.UserID);
            }
            if (user == null || user.Group == GroupType.None) return false;
            string key = Utils.GetTalkKey(notify.User, user.Group.GetKey(), user.SiteID);
            return Utils.Send(notify.User, SystemReply(key, notify.Message, "group", notify.ID));
        }

        /// <summary>
        /// 关闭一个连接
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void Close(string key)
        {
            try
            {
                WebSocket socket = SOCKETLIST.ContainsKey(key) ? SOCKETLIST[key] : null;
                User user = USERLIST.Find(t => t.KEY == key);
                if (user != null)
                {
                    Utils.GetMessage(user, "{\"Action\":\"Offline\"}");
                }
                if (socket != null)
                {
                    //请改用“WebSocketCloseOutputAsync”保持能够接收数据，但关闭输出通道
                    //Task.Run(() => socket.CloseOutputAsync(WebSocketCloseStatus.Empty, "", new CancellationToken())).Wait();
                    //socket.Dispose();
                }
            }
            catch (Exception ex)
            {
                SiteAgent.Instance().AddErrorLog(8080, ex, "关闭连接出错");
            }
            finally
            {
                WebSocket ows;
                if (Utils.SOCKETLIST.TryRemove(key, out ows))
                {
                    Utils.USERLIST.RemoveAll(t => t.KEY == key);
                }
            }
        }

        internal static async Task Send(WebSocket socket, string message)
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // 标识当前会话KEY是否已经存入数据库
        private static Dictionary<string, bool> _taklKey = new Dictionary<string, bool>();
        /// <summary>
        /// 获取对话的Key值
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <returns></returns>
        public static string GetTalkKey(string user1, string user2, int siteId = 0)
        {
            ChatType type1 = GetChatType(user1);
            ChatType type2 = GetChatType(user2);

            TalkType type = TalkType.None;
            if (type1 == ChatType.GROUP) { type = TalkType.Group; user2 = string.Empty; }
            if (type2 == ChatType.GROUP) { type = TalkType.Group; user1 = string.Empty; }

            string[] user = new string[] { user1, user2 }.OrderBy(t => t).ToArray();
            string key = MD5.toMD5(string.Join(",", user));
            if (_taklKey.ContainsKey(key)) return key;

            if (type == TalkType.None)
            {
                type = string.Join("2", new string[] { type1.ToString(), type2.ToString() }.OrderBy(t => t)).ToEnum<TalkType>();
            }

            UserAgent.Instance().SaveTalkKey(user[0], user[1], key, type, siteId);
            _taklKey.Add(key, true);
            return key;
        }

        /// <summary>
        /// 获取用户类型
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static ChatType GetChatType(string user)
        {
            user = user.Substring(0, user.IndexOf('-'));
            return user.ToEnum<ChatType>();
        }

        /// <summary>
        /// 获取用户的ID（群类型）
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetChatID(string user)
        {
            return user.Substring(user.IndexOf('-') + 1);
        }

        /// <summary>
        /// 系统回复信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="notifyId">是否是通知ID</param>
        /// <returns></returns>
        public static BW.IM.Factory.Message.Message SystemReply(string key, string message, string type = "group", int notifyId = 0)
        {
            string userId;  // 信息的接收者
            int logId = UserAgent.Instance().SaveMessage(SYSTEM_ID, key, SYSTEM_NAME, SYSTEM_FACE, message, out userId);
            return new Factory.Message.Message()
            {
                Content = message,
                ID = logId,
                Avatar = SYSTEM_FACE,
                Name = SYSTEM_NAME,
                SendID = SYSTEM_ID,
                Time = WebAgent.GetTimeStamps(),
                Type = type,
                Key = key,
                NotifyID = notifyId
            };
        }


        /// <summary>
        /// 发送给群内所有成员
        /// </summary>
        /// <param name="type">群类型</param>
        /// <param name="content">送内容</param>
        /// <param name="siteId">所属站点</param>
        /// <param name="userId">发送者ID 如果为0表示只发给管理员</param>
        /// <param name="face">发送者头像</param>
        /// <param name="name">发送者名字</param>
        /// <param name="userKey">发送者的IM KEY值</param>
        /// <param name="noAdmin">不发送给管理员</param>
        public static void SendGroup(GroupType type, string content, int siteId, int userId, string face, string name, string userKey, bool noAdmin = false)
        {
            User[] users = null;
            lock (LOCK_USERLIST)
            {
                IEnumerable<User> userlist = Utils.USERLIST.FindAll(t => t.SiteID == siteId);
                if (userId == 0)
                {
                    userlist = userlist.Where(t => t.Type == UserType.ADMIN);
                }
                else
                {
                    userlist = userlist.Where(t => t.ID != userId && (t.Group == type || t.Type == UserType.ADMIN));
                }
                if (noAdmin) userlist = userlist.Where(t => t.Type != UserType.ADMIN);
                users = userlist.ToArray();
            }
            if (userId == 1)
            {
                SiteAgent.Instance().AddSystemLog(8080, string.Format("封单通知，通知数量：{0},{1}", users.Length, content));
            }
            foreach (User t in users)
            {
                Utils.Send(t.KEY, new BW.IM.Factory.Message.Message()
                {
                    Avatar = face,
                    Content = content,
                    Key = Utils.GetTalkKey(type.GetKey(), t.KEY, siteId),
                    Name = name,
                    Time = WebAgent.GetTimeStamps(),
                    SendID = userKey,
                    Type = "group"
                });
            }
        }


        #endregion

        #region ========= 命令处理 ============

        /// <summary>
        /// 命令的缓存
        /// </summary>
        private static Dictionary<string, Command> commandCache = new Dictionary<string, Command>();
        /// <summary>
        /// 是否是命令
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static CommandMessage GetCommand(this string message, out int value)
        {
            if (commandCache.ContainsKey(message))
            {
                value = commandCache[message].Value;
                return commandCache[message].Type;
            }
            value = 0;
            foreach (CommandMessage type in Enum.GetValues(typeof(CommandMessage)))
            {
                if (type == CommandMessage.None) continue;
                Regex regex = new Regex(type.GetDescription());
                if (!regex.IsMatch(message)) continue;
                value = int.Parse(regex.Match(message).Groups["Value"].Value);
                commandCache.Add(message, new Command(type, value));
                return type;
            }

            return CommandMessage.None;
        }

        #endregion



        /// <summary>
        /// 输出一个json内容
        /// </summary>
        /// <param name="context"></param>
        /// <param name="json"></param>
        public static void output(HttpContext context, string json)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write(json);
            context.Response.End();
        }

        /// <summary>
        /// 输出一个错误信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        public static void showerror(HttpContext context, string msg)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write(string.Format("{{\"code\":1,\"msg\":\"{0}\"}}", HttpUtility.JavaScriptStringEncode(msg)));
            context.Response.End();
        }

        /// <summary>
        /// 获取用户头像的地址
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static string GetFace(string face)
        {
            if (string.IsNullOrEmpty(face))
            {
                return string.Format("{0}/images/user.png", SysSetting.GetSetting().imgServer);
            }

            if (face.StartsWith("http")) return face;
            return string.Concat(SysSetting.GetSetting().imgServer, face);
        }

        /// <summary>
        /// 获取群聊的Key值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetKey(this GroupType type)
        {
            return string.Concat(ChatType.GROUP, "-", type);
        }
    }
}
