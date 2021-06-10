using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web;

using SP.Studio.Web;
using SP.Studio.Core;
using BW.IM.Agent;

namespace BW.IM.Common
{
    /// <summary>
    /// 用户对象
    /// </summary>
    public class User
    {
        public User(DataRow dr, UserType type)
        {
            this.Type = type;
            switch (type)
            {
                case UserType.USER:
                    this.ID = (int)dr["UserID"];
                    this.Name = (string)dr["UserName"];
                    break;
                case UserType.ADMIN:
                    this.ID = (int)dr["AdminID"];
                    this.Name = (string)dr["NickName"];
                    break;
            }
            if (dr.Table.Columns.Contains("Session")) this.Session = (Guid)dr["Session"];
            if (dr.Table.Columns.Contains("Face")) this.Face = Utils.GetFace((string)dr["Face"]);
            if (dr.Table.Columns.Contains("SiteID")) this.SiteID = (int)dr["SiteID"];
            if (dr.Table.Columns.Contains("Sign")) this.Sign = (string)dr["Sign"];
            if (dr.Table.Columns.Contains("IsOnline")) this.Online = (bool)dr["IsOnline"];
            if (dr.Table.Columns.Contains("IsService")) this.IsService = (int)dr["IsService"] == 1;
        }

        public User(int siteId, int id, Guid session, string name, string sign, string face, UserType type, GroupType group, bool isService)
        {
            this.SiteID = siteId;
            this.ID = id;
            this.Session = session;
            this.Name = name;
            this.Sign = sign;
            this.Face = face;
            this.Type = type;
            this.Group = group;
            this.IsService = isService;
            this.Online = isService;
        }

        /// <summary>
        /// 所属站点
        /// </summary>
        public int SiteID { get; private set; }

