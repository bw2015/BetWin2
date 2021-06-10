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
    /// 新疆时时彩
    /// </summary>
    public partial class Sinkiang : IGateway
    {
        /// <summary>
        /// 从官网获取
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("http://www.xjflcp.com/game/sscAnnounce")]
        private Dictionary<string, string> getResultByxjflcp()
        {
            string url = "http://www.xjflcp.com/game/SelectDate";
            string date = DateTime.Now.AddHours(-3).ToString("yyyyMMdd");
            string result = NetAgent.UploadData(url, string.Format("selectDate={0}", date), Encoding.GetEncoding("GBK"));

            //"lotteryIssue":"2016031396","lotteryNumber":"5,0,8,9,7"
            Regex regex = new Regex(@"""lotteryIssue"":""(?<Date>\d{8})(?<Index>\d{2})"",""lotteryNumber"":""(?<N1>\d),(?<N2>\d),(?<N3>\d),(?<N4>\d),(?<N5>\d)""");
            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                string index = string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
                string number = string.Format("{0},{1},{2},{3},{4}", match.Groups["N1"].Value, match.Groups["N2"].Value, match.Groups["N3"].Value, match.Groups["N4"].Value, match.Groups["N5"].Value);
                list.Add(index, number);
            }
            return list;
        }

        [Description("新疆福利彩票网")]
        private Dictionary<string, string> getResultByxjflcpToday()
        {
            string url = "http://www.xjflcp.com/game/sscIndex";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            //"lotteryIssue":"2016031396","lotteryNumber":"5,0,8,9,7"
            Regex regex = new Regex(@"<td class=""bold"">(?<Date>\d{8})(?<Index>\d{2})</td>[^\<]+?<td class=""kj_codes"">(?<N1>\d),(?<N2>\d),(?<N3>\d),(?<N4>\d),(?<N5>\d)</td>");
            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                string index = string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
                string number = string.Format("{0},{1},{2},{3},{4}", match.Groups["N1"].Value, match.Groups["N2"].Value, match.Groups["N3"].Value, match.Groups["N4"].Value, match.Groups["N5"].Value);
                list.Add(index, number);
            }
            return list;
        }

        /// <summary>
        /// 从开彩网获取
        /// </summary>
        /// <returns></returns>
        [Description("开彩网")]
        private Dictionary<string, string> getResultByAPIPlus()
        {
            Dictionary<string, string> dic = Utils.GetAPI(API.opencai, this.GetType());

            return dic.ToDictionary(t => Regex.Replace(t.Key, @"^(?<Date>\d{8})(?<Index>\d{3})$", match =>
            {
                return string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value.Substring(1));
            }), t => t.Value);
        }

        /// <summary>
        /// 从开彩网获取
        /// </summary>
        /// <returns></returns>
        [Description("彩票控")]
        private Dictionary<string, string> getResultByAPICPK()
        {
            Dictionary<string, string> dic = Utils.GetAPI(API.cpk, this.GetType());

            return dic.ToDictionary(t => Regex.Replace(t.Key, @"^(?<Date>\d{8})(?<Index>\d{2})$", match =>
            {
                return string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
            }), t => t.Value);
        }
    }
}
