using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;
using SP.Studio.Array;
using IMService.Framework;

using IMService.Agent;
using IMService.Common;
using IMService.Common.Send;

using BW.Agent;
using BW.Common.Users;

namespace IMService.Common.Message
{
    /// <summary>
    /// 用户发送的信息
    /// </summary>
    public class Receive : IMessage
    {
        public Receive(Hashtable ht, IWebSocketConnection socket)
            : base(ht, socket)
        {
            this.To = ht.GetValue("to", string.Empty);
            this.Content = ht.GetValue("content", string.Empty);
            this.Rebot = ht.GetValue("rebot", 0) == 1;
        }

        /// <summary>
        /// 信息的接收者
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 信息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 是否是发给机器人
        /// </summary>
        public bool Rebot { get; set; }

        /// <summary>
        /// 收到信息，把信息发给接收者
        /// </summary>
        /// <param name="socket"></param>
        public override void Run()
        {
            if (string.IsNullOrEmpty(To) || string.IsNullOrEmpty(this.Content)) return;

            ChatLog log = new ChatLog()
            {
                SiteID = SysSetting.GetSetting().SiteID,
                Key = this.To,
                Content = this.Content,
                SendID = this.ID,
                SendName = this.Name,
                SendAvatar = this.Avatar
            };

            //#1 保存进入数据库
            int logId = UserAgent.Instance().SaveChatLog(log);

            // 如果是发给客服
            if (log.UserID == "ADMIN-0")
            {
                //# 判断是否是机器人
                if (this.Rebot)
                {
                    UserAgent.Instance().UpdateChatLogRead(logId, log.UserID);

                    string reply = IMService.Rebot.tuling.GetResult(log.Content, log.SendID);
                    log = new ChatLog()
                    {
                        SiteID = SysSetting.GetSetting().SiteID,
                        Key = this.To,
                        Content = reply,
                        SendID = log.UserID,
                        SendName = SysSetting.GetSetting().SiteInfo.Rebot.Name,
                        SendAvatar = SysSetting.GetSetting().SiteInfo.Rebot.FaceShow
                    };
                    logId = UserAgent.Instance().SaveChatLog(log);
                }
                else
                {
                    //#1.1 查找在线的客服
                    foreach (string key in AdminAgent.Instance().GetServiceAdmin(SysSetting.GetSetting().SiteID).Select(t => string.Concat(UserAgent.IM_ADMIN, "-", t)))
                    {
                        // 如果有客服在线
                        if (SysSetting.GetSetting().Client.ContainsKey(key))
                        {
                            log.UserID = key;
                            UserAgent.Instance().UpdateChatLogUserID(logId, key);
                            break;
                        }
                    }
                }
            }

            //#2.1 如果在线的话发送给接收者
            if (SysSetting.GetSetting().Client.ContainsKey(log.UserID))
            {
                SysSetting.GetSetting().Client[log.UserID].Socket.Send(new SendMessage(log).ToString());
            }
            else
            {
                // 2.2 如果不在线发送不在线通知
                this.Socket.Send(new TipMessage("对方不在线，将会在上线之后收到您的信息", TipMessage.MethodType.None).ToString());
            }
        }
    }
}