        /// <summary>
        /// 数据库内编号
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// 在线的session值
        /// </summary>
        public Guid Session { get; private set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Face { get; private set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType Type { get; private set; }

        /// <summary>
        /// 是否在群当中
        /// </summary>
        public GroupType Group { get; private set; }

        /// <summary>
        /// 是否是客服角色
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Sign { get; set; }

        /// <summary>
        /// 当前是否在线
        /// </summary>
        public bool Online { get; set; }

        /// <summary>
        /// 活动时间
        /// </summary>
        public DateTime ActiveAt { get; set; }

        /// <summary>
        /// 信息发送的连续错误次数
        /// </summary>
        public int Error { get; set; }


        /// <summary>
        /// IM key值
        /// </summary>
        public string KEY
        {
            get
            {
                return string.Concat(this.Type, "-", this.ID);
            }
        }

        public void SetGroup(GroupType type)
        {
            this.Group = type;
        }

        /// <summary>
        /// 初始化layim需要的JSON数据
        /// </summary>
        /// <returns></returns>
        public string Init()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{")
                .AppendFormat("\"code\": 0") //0表示成功，其它表示失败
                .AppendFormat(",\"msg\": \"{0}\"", this.Name) //失败信息
                .Append(",\"data\": {")
                    .Append("\"mine\": {")
                    .AppendFormat("\"username\": \"{0}\"", this.Name) //我的昵称
                    .AppendFormat(",\"id\": \"{0}\"", this.KEY) //我的ID
                    .AppendFormat(",\"status\": \"online\"") //在线状态 online：在线、hide：隐身
                    .AppendFormat(",\"sign\": \"{0}\"", HttpUtility.JavaScriptStringEncode(this.Sign)) //我的签名
                    .AppendFormat(",\"avatar\": \"{0}\"", this.Face) //我的头像
                    .Append("}")
             .Append(",\"friend\": [");
            switch (this.Type)
            {
                case UserType.USER:
                    //好友列表
                    sb.Append("{")
                        .Append("\"groupname\": \"客服\"") //好友分组名
                        .Append(",\"id\": 1") //分组ID
                        .AppendFormat(" ,\"list\": [{0}]", string.Join(",", UserAgent.Instance().GetServiceList(this.SiteID).Select(t => t.ToString(this.KEY))))
                    .Append("}");
                    User parent = UserAgent.Instance().GetParent(this.ID);
                    if (parent != null)
                    {
                        sb.Append(",{")
                              .Append("\"groupname\": \"上级\"") //好友分组名
                              .Append(",\"id\": 2") //分组ID
                              .AppendFormat(" ,\"list\": [{0}]", parent.ToString(this.KEY))
                        .Append("}");
                    }
                    sb.Append(",{")
                        .Append("\"groupname\": \"下级\"") //好友分组名
                        .Append(",\"id\": 3") //分组ID
                        .AppendFormat(" ,\"list\": [{0}]", string.Join(",", UserAgent.Instance().GetChildList(this.ID).Select(t => t.ToString(this.KEY))))
                    .Append("}");
                    break;
                case UserType.ADMIN:
                    break;
            }
            sb.Append("]");
            if (this.Type == UserType.ADMIN)
            {
                sb.AppendFormat(",\"group\":[");
                sb.Append(string.Join(",",
                typeof(GroupType).ToList().Where(t => t.ID != 0).Select(t =>
                {
                    string lotteryName = SiteAgent.Instance().GetLotteryName((GroupType)t.ID);
                    if (string.IsNullOrEmpty(lotteryName)) return string.Empty;
                    return string.Format("{{\"groupname\":\"{0}\",\"id\":\"{1}\",\"avatar\":\"{2}\"}}", lotteryName,
                       Utils.GetTalkKey(((GroupType)t.ID).GetKey(), this.KEY), Utils.GetFace("/images/group/" + t.Name.ToLower() + ".png"));
                }).Where(t => !string.IsNullOrEmpty(t))
                    ));
                sb.Append("]");
            }
            sb.Append("}    }");

            return sb.ToString();
        }

        /// <summary>
        /// 初始化一个群聊窗口
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string Init(GroupType type)
        {
            StringBuilder sb = new StringBuilder();
            string lotteryName = SiteAgent.Instance().GetLotteryName(type);

            sb.Append("{")
                .AppendFormat("\"code\": 0") //0表示成功，其它表示失败
                .AppendFormat(",\"msg\": \"{0}\"", this.Name) //失败信息
                .Append(",\"data\": {")
                    .Append("\"mine\": {")
                    .AppendFormat("\"username\": \"{0}\"", this.Name) //我的昵称
                    .AppendFormat(",\"id\": \"{0}\"", this.KEY) //我的ID
                    .AppendFormat(",\"status\": \"online\"") //在线状态 online：在线、hide：隐身
                    .AppendFormat(",\"sign\": \"{0}\"", HttpUtility.JavaScriptStringEncode(this.Sign)) //我的签名
                    .AppendFormat(",\"avatar\": \"{0}\"", this.Face) //我的头像
                    .Append("}")
             .Append(",\"friend\": []")
             .AppendFormat(",\"group\":[")
             .AppendFormat("{{\"groupname\":\"{0}\",\"id\":\"{1}\",\"avatar\":\"{2}\"}}",
                        lotteryName, Utils.GetTalkKey(type.GetKey(), this.KEY),
                        Utils.GetFace("/images/group/" + type.ToString().ToLower() + ".png"))
                        .Append("]")
                       .Append("}   }");

            return sb.ToString();
        }

        /// <summary>
        /// layim好友列表需要的json格式
        /// </summary>
        /// <param name="key">自身的key值</param>
        /// <returns></returns>
        public string ToString(string key)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"username\": \"{0}\"", this.Name) //好友昵称
                .AppendFormat(",\"id\": \"{0}\"", Utils.GetTalkKey(key, this.KEY)) //好友ID
                .AppendFormat(",\"avatar\": \"{0}\"", this.Face) //好友头像
                .AppendFormat(",\"sign\": \"{0}\"", this.Sign) //好友签名
                .AppendFormat(",\"status\": \"{0}\"", this.Online ? "online" : "offline") //若值为offline代表离线，online或者不填为在线
            .Append("}");
            return sb.ToString();
        }
    }

    public enum UserType : byte
    {
        /// <summary>
        /// 用户
        /// </summary>
        USER,
        /// <summary>
        /// 管理员
        /// </summary>
        ADMIN
    }
}
