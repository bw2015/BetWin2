using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;

using SP.Studio.Text;
using SP.Studio.Net;
using SP.Studio.Web;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 上海快三
    /// </summary>
    public partial class SHK3 : IGateway
    {
        /// <summary>
        /// 上海福彩网官方
        /// </summary>
        /// <returns></returns>
        [Description("上海福彩网")]
        private Dictionary<string, string> getResultByswlc()
        {
            string url = "http://fucai.eastday.com/LotteryNew/K3Result.aspx";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            string[] rows = StringAgent.GetStringValue(result, "<td class=\"first\">", "</tr>");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Regex regexIndex = new Regex(@"\d{8}-\d{2}");
            Regex regexNumber = new Regex(@"<span>(?<Number>[1-6])</span>");
            foreach (string row in rows)
            {
                if (!regexIndex.IsMatch(row) || !regexNumber.IsMatch(row) || regexNumber.Matches(row).Count != 3) continue;
                MatchCollection number = regexNumber.Matches(row);
                string index = regexIndex.Match(row).Value;

                dic.Add(index, string.Format("{0},{1},{2}", number[0].Groups["Number"].Value,
                    number[1].Groups["Number"].Value,
                    number[2].Groups["Number"].Value));
            }
            return dic;
        }
    }
}
