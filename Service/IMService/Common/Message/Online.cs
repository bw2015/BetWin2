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
    public class Online : IMessage
    {
        public Online(Hashtable ht, IWebSocketConnection socket)
            : base(ht, socket)
        {

        }

        /// <summary>
        /// 上线之后要做的操作
        /// </summary>
        public override void Run()
        {
            // 更新在线链接的ID
            if (SysSetting.GetSetting().Online.ContainsKey(this.Socket))
            {
                SysSetting.GetSetting().Online[this.Socket].ID = this.ID;
            }

            // 设置在线状态 同时更新缓存
            switch (this.Type)
            {
                case UserType.Admin:
                    AdminAgent.Instance().SetOnlineStatus(this.UserID, true);
                    break;
                case UserType.User:
                    UserAgent.Instance().SetOnlineStatus(this.UserID, true);
                    break;
            }

            // 通知在线用户
            if (this.Type == UserType.User)
            {
                foreach (string key in UserAgent.Instance().GetFriends(this.UserID).Select(t => string.Concat(UserAgent.IM_USER, "-", t)))
                {
                    // 发送上线通知给好友
                    if (SysSetting.GetSetting().Client.ContainsKey(key))
                    {
                        string talkKey = UserAgent.Instance().GetTalkKey(this.ID, key);
                        SysSetting.GetSetting().Client[key].Socket.Send(new OnlineMessage(talkKey).ToString());
                    }
                }
            }

            // 更新缓存
            if (SysSetting.GetSetting().Client.ContainsKey(this.ID))
            {
                SysSetting.GetSetting().Client[this.ID].Socket.Close();
                SysSetting.GetSetting().Client[this.ID] = new User(this);
            }
            else
            {
                SysSetting.GetSetting().Client.Add(this.ID, new User(this));
            }

            //领取未读信息
            UserAgent.Instance().GetChatLogUnread(SysSetting.GetSetting().SiteID, this.ID).ForEach(t =>
            {
                this.Socket.Send(new SendMessage(t).ToString());
            });
        }
    }
}
