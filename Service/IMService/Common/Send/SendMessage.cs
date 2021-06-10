using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using SP.Studio.Web;
using IMService.Framework;
using IMService.Agent;
using BW.Common.Users;
using BW.Agent;

namespace IMService.Common.Send
{
    /// <summary>
    /// 发送的信息
    /// </summary>
    public class SendMessage : ISend
    {
        public SendMessage(ChatLog log)
        {
            this.LogID = log.ID;
            this.Name = log.SendName;
            this.Avatar = log.SendAvatar;
            this.Content = log.Content;
            this.CreateAt = log.CreateAt;
            this.SendID = log.SendID;
            this.UserID = log.UserID;
            this.Sign = "离线信息";
            this.ID = log.Key;
        }

        /// <summary>
        /// 信息接收者的ID
        /// </summary>
        private string UserID;

        private int LogID;

        /// <summary>
        /// 名字
        /// </summary>
        private string Name;

        /// <summary>
        /// 头像路径
        /// </summary>
        private string Avatar;

        private string Content;

        private DateTime CreateAt;

        /// <summary>
        /// 发送者的ID
        /// </summary>
        private string SendID;

        /// <summary>
        /// 用户签名
        /// </summary>
        private string Sign;

        /// <summary>
        /// 会话编号
        /// </summary>
        private string ID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
               .Append("\"Action\":\"Message\"")
               .AppendFormat(",\"logid\": \"{0}\"", this.LogID)
               .AppendFormat(",\"username\": \"{0}\"", this.Name)
               .AppendFormat(",\"avatar\": \"{0}\"", this.Avatar)
               .AppendFormat(",\"id\": \"{0}\"", this.ID)
               .AppendFormat(",\"content\":\"{0}\"", HttpUtility.JavaScriptStringEncode(this.Content))
               .AppendFormat(",\"timestamp\":{0}", WebAgent.GetTimeStamps(this.CreateAt))
               .AppendFormat(",\"sign\":\"{0}\"", this.Sign)
               .Append("}");
            return sb.ToString();
        }
    }
}
