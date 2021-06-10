using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Net;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 东京1.5分
    /// </summary>
    public partial class Japan15 : IGateway
    {
        protected override Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            Regex opencai = new Regex(@"^(?<Date>\d{8})(?<Index>\d{3})$");
            Regex cpk = new Regex(@"^(?<Date>\d{8})(?<Index>\d{3})$");

            return dic.ToDictionary(t =>
            {
                if (opencai.IsMatch(t.Key)) return opencai.Replace(t.Key, "${Date}-${Index}");
                if (cpk.IsMatch(t.Key)) return cpk.Replace(t.Key, "${Date}-${Index}");
                return t.Value;

            }, t => Utils.GetOpenCode(t.Value));
        }
    }
}
