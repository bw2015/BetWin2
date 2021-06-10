using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.IM.Common;
using BW.IM.Factory.Message;
using SP.Studio.Web;

namespace BW.IM.Factory.Command
{
    /// <summary>
    /// 禁言设定
    /// </summary>
    public class BLOCK : ICommand
    {
        /// <summary>
        /// 禁言执行
        /// </summary>
        /// <param name="user">发出命令的用户</param>
        /// <param name="value">命令值</param>
        /// <param name="userId">被禁言对象</param>
        /// <param name="group">对整个群进行禁言</param>
        public BLOCK(User user, int value, int userId, GroupType group)
            : base(user, value)
        {
            string message = null;
            if (user.Type != UserType.ADMIN) return;

            if (value == 0)
            {
                message = string.Format("管理员{0}解除禁言", user.Name);
            }
            else
            {
                message = string.Format("管理员{0}设置禁言至{1}", user.Name, DateTime.Now.AddSeconds(value));
            }

            if (group != GroupType.None) userId = user.SiteID * 100 + (int)group;

            DateTime blockAt = DateTime.Now.AddSeconds(value);
            if (Utils.BLOCKUSER.ContainsKey(userId))
            {
                if (value == 0)
                {
                    Utils.BLOCKUSER.Remove(userId);
                }
                else
                {
                    Utils.BLOCKUSER[userId] = blockAt;
                }
            }
            else if (value > 0)
            {
                Utils.BLOCKUSER.Add(userId, blockAt);
            }

            if (group == GroupType.None)
            {
                string user1 = string.Concat(UserType.USER, "-", userId);
                Utils.Send(user1, new Tip
                {
                    ChatType = "friend",
                    Content = message,
                    Time = WebAgent.GetTimeStamp(),
                    Key = Utils.GetTalkKey(user1, user.KEY),
                    Type = Tip.TipType.System
                });
            }
            else
            {
                string[] userlist;
                lock (Utils.LOCK_USERLIST)
                {
                    userlist = Utils.USERLIST.Where(t => t.Group == group).Select(t => t.KEY).ToArray();
                }
                foreach (string key in userlist)
                {
                    Utils.Send(key, new Tip
                    {
                        ChatType = "group",
                        Content = message,
                        Time = WebAgent.GetTimeStamp(),
                        Key = Utils.GetTalkKey(key, group.GetKey()),
                        Type = Tip.TipType.System
                    });
                }
            }
        }
    }
}
