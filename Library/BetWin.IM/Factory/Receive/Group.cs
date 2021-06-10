using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BW.IM.Agent;
using BW.IM.Common;
using BW.IM.Framework;
using BW.IM.Factory.Message;
using BW.IM.Factory.Command;

using SP.Studio.Core;
using SP.Studio.Net;
using SP.Studio.Json;
using System.Net;

namespace BW.IM.Factory.Receive
{
    /// <summary>
    /// 收到群消息
    /// </summary>
    public class Group : IReceive
    {
        public Group(User user, Hashtable ht) : base(user, ht) { }

        /// <summary>
        /// 投注内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 唯一的信息编号
        /// </summary>
        public long MsgID { get; set; }

        /// <summary>
        /// 会话KEY
        /// </summary>
        public string TalkKey { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Time { get; set; }

        public override void Run()
        {
            if (SysSetting.GetSetting().CommandList == null)
            {
                Utils.Send(this.UserInfo.KEY, new Tip()
                {
                    Content = "信息发送失败，请重试",
                    Key = this.TalkKey,
                    Type = Tip.TipType.Error,
                    ChatType = "group",
                    Time = this.Time
                });
                return;
            }

            string userId;
            // #1 存入数据库
            int msgId = UserAgent.Instance().SaveMessage(this.UserInfo.KEY, this.TalkKey, this.UserInfo.Name, this.UserInfo.Face, this.Content, out userId);

            if (Utils.GetChatType(userId) != ChatType.GROUP)
            {
                Utils.Send(this.UserInfo.KEY, new Tip()
                {
                    Content = "未能识别群类型",
                    Key = this.TalkKey,
                    Type = Tip.TipType.Error,
                    ChatType = "group"
                });
                return;
            }

            string groupType = Utils.GetChatID(userId);
            GroupType type = groupType.ToEnum<GroupType>();


            if (!SysSetting.GetSetting().CommandList.ContainsKey(type))
            {
                Utils.Send(this.UserInfo.KEY, new Tip()
                {
                    Content = "未能识别群类型",
                    Key = Utils.GetTalkKey(type.GetKey(), UserInfo.KEY),
                    Type = Tip.TipType.Error,
                    ChatType = "group"
                });
                return;
            }

            if (this.UserInfo.Type == UserType.ADMIN)
            {
                int commandValue;
                CommandMessage commandType = this.Content.GetCommand(out commandValue);
                bool isCommandBreak = false;
                // 判断是否是命令
                switch (commandType)
                {
                    case CommandMessage.BLOCK:
                        new BLOCK(this.UserInfo, commandValue, 0, type);
                        isCommandBreak = true;
                        break;
                }

                if (isCommandBreak) return;
            }

            //#1 判断是否是投注
            List<string> commandlist = SysSetting.GetSetting().CommandList[type];

            int groupId = Utils.GetGroupID(this.UserInfo.SiteID, type);

            if (this.UserInfo.Type == UserType.USER && commandlist.Exists(t => Regex.IsMatch(this.Content, t)))
            {
                string url = string.Format("{0}/handler/game/wechat/bet", SysSetting.GetSetting().handlerServer);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("SITEID", this.UserInfo.SiteID.ToString());
                    wc.Headers.Add(HttpRequestHeader.Cookie, "USER=" + this.UserInfo.Session.ToString("N"));
                    string data = string.Format("Content={0}&Type={1}", this.Content, type);
                    string userKey = this.UserInfo.KEY;
                    string userFace = this.UserInfo.Face;
                    int siteId = this.UserInfo.SiteID;
                    string sendName = this.UserInfo.Name;

                    // 管理员是否不接受群信息
                    bool betMessage = Utils.GROUPSETTING.ContainsKey(groupId) && Utils.GROUPSETTING[groupId].BetMessage;

                    Utils.SendGroup(type, this.Content, this.UserInfo.SiteID, this.UserInfo.ID, this.UserInfo.Face, this.UserInfo.Name, this.UserInfo.KEY, !betMessage);

                    NetAgent.UploadDataSync(url, data, (sender, e) =>
                    {
                        string result = Encoding.UTF8.GetString(e.Result);
                        Hashtable ht = JsonAgent.GetJObject(result);

                        string replyContent = string.Format("@{0} {1}", sendName, ht.ContainsKey("msg") ? ht["msg"].ToString() : result);
                        Utils.Send(userKey, Utils.SystemReply(this.TalkKey, replyContent, "group"));

                        Utils.SendGroup(type, replyContent, siteId, 0, Utils.SYSTEM_FACE, Utils.SYSTEM_NAME, Utils.SYSTEM_ID, !betMessage);

                    }, Encoding.UTF8, wc);
                }
            }
            else
            {
                if (this.UserInfo.Type == UserType.USER)
                {
                    if (Utils.GROUPSETTING.ContainsKey(groupId) && !Utils.GROUPSETTING[groupId].Chat)
                    {
                        Utils.Send(this.UserInfo.KEY, new Tip()
                        {
                            Content = "未开放聊天",
                            Key = this.TalkKey,
                            Type = Tip.TipType.System,
                            ChatType = "group",
                            Time = this.Time
                        });
                        return;
                    }
                    DateTime blockAt = DateTime.MinValue;
                    if (Utils.BLOCKUSER.ContainsKey(this.UserInfo.ID)) blockAt = Utils.BLOCKUSER[this.UserInfo.ID];
                    if (Utils.BLOCKUSER.ContainsKey(groupId)) blockAt = blockAt > Utils.BLOCKUSER[groupId] ? blockAt : Utils.BLOCKUSER[groupId];

                    if (blockAt > DateTime.Now)
                    {
                        int blockTime = (int)((TimeSpan)(blockAt - DateTime.Now)).TotalSeconds;
                        Utils.Send(this.UserInfo.KEY, new Tip()
                        {
                            Content = string.Format("您需要在{0}秒之后才可发言", blockTime),
                            Key = this.TalkKey,
                            Type = Tip.TipType.System,
                            ChatType = "group",
                            Time = this.Time
                        });
                        return;
                    }
                }

                Utils.SendGroup(type, this.Content, this.UserInfo.SiteID, this.UserInfo.ID, this.UserInfo.Face, this.UserInfo.Name, this.UserInfo.KEY);
            }
        }

    }
}
