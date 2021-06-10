using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using SP.Studio.Net;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 山西十一选五
    /// </summary>
    public partial class SX11x5 : IGateway
    {
        /// <summary>
        /// 从体彩官网获取
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("www.sxlottery.net")]
        private Dictionary<string, string> getResultBysxlottery()
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            string url = "http://www.sxlottery.net/11x5/";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            Regex regex = new Regex(@"<td>(?<Date>\d{6})(?<Index>\d{2})期</td>(?<Number>[\s\S]+?)</tr>");
            Regex numberRegex = new Regex(@"<span class=""kjhm"">(?<N>\d{2})</span>");
            if (!regex.IsMatch(result))
            {
                return list;
            }

            foreach (Match match in regex.Matches(result))
            {
                string index = string.Concat("20", match.Groups["Date"].Value, "-", match.Groups["Index"].Value);

                string number = match.Groups["Number"].Value;
                List<string> num = new List<string>();

                foreach (Match n in numberRegex.Matches(number))
                {
                    num.Add(n.Groups["N"].Value);
                }
                list.Add(index, string.Join(",", num));
            }
            return list;
        }
        
    }
}
