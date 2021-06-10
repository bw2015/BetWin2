using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.IM.Message
{
    /// <summary>
    /// 要发出的信息
    /// </summary>
    public class Message : IMessage
    {
        /// <summary>
        /// 信息编号
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 对话Key值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// 发送者的ID
        /// </summary>
        public string SendID { get; set; }

        /// <summary>
        /// 信息发送类型
        /// </summary>
        public string Type { get; set; }
    }
}
