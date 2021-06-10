using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.IM.Common;
using BW.IM.Agent;

namespace BW.IM.Factory.Receive
{
    /// <summary>
    /// 离线信息
    /// </summary>
    public class Offline : IReceive
    {
        public Offline(User user, Hashtable ht) : base(user, ht) { }

        public override void Run()
        {
            if (this.UserInfo == null) return;

            string[] userlist = null;
            lock (Utils.LOCK_USERLIST)
            {
                switch (this.UserInfo.Type)
                {
                    case UserType.USER:
                        userlist = UserAgent.Instance().GetFriends(this.UserInfo.ID).Join(Utils.USERLIST.Where(t => t.SiteID == this.UserInfo.SiteID && t.Type == this.UserInfo.Type),
                            t => t, t => t.ID, (friend, user) => user.KEY).ToArray();
                        UserAgent.Instance().SetUserOnlineStatus(this.UserInfo.ID, false);
                        break;
                    case UserType.ADMIN:
                        if (this.UserInfo.IsService)
                        { 
                            //#1 通知所有在线会员，客服下线了
                            userlist = Utils.USERLIST.Where(t => t.SiteID == this.UserInfo.SiteID && t.Type == UserType.USER).Select(t => t.KEY).ToArray();
                        }
                        UserAgent.Instance().SetAdminOnlineStatus(this.UserInfo.ID, false);
                        break;
                }
            }

            if (userlist != null)
            {
                //#1 通知好友下线
                foreach (string friend in userlist)
                {
                    Utils.Send(friend, new BW.IM.Factory.Message.Offline()
                    {
                        FriendID = friend,
                        UserID = this.UserInfo.KEY
                    });
                }
            }
        }
    }
}
