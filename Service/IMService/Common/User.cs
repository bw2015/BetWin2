using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using IMService.Common.Message;

using IMService.Framework;
using Fleck;

namespace IMService.Common
{
    /// <summary>
    /// IM系统的用户表
    /// </summary>
    public struct User
    {
        public User(Online online)
        {
            this.UserID = online.UserID;
            this.UserName = online.Name;
            this.NickName = online.Name;
            this._face = online.Avatar;
            this.Session = online.Session;
            this.Socket = online.Socket;
        }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName;

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName;

        /// <summary>
        /// 头像
        /// </summary>
        private string _face;

        /// <summary>
        /// 在线标识KEY
        /// </summary>
        public Guid Session;

        /// <summary>
        /// 头对外显示的像
        /// </summary>
        public string Face
        {
            get
            {
                if (string.IsNullOrEmpty(_face)) return Utils.GetImage("/images/user.jpg");
                return Utils.GetImage(this._face);
            }
        }

        /// <summary>
        /// 对外显示的名字
        /// </summary>
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(this.NickName) ? this.UserName : this.NickName;
            }
        }

        /// <summary>
        /// 当前用户关联的socket列表
        /// </summary>
        public IWebSocketConnection Socket;

        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="message"></param>
        public void Send(ISend message)
        {
            if (!this.Socket.IsAvailable) return;
            this.Socket.Send(message.ToString());
        }

    }
}
