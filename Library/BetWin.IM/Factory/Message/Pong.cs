using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.IM.Factory.Message
{
    /// <summary>
    /// 收到客户端的ping包之后回复给服务器端的pong包
    /// </summary>
    public class Pong : IMessage
    {
        /// <summary>
        /// 当前在线的用户数量
        /// </summary>
        public int Online { get; set; }
    }
}
