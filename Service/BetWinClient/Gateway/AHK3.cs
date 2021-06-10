using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using SP.Studio.Net;


namespace BetWinClient.Gateway
{
    /// <summary>
    /// 安徽快三
    /// </summary>
    public partial class AHK3 : IGateway
    {
        /// <summary>
        /// 从安徽福彩官网获取
        /// </summary>
        /// <returns></returns>
        [Description("安徽福彩网")]
        private Dictionary<string, string> getResultByahfc()
        {
            string url = "http://data.ahfc.gov.cn/k3/index.html";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            //        <TD class=line_r>160509001</TD>
            //<TD class=ball01>2</TD>
            //<TD class=ball01>4</TD>
            //<TD class="ball01 line_r">6</TD>

            Regex regex = new Regex(@"<TD class=line_r>(?<Date>\d{6})(?<Index>\d{3})</TD>[\s\S]+?<TD class=ball01>(?<N1>\d)</TD>[\s\S]+?<TD class=ball01>(?<N2>\d)</TD>[\s\S]+?<TD class=""ball01 line_r"">(?<N3>\d)</TD>");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                string index = string.Format("20{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
                string number = string.Format("{0},{1},{2}", match.Groups["N1"].Value, match.Groups["N2"].Value, match.Groups["N3"].Value);

                dic.Add(index, number);
            }
            return dic;
        }
    }
}
