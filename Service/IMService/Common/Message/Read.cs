using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

using SP.Studio.Array;

using IMService.Framework;
using IMService.Common;
using IMService.Common.Send;

using IMService.Agent;

using BW.Agent;

using Fleck;

namespace IMService.Common.Message
{
    /// <summary>
    /// 收到一条标记已读的信息
    /// </summary>
    public class Read : IMessage
    {
        public Read(Hashtable ht, IWebSocketConnection stocket)
            : base(ht, stocket)
        {
            this.LogID = ht.GetValue("LogID", 0);
        }

        /// <summary>
        /// 信息编号
        /// </summary>
        private int LogID;

        /// <summary>
        /// 标记信息为已读
        /// </summary>
        /// <param name="socket"></param>
        public override void Run()
        {
            UserAgent.Instance().UpdateChatLogRead(this.LogID, this.ID);
        }
    }
}
