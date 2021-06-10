using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.IM.Factory.Message
{
    /// <summary>
    /// 信息对象
    /// </summary>
    public class Message : IMessage
    {
        /// <summary>
        /// 发送者头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 信息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 信息编号
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 对话Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 发送者名字
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// 信息发送者
        /// </summary>
        public string SendID { get; set; }

        /// <summary>
        /// 类型 （群）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 通知编号
        /// </summary>
        public int NotifyID { get; set; }
    }
}
