using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Admins;
using BW.Common.Sites;

using BW.Framework;

using SP.Studio.Web;
using SP.Studio.Core;


namespace BW.Common.IM
{
    /// <summary>
    /// 自己的信息
    /// </summary>
    public class IMUser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public IMUser(ChatLog.UserType type)
        {
            switch (type)
            {
                case ChatLog.UserType.Admin:
                    this.Name = "在线客服";
                    this.ID = string.Concat(UserAgent.IM_ADMIN, "-0");
                    this.Sign = "";
                    this.Face = SysSetting.GetSetting().GetImage(string.Format("/images/{0}.jpg", type.ToString().ToLower()));
                    break;
            }
        }

        /// <summary>
        /// 机器人
        /// </summary>
        /// <param name="rebot"></param>
        public IMUser(Site.RebotSetting rebot)
        {
            this.Name = rebot.Name;
            this.ID = string.Concat(UserAgent.IM_ADMIN, "-0");
            this.Sign = "客服";
            this.Face = rebot.FaceShow;
            this.Online = true;
        }

        /// <summary>
        /// 构建用户数据数据
        /// </summary>
        /// <param name="user"></param>
        /// <param name="self">是否是自己（如果是用户与用户聊天的话，用户是不可以看到对方的IP信息）</param>
        public IMUser(User user, bool self = true, string name = null)
        {
            if (user == null)
            {
                this.Name = "游客";
                this.ID = string.Concat(UserAgent.IM_GUEST, "-", UserAgent.Instance().GetBowserID().ToString("N").ToLower());
                this.Sign = string.Format("{0}({1})", IPAgent.IP, IPAgent.GetAddress()); ;
                this.Face = SysSetting.GetSetting().GetImage("/images/guest.jpg");
                this.Online = true;
            }
            else
            {
                this.Name = string.IsNullOrEmpty(name) ? user.UserName : name;
                this.ID = user.IMID;
                this.Sign = self ? string.Format("{0}({1})", user.LoginIP, UserAgent.Instance().GetIPAddress(user.LoginIP)) : string.Format("({0})", user.Name);
                this.Face = user.FaceShow;
                this.Online = user.IsOnline;
            }
        }

        public IMUser(Admin admin)
        {
            this.Name = admin.Name;
            this.ID = admin.IMID;
            this.Sign = admin.GroupInfo.Name;
            this.Face = admin.FaceShow;
            this.Online = admin.IsOnline;
        }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Face { get; set; }

        /// <summary>
        /// 是否在线
        /// </summary>
        public bool Online { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
               .AppendFormat("\"username\":\"{0}\"", this.Name)
               .AppendFormat(",\"id\":\"{0}\"", this.ID)
               .AppendFormat(",\"avatar\":\"{0}\"", this.Face)
               .AppendFormat(",\"sign\":\"{0}\"", this.Sign)
               .AppendFormat(",\"status\":\"{0\"", this.Online ? "online" : "offline")
               .Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 可以自定义参数的json转换
        /// </summary>
        /// <param name="args">Key,Value 形式</param>
        /// <returns></returns>
        public string ToString(params object[] args)
        {
            // 当前的用户标识
            string user = string.Empty;
            if (args.Length % 2 != 0)
            {
                user = args[0].ToString();
                args = args.Skip(1).ToArray();
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
               .AppendFormat("\"username\":\"{0}\"", this.Name)
               .AppendFormat(",\"id\":\"{0}\"", string.IsNullOrEmpty(user) ? this.ID : UserAgent.Instance().GetTalkKey(user, this.ID))
               .AppendFormat(",\"avatar\":\"{0}\"", this.Face)
               .AppendFormat(",\"sign\":\"{0}\"", this.Sign)
               .AppendFormat(",\"status\":\"{0}\"", this.Online ? "online" : "offline");
            for (int i = 0; i < args.Length; i += 2)
            {
                sb.AppendFormat(",\"{0}\":\"{1}\"", args[i], args[i + 1]);
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
