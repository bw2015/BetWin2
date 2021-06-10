using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;
using BW.Common.Users;
using SP.Studio.Web;


namespace BW.GateWay.IM.Receive
{
    /// <summary>
    /// 群登录信息
    /// </summary>
    public class Group : IReceive
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public string Key { get; set; }

        public Group(Hashtable ht) : base(ht) { }

        public override void Run()
        {
            string groupId = UserAgent.Instance().GetChatUserID(this.Key, this.ID);
            if (string.IsNullOrEmpty(groupId)) return;
            string type;
            ChatTalk.GroupType groupType = (ChatTalk.GroupType)UserAgent.Instance().GetChatUserID(groupId, out type);
            if (type != UserAgent.IM_GROUP) return;
        }
    }
}
