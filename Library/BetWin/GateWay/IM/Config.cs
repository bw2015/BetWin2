using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;
using System.Timers;
using System.Net;
using System.Net.WebSockets;
using System.Text.RegularExpressions;

using BW.GateWay.IM.Message;
using BW.Agent;
using SP.Studio.Json;
using BW.GateWay.IM.Receive;
using BW.Common.Users;
using BW.Common.Lottery;
using BW.Common.Wechat;
using BW.Framework;

using Timer = System.Timers.Timer;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Web;

namespace BW.GateWay.IM
{
    /// <summary>
    /// WebSocket对象的缓存
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// 时间计数器
        /// </summary>
        internal static int TimeCount = 0;

        /// <summary>
        /// 客服列表
        /// </summary>
        internal static Dictionary<int, List<int>> serviceList = new Dictionary<int, List<int>>();

        /// <summary>
        /// 所有在线的用户Key值
        /// </summary>
        public static Dictionary<string, WebSocket> OnlineList = new Dictionary<string, WebSocket>();

        /// <summary>
        /// 微信玩家
        /// </summary>
        public static Dictionary<LotteryType, List<string>> WechatUser = new Dictionary<LotteryType, List<string>>();

        /// <summary>
        /// 在线用户所属的站点
        /// </summary>
        public static Dictionary<int, List<string>> SiteUser = new Dictionary<int, List<string>>();

        /// <summary>
        /// 微信的群参数设置
        /// </summary>
        public static Dictionary<string, GroupSetting> WechatGroupSetting = new Dictionary<string, GroupSetting>();

        static Config()
        {
            // 开奖结果的回调通知
            /*
            Timer timer = new Timer(1000);
            timer.Elapsed += timer_Elapsed;

            foreach (LotteryType type in Enum.GetValues(typeof(LotteryType)))
            {
                if (!type.GetCategory().Wechat) continue;
                WechatUser.Add(type, new List<string>());

                if (!LotteryAgent.Instance().OpenRewardCallback.ContainsKey(type)) LotteryAgent.Instance().OpenRewardCallback.Add(type, new List<Action<LotteryType, int[]>>());
                LotteryAgent.Instance().OpenRewardCallback[type].Add(_sendLotteryNotify);
            }

            timer.Start();
            */
        }

        /// <summary>
        /// 发送中奖通知
        /// </summary>
        /// <param name="type"></param>
        /// <param name="orders"></param>
        private static void _sendLotteryNotify(LotteryType type, int[] orders)
        {
            if (!WechatUser.ContainsKey(type) || WechatUser[type].Count == 0) return;
            List<LotteryOrder> orderList = LotteryAgent.Instance().GetLotteryOrderList(orders);
            foreach (int userId in orderList.Where(p => p.IsLottery).GroupBy(p => p.UserID).Select(p => p.Key))
            {
                string userKey = string.Concat(UserAgent.IM_USER, "-", userId);
                if (!WechatUser[type].Contains(userKey)) continue;

                IEnumerable<LotteryOrder> list = orderList.Where(p => p.UserID == userId);
                StringBuilder sb = new StringBuilder();
                sb.Append("{")
                    .AppendFormat("\"index\":\"{0}\",", list.FirstOrDefault().Index)
                    .AppendFormat("\"list\":{0}", list.ToJson(p => p.Remark, p => p.Money, p => p.Reward))
                    .Append("}");
                Config.Send(userKey, new Notify()
                {
                    Message = "开奖通知",
                    Content = new JsonString(sb.ToString())
                });
            }
        }

        /// <summary>
        /// 收到信息
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        internal static void GetMessage(string userId, string message)
        {
            Hashtable ht = JsonAgent.GetJObject(message);
            if (ht == null || !ht.ContainsKey("Action"))
            {
                SystemAgent.Instance().AddSystemLog(0, string.Format("收到非json格式，内容：{0}", message));
                return;
            }
            string action = ht["Action"].ToString();

            Type type = typeof(Config).Assembly.GetType("BW.GateWay.IM.Receive." + action);
            if (type == null)
            {
                SystemAgent.Instance().AddSystemLog(0, string.Format("没有解析类，内容：{0}", message));
                return;
            }
            IReceive receive = (IReceive)Activator.CreateInstance(type, new object[] { ht });
            receive.Run();
        }

