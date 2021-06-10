using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Xml.Linq;

using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Text;
using SP.Studio.Xml;


namespace BetWinClient.Gateway
{
    /// <summary>
    /// 吉林福彩
    /// </summary>
    public partial class JLK3 : IGateway
    {
        /// <summary>
        /// 从吉林福彩官方获取
        /// </summary>
        /// <returns></returns>
        [Description("500彩票网")]
        private Dictionary<string, string> getResultByjlfc()
        {
            return Utils.Get500("jlk3", t => Regex.Replace(t, @"(?<Date>\d{6})(?<Index>\d{3})", "20${Date}-${Index}"), t => t);
        }

    }
}
