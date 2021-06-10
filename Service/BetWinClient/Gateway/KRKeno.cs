using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

using SP.Studio.Net;
using SP.Studio.Xml;
using System.Xml.Linq;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 韩国1.5分
    /// </summary>
    public partial class KRKeno : IGateway
    {
        protected override Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            return dic.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
        }
    }
}
