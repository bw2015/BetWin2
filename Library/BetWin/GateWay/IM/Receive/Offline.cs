using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;

namespace BW.GateWay.IM.Receive
{
    /// <summary>
    /// 接收到会员下线通知
    /// </summary>
    public class Offline : IReceive
    {
        public Offline(Hashtable ht) : base(ht) { }

        public override void Run()
        {
            lock (Config.OnlineList)
            {
                if (Config.OnlineList.ContainsKey(this.ID))
                {
                    Config.OnlineList.Remove(this.ID);
                }
                foreach (List<string> list in Config.WechatUser.Select(t => t.Value))
                {
                    list.Remove(this.ID);
                }
                foreach (List<string> list in Config.SiteUser.Select(t => t.Value))
                {
                    list.Remove(this.ID);
                }
            }

            switch (this.UserType)
            {
                case UserAgent.IM_USER:
                    //#1 通知好友已经下线
                    foreach (string friend in UserAgent.Instance().GetFriends(this.UserID).Select(t => string.Concat(UserAgent.IM_USER, "-", t)).
                        Join(Config.OnlineList.Select(t => t.Key), t => t, t => t, (friend, user) => user))
                    {
                        Config.Send(friend, new BW.GateWay.IM.Message.Offline()
                        {
                            UserID = this.ID,
                            FriendID = friend
                        });
                    }
                    UserAgent.Instance().SetOnlineStatus(this.UserID, false);

                    break;
                case UserAgent.IM_ADMIN:
                    AdminAgent.Instance().SetOnlineStatus(this.UserID, false);

                    if (AdminAgent.Instance().IsService(this.UserID))
                    {
                        //#1 通知所有在线会员，客服下线了
                        foreach (string user in UserAgent.Instance().GetOnlineUser().Join(Config.OnlineList.Select(t => t.Key), t => string.Concat(UserAgent.IM_USER, "-", t), t => t, (userId, user) => user))
                        {
                            Config.Send(user, new BW.GateWay.IM.Message.Offline()
                            {
                                UserID = this.ID,
                                FriendID = user
                            });
                        }
                    }

                    break;
            }
        }
    }
}
