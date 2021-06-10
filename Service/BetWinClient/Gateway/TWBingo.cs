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
    public partial class TWBingo : IGateway
    {
        protected override Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            return dic.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
        }

        [Description("开彩网API")]
        private Dictionary<string, string> getResultByAPIUS()
        {
            Dictionary<string, string> list = Utils.GetAPI(API.opencai, this.GetType());
            return list.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
        }

        [Description("彩票控")]
        private Dictionary<string, string> getResultByCPK()
        {
            Dictionary<string, string> list = Utils.GetAPI(API.cpk, this.GetType());
            return list.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
        }

        [Description("多彩网")]
        private Dictionary<string,string> getResultByMCai()
        {
            Dictionary<string, string> list = Utils.GetAPI(API.mcai, this.GetType());
            return list.ToDictionary(t => t.Key, t => Utils.GetOpenCode(t.Value));
        }
    }
}
