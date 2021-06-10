using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

using SP.Studio.Array;

using IMService.Framework;
using IMService.Common;
using IMService.Common.Send;

using BW.Agent;

using IMService.Agent;
using Fleck;

namespace IMService.Common.Message
{
    /// <summary>
    /// 链接关闭事件
    /// </summary>
    public class Offline : IMessage
    {

        public Offline(string id)
        {
            this.ID = id;
        }

        public override void Run()
        {
            //#1 如果是用户则给所有好友发送下线通知
            int userId;
            UserType type = Utils.GetUserType(this.ID, out userId);
            switch (type)
            {
                case UserType.Admin:
                    AdminAgent.Instance().SetOnlineStatus(userId, false);
                    break;
                case UserType.User:
                    UserAgent.Instance().SetOnlineStatus(userId, false);

                    foreach (string friend in UserAgent.Instance().GetFriends(userId).Select(t => string.Concat(UserAgent.IM_USER, "-", t)))
                    {
                        if (SysSetting.GetSetting().Client.ContainsKey(friend))
                        {
                            string key = UserAgent.Instance().GetTalkKey(this.ID, friend);
                            SysSetting.GetSetting().Client[friend].Send(new OfflineMessage(key));
                        }
                    }
                    break;
            }
        }
    }
}
