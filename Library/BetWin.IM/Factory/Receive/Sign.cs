using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.IM.Agent;
using BW.IM.Common;

namespace BW.IM.Factory.Receive
{
    /// <summary>
    /// 要求修改签名
    /// </summary>
    public class Sign : IReceive
    {
        public Sign(User user, Hashtable ht) : base(user, ht) { }

        public string Content { get; set; }

        public override void Run()
        {
            UserAgent.Instance().SaveSign(UserInfo, this.Content);
        }
    }
}
