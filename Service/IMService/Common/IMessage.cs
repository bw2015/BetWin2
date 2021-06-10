using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

using SP.Studio.Json;
using SP.Studio.Array;
using SP.Studio.Core;

using IMService.Common.Message;

using Fleck;

namespace IMService.Common
{

    /// <summary>
    /// 收到的信息工厂
    /// </summary>
    public static class MessageFactory
    {
        public static IMessage GetMessage(string json, IWebSocketConnection socket)
        {
            Hashtable ht = JsonAgent.GetJObject(json);
            if (ht == null) return null;
            IMessage msg = null;
            switch (ht.GetValue("action", string.Empty).ToEnum<ActionType>())
            {
                case ActionType.Online:
                    msg = new Online(ht, socket);
                    break;
                case ActionType.Receive:
                    msg = new Receive(ht, socket);
                    break;
                case ActionType.Read:
                    msg = new Read(ht, socket);
                    break;

            }
            return msg;
        }
    }

    /// <summary>
    /// 接收到信息的基类
    /// </summary>
    public abstract class IMessage
    {
        public IMessage() { }

        public IMessage(Hashtable ht, IWebSocketConnection socket)
        {
            this.Action = ht.GetValue("action", string.Empty).ToEnum<ActionType>();
            this.Session = ht.GetValue("session", Guid.Empty);
            this.Sign = ht.GetValue("sign", string.Empty);
            this.ID = ht.GetValue("id", string.Empty);
            this.Avatar = ht.GetValue("avatar", string.Empty);
            this.Name = ht.GetValue("name", string.Empty);
            this.Socket = socket;
        }

        /// <summary>
        /// 信息类型
        /// </summary>
        public ActionType Action { get; set; }


        /// <summary>
        /// 用户身份标识
        /// </summary>
        public Guid Session { get; set; }

        /// <summary>
        /// 用户的签名
        /// </summary>
        public string Sign { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType Type
        {
            get
            {
                int userId;
                UserType type = Utils.GetUserType(this.ID, out userId);
                this.UserID = userId;
                return type;
            }
        }

        /// <summary>
        /// Socket链接对象
        /// </summary>
        public IWebSocketConnection Socket { get; private set; }

        /// <summary>
        /// 执行信息体方法
        /// </summary>
        public abstract void Run();

    }

    /// <summary>
    /// 信息的动作类型
    /// </summary>
    public enum ActionType
    {
        None,
        /// <summary>
        /// 上线
        /// </summary>
        Online,
        /// <summary>
        /// 接收到信息
        /// </summary>
        Receive,
        /// <summary>
        /// 已读信息
        /// </summary>
        Read,
        /// <summary>
        /// 离线
        /// </summary>
        Offline,
        /// <summary>
        /// 请求切换到人工客服
        /// </summary>
        Staff,
    }

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType : byte
    {
        None = 0,
        /// <summary>
        /// 用户  0
        /// </summary>
        User = 1,
        /// <summary>
        /// 管理员 1
        /// </summary>
        Admin = 2,
        /// <summary>
        /// 游客  2
        /// </summary>
        Guest = 3,
        /// <summary>
        /// 机器人 3
        /// </summary>
        Rebot = 4
    }
}
