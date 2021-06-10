using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SP.Studio.Net;

using System.ComponentModel;
using SP.Studio.Web;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 湖北快三
    /// </summary>
    public partial class HBK3 : IGateway
    {
        /// <summary>
        /// 从网易彩票获取
        /// </summary>
        /// <returns></returns>
        [Description("网易彩票")]
        private Dictionary<string, string> getResultBy163()
        {
            string url = "http://caipiao.163.com/t/awardlist.html?gameEn=hbkuai3";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            //"number":"1 4 5","period":"160509025"

            Regex regex = new Regex(@"""number"":""(?<N1>\d) (?<N2>\d) (?<N3>\d)"",""period"":""(?<Date>\d{6})(?<Index>\d{3})""");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                string index = string.Format("20{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
                string number = string.Format("{0}{1}{2}", match.Groups["N1"].Value, match.Groups["N2"].Value, match.Groups["N3"].Value);

                dic.Add(index, number);
            }
            return dic;
        }
        
    }
}
