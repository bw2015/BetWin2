using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 天津时时彩
    /// </summary>
    public partial class Tianjin : IGateway
    {
        protected override Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            Regex opencai = new Regex(@"^(?<Date>\d{8})(?<Index>\d{3})$");
            Regex mcai = new Regex(@"^(?<Date>\d{8})(?<Index>\d{2})$");
            return dic.ToDictionary(t =>
            {
                if (opencai.IsMatch(t.Key)) return opencai.Replace(t.Key, "${Date}-${Index}");
                if (mcai.IsMatch(t.Key)) return mcai.Replace(t.Key, "${Date}-0${Index}");
                return t.Key;
            }, t => t.Value);
        }
    }
}
