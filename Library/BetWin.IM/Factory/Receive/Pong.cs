using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading.Tasks;

using BW.IM.Agent;
using BW.IM.Common;

namespace BW.IM.Factory.Receive
{
    /// <summary>
    /// 收到了用户的在线心跳
    /// </summary>
    public class Pong : IReceive
    {
        public Pong(User user, Hashtable ht) : base(user, ht) { }

        public override void Run()
        {

        }
    }
}
