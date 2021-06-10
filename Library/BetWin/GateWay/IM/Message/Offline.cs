using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;


namespace BW.GateWay.IM.Message
{
    /// <summary>
    /// 下线信息
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
                return UserAgent.Instance().GetTalkKey(this.UserID, this.FriendID);
            }
        }
    }
}
