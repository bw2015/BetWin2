using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IMService.Agent;

namespace IMService.Common.Send
{
    /// <summary>
    /// 会员上线
    /// </summary>
    public class OnlineMessage : ISend
    {
        public string TalkKey { get; set; }

        public OnlineMessage(string key)
        {
            this.TalkKey = key;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .AppendFormat("\"Action\":\"{0}\"", SendType.Online)
                .AppendFormat(",\"id\":\"{0}\"", this.TalkKey)
                .Append("}");
            return sb.ToString();
        }
    }
}