        /// <summary>
        /// 随机启动机器人的时间
        /// </summary>
        private static int rebotRandomTime = 60;

        /// <summary>
        /// 系统定时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (typeof(Config))
                {
                    if (TimeCount % 60 == 0)
                    {
                        serviceList = AdminAgent.Instance().GetServiceAdmin();
                        WechatGroupSetting = WechatAgent.Instance().GetWechatGroupList().ToDictionary(t => string.Format("{0}-{1}", t.SiteID, t.Type), t => t);
                    }

                    if (rebotRandomTime == 0)
                    {
                        _wechatRebot();
                        rebotRandomTime = 10 + WebAgent.GetRandom(10, 60);
                    }

                    rebotRandomTime--;

                    // 发送封单/开启信息
                    _closeIndex();
                }
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(0, ex, "执行IM定时器任务出错");
            }
            finally
            {
                TimeCount++;
            }
        }



        /// <summary>
        /// 最后一期的机器人投注期号（用户ID、期号）
        /// </summary>
        private static Dictionary<LotteryType, Dictionary<int, string>> lastRebotIndex = new Dictionary<LotteryType, Dictionary<int, string>>();
        /// <summary>
        /// 机器人定时投注
        /// </summary>
        private static void _wechatRebot()
        {
            //机器人
            List<WechatRebot> rebotList = WechatAgent.Instance().GetRebotList().Where(t => t.Setting.IsTime()).ToList();

            foreach (ChatTalk.GroupType type in rebotList.Select(t => t.Type).Distinct())
            {
                LotteryType game = type.ToEnum<LotteryType>();
                string betIndex = null;

                if (!lastRebotIndex.ContainsKey(game)) lastRebotIndex.Add(game, new Dictionary<int, string>());

                foreach (int siteId in rebotList.Select(t => t.SiteID).Distinct())
                {
                    bool isBreak = false;
                    try
                    {
                        if (!Utils.IsBet(game, out betIndex, siteId))
                        {
                            SystemAgent.Instance().AddSystemLog(siteId, string.Format("{0} 当前封单", game));
                            isBreak = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemAgent.Instance().AddErrorLog(0, ex, "获取是否可投注出错");
                        isBreak = true;
                    }
                    if (isBreak) continue;

                    int betCount = 0;
                    foreach (WechatRebot rebot in rebotList.Where(t => t.SiteID == siteId && t.Type == type).OrderBy(t => Guid.NewGuid()).Take(5))
                    {
                        if (betCount >= 2) continue;
                        User user = UserAgent.Instance().GetUserInfo(rebot.UserID);
                        if (user == null || !user.IsTest || string.IsNullOrEmpty(rebot.Setting.Command)) continue;
                        if (!lastRebotIndex[game].ContainsKey(user.ID)) lastRebotIndex[game].Add(user.ID, null);

                        if (lastRebotIndex[game][user.ID] == betIndex) continue;

                        StringBuilder sb = new StringBuilder();
                        string content = rebot.Setting.Command.Split(' ', '|', '\n').GetRandom();
                        if (content.Length > 20 || string.IsNullOrEmpty(content)) continue;
                        sb.Append("{")
                            .AppendFormat("Action:\"WechatBet\",")
                            .AppendFormat("Content:\"{0}\",", content)
                            .AppendFormat("Game:\"{0}\",", game)
                            .AppendFormat("ID:\"{0}\",", user.IMID)
                            .AppendFormat("Session:\"{0}\",", UserAgent.Instance().GetUserSession(rebot.UserID, SP.Studio.PageBase.PlatformType.Wechat, true))
                            .AppendFormat("MsgID:\"{0}\"", WebAgent.GetTimeStamp())
                            .Append("}");

                        try
                        {
                            GetMessage(user.IMID, sb.ToString());
                            SystemAgent.Instance().AddSystemLog(rebot.SiteID, string.Format("机器人投注成功，用户名：{0} 投注内容：{1}", user.UserName, content));
                            betCount++;
                            lastRebotIndex[game][user.ID] = betIndex;
                        }
                        catch (Exception ex)
                        {
                            SystemAgent.Instance().AddErrorLog(rebot.SiteID, ex, "机器人投注错误");
                        }
                    }
                }

            }
        }


        /// <summary>
        /// 标记
        /// </summary>
        private static Dictionary<string, bool> closeIndex = new Dictionary<string, bool>();
        /// <summary>
        /// 封单和开放的通知
        /// </summary>
        private static void _closeIndex()
        {
            if (TimeCount % 30 == 0)
            {
                foreach (LotteryType type in WechatUser.Select(t => t.Key).ToArray())
                {
                    string key;
                    if (type.GetCategory().SiteLottery)
                    {
                        foreach (int siteId in LotteryAgent.Instance().GetOpenSiteList(type))
                        {
                            key = string.Concat(siteId, "-", type);
                            if (!closeIndex.ContainsKey(key)) closeIndex.Add(key, false);
                        }
                    }
                    else
                    {
                        key = type.ToString();
                        if (!closeIndex.ContainsKey(key)) closeIndex.Add(key, false);
                    }
                }
            }

            Regex regex = new Regex(@"^(?<SiteID>\d+)\-(?<Type>\w+)", RegexOptions.IgnoreCase);
            foreach (string key in closeIndex.Select(t => t.Key).ToArray())
            {
                int siteId = 0;
                LotteryType type;
                if (regex.IsMatch(key))
                {
                    siteId = int.Parse(regex.Match(key).Groups["SiteID"].Value);
                    type = regex.Match(key).Groups["Type"].Value.ToEnum<LotteryType>();
                }
                else
                {
                    type = key.ToEnum<LotteryType>();
                }

                bool isBet = false;
                string index;
                isBet = Utils.IsBet(type, out index, siteId);


                if (closeIndex[key] != isBet)
                {
                    closeIndex[key] = isBet;
                    IEnumerable<string> userList = WechatUser[type].ToArray();
                    if (siteId != 0)
                    {
                        if (!SiteUser.ContainsKey(siteId)) continue;
                        userList = userList.Join(SiteUser[siteId].ToArray(), t => t, t => t, (a, b) => a);
                    }
                    foreach (string user in userList)
                    {
                        Send(user, new Tip()
                        {
                            Type = "",
                            Content = isBet ? string.Format("第{0}期已开放投注", index) : "当前期已封单",
                            MsgId = WebAgent.GetTimeStamps(),
                            TipType = isBet ? "OpenIndex" : "CloseIndex"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 判断用户是否在线
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static bool IsOnline(string user, int siteId = 0)
        {
            if (user == UserAgent.IM_ADMIN_SERVICE)
            {
                user = GetServiceID(siteId);
            }

            // 如果信息接收者不在线则不需要发送
            if (string.IsNullOrEmpty(user) || !OnlineList.ContainsKey(user)) return false;

            return true;
        }

        /// <summary>
        /// 发送信息(直接发送）
        /// </summary>
        /// <param name="user">信息的接收者</param>
        /// <param name="message">要发送出的信息构造体</param>
        /// <param name="siteId">指定站点</param>
        /// <returns>接收者是否在线</returns>
        internal static void Send(string user, IMessage message, int siteId = 0)
        {
            if (!IsOnline(user, siteId)) return;
            //Send(user, message.ToString());

            Task task = null;
            Task.Run(() => { task = Send(user, message.ToString()); }).Wait();
        }

        /// <summary>
        /// 执行发送任务
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        internal static async Task Send(string user, string message)
        {
            WebSocket socket = OnlineList[user];
            ArraySegment<byte> buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// 获取在线的客服
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        internal static string GetServiceID(int siteId)
        {
            if (!serviceList.ContainsKey(siteId)) return null;
            return serviceList[siteId].Select(t => string.Concat(UserAgent.IM_ADMIN, "-", t)).Join(OnlineList.Select(t => t.Key), t => t, t => t, (admin, user) => admin).FirstOrDefault();
        }
    }
}
