using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Net;
using SP.Studio.Text;
using System.ComponentModel;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 江苏快三
    /// </summary>
    public partial class JSK3 : IGateway
    {
        [Description("500彩票网")]
        private Dictionary<string, string> getResultByjlfc()
        {
            return Utils.Get500("jsk3", t => Regex.Replace(t, @"(?<Date>\d{6})(?<Index>\d{2})", "20${Date}-0${Index}"), t => t);
        }
    }
}
