using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;
using BW.Common.Users;
using SP.Studio.Web;


namespace BW.GateWay.IM.Receive
{
    /// <summary>
    /// 用户上线信息
    /// </summary>
    public class Online : IReceive
    {
        public Online(Hashtable ht) : base(ht) { }

        public override void Run()
        {
            string[] users = null;
            switch (this.UserType)
            {
                case UserAgent.IM_USER:
                    //#1 通知好友上线
                    foreach (string friend in UserAgent.Instance().GetFriends(this.UserID).Join(Config.OnlineList.Select(t => t.Key), t => string.Concat(UserAgent.IM_USER, "-", t), t => t, (friend, user) => user))
                    {
                        Config.Send(friend, new BW.GateWay.IM.Message.Online()
                        {
                            UserID = this.ID,
                            FriendID = friend
                        });
                    }
                    UserAgent.Instance().SetOnlineStatus(this.UserID, true);
                    users = new string[] { this.ID };
                    break;
                case UserAgent.IM_ADMIN:
                    AdminAgent.Instance().SetOnlineStatus(this.UserID, true);
                    if (AdminAgent.Instance().IsService(this.UserID))
                    {
                        users = new string[] { UserAgent.IM_ADMIN_SERVICE, this.ID };

                        //#1 通知所有在线会员，客服上线了
                        foreach (string user in UserAgent.Instance().GetOnlineUser().Join(Config.OnlineList.Select(t => t.Key), t => string.Concat(UserAgent.IM_USER, "-", t), t => t, (userId, user) => user))
                        {
                            Config.Send(user, new BW.GateWay.IM.Message.Online()
                            {
                                UserID = this.ID,
                                FriendID = user
                            });
                        }
                    }
                    else
                    {
                        users = new string[] { this.ID };
                    }
                    break;
            }

            if (users != null)
            {
                //#2 获取离线消息
                foreach (ChatLog log in UserAgent.Instance().GetChatLogUnread(users))
                {
                    Config.Send(this.ID, new BW.GateWay.IM.Message.Message()
                    {
                        Avatar = log.SendAvatar,
                        Content = log.Content,
                        ID = log.ID,
                        Key = log.Key,
                        Name = log.SendName,
                        Time = WebAgent.GetTimeStamps(log.CreateAt),
                        SendID = log.SendID
                    });
                }
            }
        }
    }
}
