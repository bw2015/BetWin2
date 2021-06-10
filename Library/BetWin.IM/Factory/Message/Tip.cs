using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BW.IM.Factory.Message
{
    /// <summary>
    /// 要发出的通知信息
    /// </summary>
    public class Tip : IMessage
    {
        /// <summary>
        /// 通知内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 对话ID
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///  信息提示类型
        /// </summary>
        public TipType Type { get; set; }

        /// <summary>
        /// 对话类型（好友、群）
        /// </summary>
        public string ChatType { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public long Time { get; set; }

        public enum TipType
        {
            None,
            /// <summary>
            /// 信息发送失败
            /// </summary>
            Error,
            /// <summary>
            /// 投注错误
            /// </summary>
            BetError,
            /// <summary>
            /// 信息的接收方不在线
            /// </summary>
            Offline,
            /// <summary>
            /// 投注提交成功
            /// </summary>
            BetSuccess,
            /// <summary>
            /// 投注提交失败
            /// </summary>
            BetFaild,
            /// <summary>
            /// 系统信息
            /// </summary>
            System
        }
    }
}
