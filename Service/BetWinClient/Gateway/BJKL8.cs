using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;

using SP.Studio.Xml;
using SP.Studio.Net;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 北京快乐8
    /// </summary>
    public partial class BJKL8 : IGateway
    {
        protected override Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            return dic.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
        }
    }
}
