using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.IM.Agent;
using BW.IM.Common;

using SP.Studio.Web;
using BW.IM.Factory.Command;


namespace BW.IM.Factory.Receive
{
    public class Message : IReceive
    {
        public Message(User user, Hashtable ht) : base(user, ht) { }

        /// <summary>
        /// 信息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 会话KEY
        /// </summary>
        public string TalkKey { get; set; }

        public override void Run()
        {
            string userId;
            string talkKey = this.TalkKey;
            // #1 存入数据库
            int msgId = UserAgent.Instance().SaveMessage(this.UserInfo.SiteID, this.UserInfo.Type, this.UserInfo.ID, this.UserInfo.KEY, ref talkKey, this.UserInfo.Name, this.UserInfo.Face, this.Content, out userId);
           

            if (msgId == 0)
            {
                Utils.Send(this.UserInfo.KEY, new BW.IM.Factory.Message.Tip()
                {
                    Content = "信息发送失败",
                    Key = this.TalkKey
                });
                return;
            }

            if (this.UserInfo.Type == UserType.ADMIN)
            {
                int commandValue;
                CommandMessage commandType = this.Content.GetCommand(out commandValue);
                bool isCommandBreak = false;
                int uid = int.Parse(Utils.GetChatID(userId));
                switch (commandType)
                {
                    case CommandMessage.BLOCK:
                        new BLOCK(this.UserInfo, commandValue, uid, GroupType.None);
                        isCommandBreak = true;
                        break;
                }

                if (isCommandBreak) return;
                // 如果开启了客服合并才变更对话key
                if (Utils.CUSTOMERSERVICE.ContainsKey(this.UserInfo.SiteID) && Utils.CUSTOMERSERVICE[this.UserInfo.SiteID].IsOpen)
                {
                    talkKey = Utils.GetTalkKey(userId, Utils.SERVICE, this.UserInfo.SiteID);
                }
            }

            // #3 发给接收者
            if (!Utils.Send(userId, new BW.IM.Factory.Message.Message()
            {
                Avatar = this.UserInfo.Face,
                Content = this.Content,
                Key = talkKey,
                Name = this.UserInfo.Name,
                SendID = this.UserInfo.KEY,
                Time = WebAgent.GetTimeStamps(),
                ID = msgId
            }))
            {
                Utils.Send(this.UserInfo.KEY, new BW.IM.Factory.Message.Tip()
                {
                    Content = "对方不在线，将在上线后收到您的信息",
                    Key = this.TalkKey,
                    Type = Factory.Message.Tip.TipType.Offline
                });
            }
        }
    }
}
