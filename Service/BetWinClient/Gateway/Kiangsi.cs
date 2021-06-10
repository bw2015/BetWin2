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
using SP.Studio.Net;

namespace BetWinClient.Gateway
{
    public partial class Kiangsi : IGateway
    {
     
        /// <summary>
        /// 从乐彩的移动端获取（只能获取最新的15期）
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("m.lecai.com")]
        private Dictionary<string, string> getResultByLecaiMobile()
        {
            string url = "http://m.lecai.com/lottery/draw/history.php?lottery_type=202&num=15";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            Regex p = new Regex(@"<p>(?<Date>\d{8})(?<Index>\d{3})期</p><p><span class=""draw-red"">(?<Number>\d,\d,\d,\d,\d)</span></p>");
            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (Match match in p.Matches(result))
            {
                list.Add(string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value), match.Groups["Number"].Value.Replace(",", ""));
            }
            return list;
        }

        /// <summary>
        /// 从网易彩票的移动端获取（只能获取最近的30期）
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("网易彩票移动端")]
        private Dictionary<string, string> getResultBy163Mobile()
        {
            string url = "http://caipiao.163.com/t/awardlist.html?gameEn=jxssc";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            //"number":"9 8 6 8 8","period":"20160115037"
            Regex regex = new Regex(@"""number"":""(?<Value>\d \d \d \d \d)"",""period"":""(?<Date>\d{8})(?<Index>\d{3})""");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                dic.Add(string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value), match.Groups["Value"].Value.Replace(" ", ""));
            }
            return dic;
        }
    }
}
