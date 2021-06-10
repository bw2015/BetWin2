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
    /// 标记已读
    /// </summary>
    public class Read : IReceive
    {
        public Read(User user, Hashtable ht) : base(user, ht) { }

        /// <summary>
        /// 信息编号
        /// </summary>
        public int MsgID { get; set; }

        /// <summary>
        /// 通知编号
        /// </summary>
        public int NotifyID { get; set; }

        public override void Run()
        {
            if (this.MsgID != 0)
            {
                UserAgent.Instance().UpdateMessageRead(this.UserInfo, this.MsgID);
            }
            if (this.NotifyID != 0)
            {
                UserAgent.Instance().UpdateNotifyRead(this.UserInfo, this.NotifyID);
            }
        }
    }
}
