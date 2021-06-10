using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;
using Fleck;

namespace IMService.Common
{
    public class ChatUser
    {
        public ChatUser()
        {

        }

        public ChatUser(int userId, Guid session, IWebSocketConnection socket)
            : this()
        {
            this.UserID = userId;
            this.Session = session;
            this.Socket = socket;
        }


        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 在线Key值
        /// </summary>
        public Guid Session { get; set; }

        /// <summary>
        /// 在线对象
        /// </summary>
        public IWebSocketConnection Socket { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get
            {
                return UserAgent.Instance().GetUserName(this.UserID);
            }
        }

        /// <summary>
        /// 下线
        /// </summary>
        /// <param name="socket">要关闭的链接</param>
        /// <returns>如果不存在对象了则关闭</returns>
        public void Close()
        {
            this.Socket.Close();
        }

        /// <summary>
        /// 发送一条信息给该用户
        /// </summary>
        /// <param name="message">JSON数据</param>
        public void Send(string message)
        {
            if (this.Socket.IsAvailable) this.Socket.Send(message);
        }
    }
}
