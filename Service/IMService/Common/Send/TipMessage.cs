using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMService.Common.Send
{
    /// <summary>
    /// 信息提醒
    /// </summary>
    public class TipMessage : ISend
    {
        public TipMessage(string msg)
        {
            this.Msg = msg;
        }

        public TipMessage(string msg, MethodType method)
            : this(msg)
        {
            this.Method = method;
        }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 要进行的客户端动作
        /// </summary>
        public MethodType Method { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Action\":\"{0}\"", SendType.Tip)
                .AppendFormat(",\"msg\":\"{0}\"", this.Msg)
                .AppendFormat(",\"Method\":\"{0}\"", this.Method)
                .Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 提醒后要进行的动作
        /// </summary>
        public enum MethodType
        {
            /// <summary>
            /// 没有动作
            /// </summary>
            None
        }
    }
}
