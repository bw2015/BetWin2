using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMService.Common.Send
{
    /// <summary>
    /// 下线通知
    /// </summary>
    public class OfflineMessage : ISend
    {
        public string TalkKey { get; set; }

        public OfflineMessage(string key)
        {
            this.TalkKey = key;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Action\":\"{0}\"", SendType.Offline)
                .AppendFormat(",\"id\":\"{0}\"", this.TalkKey)
                .Append("}");
            return sb.ToString();
        }
    }
}
