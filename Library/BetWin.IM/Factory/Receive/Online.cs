using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.IM.Agent;
using BW.IM.Common;

using SP.Studio.Core;
using SP.Studio.Web;


namespace BW.IM.Factory.Receive
{
    /// <summary>
    /// 用户上线信息
    /// </summary>
    public class Online : IReceive
    {
        /// <summary>
        /// 要加入的群组
        /// </summary>
        public string Group { get; set; }

        public Online(User user, Hashtable ht) : base(user, ht) { }

        public override void Run()
        {
            if (this.UserInfo == null) return;

            if (!string.IsNullOrEmpty(this.Group))
            {
                GroupType group = this.Group.ToEnum<GroupType>();
                if (group == GroupType.None) return;
                User user = Utils.USERLIST.Find(t => t.KEY == this.UserInfo.KEY);
                if (user != null) user.SetGroup(group);
                return;
            }

            string[] userlist = null;
            lock (Utils.LOCK_USERLIST)
            {
                switch (this.UserInfo.Type)
                {
                    case UserType.USER:
                        //#1 通知好友上线
                        userlist = UserAgent.Instance().GetFriends(this.UserInfo.ID).Join(Utils.USERLIST.Where(t => t.SiteID == this.UserInfo.SiteID && t.Type == this.UserInfo.Type),
                            t => t, t => t.ID, (friend, user) => user.KEY).ToArray();

                        UserAgent.Instance().SetUserOnlineStatus(this.UserInfo.ID, true);
                        break;
                    case UserType.ADMIN:

                        if (this.UserInfo.IsService)
                        {
                            //#1 通知所有在线会员，客服上线了
                            userlist = Utils.USERLIST.Where(t => t.SiteID == this.UserInfo.SiteID && t.Type == UserType.USER).Select(t => t.KEY).ToArray();
                        }
                        UserAgent.Instance().SetAdminOnlineStatus(this.UserInfo.ID, true);
                        break;
                }
            }

            if (userlist != null)
            {
                foreach (string friend in userlist)
                {
                    Utils.Send(friend, new BW.IM.Factory.Message.Online()
                    {
                        FriendID = friend,
                        UserID = this.UserInfo.KEY
                    });
                }
            }

            //#2 获取离线消息
            foreach (ChatLog log in UserAgent.Instance().GetChatLogUnread(this.UserInfo.KEY))
            {
                Utils.Send(this.UserInfo.KEY, new BW.IM.Factory.Message.Message()
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
