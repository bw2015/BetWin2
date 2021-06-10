using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using BW.Common.Users;

namespace IMService.Common.Send
{
    /// <summary>
    /// 通知信息
    /// </summary>
    public class NotifyMessage : ISend
    {
        public UserNotify.NotifyType NotifyType { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Action\":\"{0}\"", SendType.Notify)
                .AppendFormat(",\"Type\":\"{0}\"", this.NotifyType)
                .AppendFormat(",\"Message\":\"{0}\"", HttpUtility.JavaScriptStringEncode(this.Message))
                .Append("}");
            return sb.ToString();
        }
    }
}
