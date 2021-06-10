using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;
using BW.Common.Users;
using SP.Studio.Core;
using SP.Studio.Web;
using System.Text.RegularExpressions;
using BW.GateWay.Lottery;
using BW.Common.Lottery;

namespace BW.GateWay.IM.Receive
{
    public class Message : IReceive
    {
        /// <summary>
        /// 收到发出的信息
        /// </summary>
        /// <param name="ht"></param>
        public Message(Hashtable ht)
            : base(ht)
        {

        }

        /// <summary>
        /// 回话Key
        /// </summary>
        public string Key { get; set; }

        public string Content { get; set; }

        /// <summary>
        /// 发送的头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 信息编号
        /// </summary>
        public long MsgID { get; set; }

        /// <summary>
        /// 收到后要进行的操作
        /// 1、判断接收类型
        /// 2.1、管理员（查找在线的客服，如果有则推送）
        /// 2.2、查找在线的好友，如果有就推送
        /// 2.3、如果信息接收方不在线，则返回不在线提示
        /// </summary>
        public override void Run()
        {
            //1、保存聊天记录
            ChatLog log = new ChatLog()
            {
                Key = this.Key,
                Content = this.Content,
                CreateAt = DateTime.Now,
                SendAvatar = this.Avatar,
                SendName = this.Name,
                SendID = this.ID
            };

            UserAgent.Instance().SaveChatLog(log);

            if (log.ID == 0)
            {
                return;
            }

            //#2、推送给在线好友
            string userType;
            int userId = UserAgent.Instance().GetChatUserID(log.UserID, out userType);

            bool isOnline = false;
            // 如果是发给客服
            if (log.UserID == UserAgent.IM_ADMIN_SERVICE)
            {
                log.UserID = Config.GetServiceID(log.SiteID);
                isOnline = !string.IsNullOrEmpty(log.UserID);
            }
            else
            {
                // 如果是发送给群
                if (userType == UserAgent.IM_GROUP)
                {
                    this.GroupMessage(log);
                    return;
                }
                isOnline = Config.IsOnline(log.UserID);
            }

            // 如果接收者不在线
            if (!isOnline)
            {
                Config.Send(log.SendID, new BW.GateWay.IM.Message.Tip()
                {
                    Key = log.Key,
                    Content = "对方不在线，将会在上线后收到您的信息"
                });
                return;
            }

            Config.Send(log.UserID, new BW.GateWay.IM.Message.Message()
            {
                ID = log.ID,
                Avatar = log.SendAvatar,
                Content = log.Content,
                Key = log.Key,
                Name = log.SendName,
                Time = WebAgent.GetTimeStamps(log.CreateAt),
                SendID = log.SendID
            });
        }

        /// <summary>
        /// 把信息推送给在线的群成员
        /// </summary>
        /// <param name="log"></param>
        private void GroupMessage(ChatLog log)
        {
            UserAgent.Instance().UpdateChatLogRead(log.ID, log.UserID);

            //群类型
            ChatTalk.GroupType type = (ChatTalk.GroupType)UserAgent.Instance().GetChatUserID(log.UserID);

            if (!Config.WechatUser.ContainsKey(type.ToEnum<LotteryType>())) return;

            string sendType = UserAgent.Instance().GetTalkType(log.SendID);

            switch (sendType)
            {
                case UserAgent.IM_USER:
                    this.GroupMessageByUser(type, log);
                    break;
                case UserAgent.IM_ADMIN:
                    this.GroupMessageByAdmin(type, log);
                    break;
            }
        }

        /// <summary>
        /// 如果是用户发出的信息
        /// </summary>
        /// <param name="log"></param>
        private void GroupMessageByUser(ChatTalk.GroupType type, ChatLog log)
        {
            // 群发信息
            this.GroupMessage(log, type, false);
        }

        /// <summary>
        /// 管理员发出的信息
        /// </summary>
        /// <param name="log"></param>
        private void GroupMessageByAdmin(ChatTalk.GroupType type, ChatLog log)
        {
            if (log.Content.StartsWith("@"))
            {
                // 发给单一用户
                Regex regex = new Regex(@"^@(?<User>\w+)", RegexOptions.IgnoreCase);
                if (!regex.IsMatch(log.Content))
                {
                    this.Tip(log, "回复格式错误", null);
                    return;
                }

                string userName = regex.Match(log.Content).Groups["User"].Value;
                int userId = UserAgent.Instance().GetUserID(userName);

                if (userId == 0)
                {
                    this.Tip(log, string.Format("用户{0}不存在", userName), null);
                    return;
                }

                this.GroupMessage(log, type, false, string.Concat(UserAgent.IM_USER, "-", userId));
            }
            else
            {
                // 发给所有人
                this.GroupMessage(log, type, true);
            }
        }

        /// <summary>
        /// 回复发送者
        /// </summary>
        /// <param name="log"></param>
        /// <param name="message"></param>
        private void Tip(ChatLog log, string message, string tipType, BW.GateWay.IM.Message.Tip.IconType icon = IM.Message.Tip.IconType.None)
        {
            // 投注
            Config.Send(log.SendID, new BW.GateWay.IM.Message.Tip()
            {
                Key = log.Key,
                Content = message,
                Type = "group",
                Icon = icon,
                TipType = tipType,
                MsgId = this.MsgID
            });
        }

        /// <summary>
        /// 群发信息（一定会带上管理员）
        /// </summary>
        /// <param name="log"></param>
        /// <param name="all">为true表示发送给所有的群成员 false表示只发给管理员</param>
        /// <param name="users">发给指定用户</param>
        private void GroupMessage(ChatLog log, ChatTalk.GroupType type, bool all, params string[] users)
        {

            LotteryType lottery = type.ToEnum<LotteryType>();

            IEnumerable<string> list = Config.WechatUser[lottery].Where(t => t != log.SendID && Config.SiteUser[log.SiteID].Contains(t));
            if (!all)
            {
                list = list.Where(t => users.Contains(t));
            }
            list = list.Union(Config.serviceList[log.SiteID].Select(t => string.Concat(UserAgent.IM_ADMIN, "-", t))).Where(t => t != log.SendID);

            foreach (string userId in list)
            {
                Config.Send(userId, new BW.GateWay.IM.Message.Message()
                {
                    ID = log.ID,
                    Avatar = log.SendAvatar,
                    Content = log.Content,
                    Key = UserAgent.Instance().GetTalkKey(userId, type),
                    Name = log.SendName,
                    Time = WebAgent.GetTimeStamps(log.CreateAt),
                    SendID = log.SendID,
                    Type = "group"
                });
            }
        }
    }
}
