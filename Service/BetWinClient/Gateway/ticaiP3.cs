using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel;

using SP.Studio.Text;
using SP.Studio.Net;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 体彩P3
    /// </summary>
    public partial class ticaiP3 : IGateway
    {
        /// <summary>
        /// 从体彩官网获取（只能获取最新一期）
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("www.lottery.gov.cn")]
        private Dictionary<string, string> getResultBylotterygovcn()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string url = "http://www.lottery.gov.cn/api/lottery_index_kj.jspx";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            result = StringAgent.GetString(result, "\"pls\":{", "}");
            if (string.IsNullOrEmpty(result)) return dic;

            Regex indexRegex = new Regex(@"""term"":""(?<Index>\d+)""");
            Regex numberRegex = new Regex(@"""numberCode"":\[""(?<N1>\d)"",""(?<N2>\d)"",""(?<N3>\d)""\]");
            if (!indexRegex.IsMatch(result) || !numberRegex.IsMatch(result)) return dic;
            GroupCollection number = numberRegex.Match(result).Groups;
            dic.Add("20" + indexRegex.Match(result).Groups["Index"].Value, string.Format("{0},{1},{2}", number["N1"].Value, number["N2"].Value, number["N3"].Value));
            return dic;
        }

        /// <summary>
        /// 从网易彩票获取（只能获取最新5期）
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("caipiao.163.com")]
        private Dictionary<string, string> getResultBycaipiao163()
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            string url = "http://caipiao.163.com/order/pl3/";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            Regex regex1 = new Regex("<td><a href=\"/award/pl3/(?<Index>\\d{5}).html\"");
            Regex regex2 = new Regex("<b class=\"c_ba2636\">(?<Number>\\d \\d \\d)</b>");

            MatchCollection match1 = regex1.Matches(result);
            MatchCollection match2 = regex2.Matches(result);

            if (match1.Count == 0 || match2.Count == 0 || match1.Count != match2.Count) return list;

            for (int i = 0; i < match1.Count; i++)
            {
                string index = "20" + match1[i].Groups["Index"].Value;
                string number = match2[i].Groups["Number"].Value.Replace(" ", ",");
                list.Add(index, number);
            }

            return list;
        }

    }
}