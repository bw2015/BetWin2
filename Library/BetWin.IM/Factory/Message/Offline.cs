using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BW.IM.Factory.Message
{
    /// <summary>
    /// 通知好友自己上线了
    /// </summary>
    public class Offline : IMessage
    {
        public Offline() { }

        /// <summary>
        /// 自己的ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 好友的ID
        /// </summary>
        public string FriendID { get; set; }

        /// <summary>
        /// 对话Key
        /// </summary>
        public string Key
        {
            get
            {
                return Utils.GetTalkKey(this.UserID, this.FriendID);
            }
        }
    }
}
