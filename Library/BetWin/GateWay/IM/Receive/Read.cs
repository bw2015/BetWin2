using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BW.Agent;

namespace BW.GateWay.IM.Receive
{
    public class Read : IReceive
    {
        public Read(Hashtable ht)
            : base(ht)
        {

        }

        /// <summary>
        /// 信息编号
        /// </summary>
        public int LogID { get; set; }

        public override void Run()
        {
            if (LogID != 0)
            {
                UserAgent.Instance().UpdateChatLogRead(this.LogID);
            }
        }
    }
}
