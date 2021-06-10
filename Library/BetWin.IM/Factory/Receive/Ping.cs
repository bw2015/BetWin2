using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

using BW.IM.Common;

namespace BW.IM.Factory.Receive
{
    /// <summary>
    /// 收到客户端的ping包
    /// </summary>
    public class Ping : IReceive
    {
        public Ping(User user, Hashtable ht) : base(user, ht) { }

        /// <summary>
        /// 回复一个pong包
        /// </summary>
        public override void Run()
        {
            Utils.Send(this.UserInfo.KEY, new BW.IM.Factory.Message.Pong()
            {
                Online = Utils.USERLIST.Count
            });
        }
    }
}
