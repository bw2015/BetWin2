using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SP.Studio.Web;
using SP.Studio.Json;
using BW.Agent;

namespace BW.Common.Users
{
    partial class ChatLog
    {
        public ChatLog() { }

        public ChatLog(string data)
            : this(JsonAgent.GetJObject(data))
        {
        }

        public ChatLog(Hashtable ht)
        {
            // 信息发送者
            Hashtable mine = JsonAgent.GetJObject(ht["mine"].ToString());
            // 信息接收者
            Hashtable to = JsonAgent.GetJObject(ht["to"].ToString());

            this.CreateAt = DateTime.Now;
            this.Content = mine["content"].ToString();
        }

        /// <summary>
        /// 消息结构
        /// </summary>
        /// <param name="userId">信息接收者</param>
        /// <param name="sendId">信息发送者</param>
        /// <param name="content">内容</param>
        public ChatLog(string userId, string sendId, string content)
        {
            this.UserID = userId;
            this.SendID = sendId;
            this.Content = content;
            this.CreateAt = DateTime.Now;
        }

        /// <summary>
        /// 格式化通知客户端的json格式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Action\":\"Message\"")
                .AppendFormat(",\"ID\":{0}", this.ID)
                .AppendFormat(",\"Name\":\"{0}\"", this.SendName)//消息来源用户名
                .AppendFormat(",\"Avatar\":\"{0}\"", this.SendAvatar)//消息来源用户头像
                .AppendFormat(",\"Key\":\"{0}\"", this.Key) //聊天窗口来源ID（如果是私聊，则是用户id，如果是群聊，则是群组id）
                .AppendFormat(",\"type\":\"friend\"")//聊天窗口来源类型，从发送消息传递的to里面获取
                .AppendFormat(",\"Content\":\"{0}\"", HttpUtility.JavaScriptStringEncode(this.Content)) //消息内容
                //.AppendFormat(",\"ID\":{0}", this.ID) //消息id，可不传。除非你要对消息进行一些操作（如撤回）
                .AppendFormat(",\"SendID\":\"{0}\"", this.SendID)//消息来源者的id，可用于自动解决浏览器多窗口时的一些问题
                .AppendFormat(",\"Time\":{0}", WebAgent.GetTimeStamps(this.CreateAt))//服务端动态时间戳
                .Append("}");

            return sb.ToString();
        }
    }
}
