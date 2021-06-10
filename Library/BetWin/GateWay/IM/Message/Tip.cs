using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.GateWay.IM.Message
{
    /// <summary>
    /// 系统提示
    /// </summary>
    public class Tip : IMessage
    {
        /// <summary>
        /// 信息提示内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 会话Key值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 信息类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 图标提示
        /// </summary>
        public IconType Icon { get; set; }

        /// <summary>
        /// 信息针对的信息编号
        /// </summary>
        public long MsgId { get; set; }

        /// <summary>
        /// 提示类型
        /// </summary>
        public string TipType { get; set; }

        /// <summary>
        /// 提示的类型
        /// </summary>
        public enum IconType
        {
            None,
            /// <summary>
            /// 成功
            /// </summary>
            Success,
            /// <summary>
            /// 错误
            /// </summary>
            Error,
            /// <summary>
            /// 警告
            /// </summary>
            Waring
        }

    }
}
