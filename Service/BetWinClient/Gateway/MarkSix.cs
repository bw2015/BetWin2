using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

using System.Net;
using SP.Studio.Text;
using SP.Studio.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 香港六合彩
    /// </summary>
    public partial class MarkSix : IGateway
    {

        /// <summary>
        /// 需翻墙才能访问
        /// </summary>
        /// <returns></returns>
        [Description("马会官方站")]
        private Dictionary<string, string> getResultByHKJC()
        {
            string url = "http://bet.hkjc.com/marksix/index.aspx?lang=ch";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            //攪珠期數 : 17/118
            Regex index = new Regex(@"攪珠期數 : (?<Year>\d{2})/(?<Index>\d{3})&nbsp;");
            Regex regex = new Regex(@"no_(?<Number>\d{2}).gif", RegexOptions.IgnoreCase);
            if (!index.IsMatch(result) || !regex.IsMatch(result)) return this.getHistory();
            Dictionary<string, string> dic = new Dictionary<string, string>();

            List<string> number = new List<string>();
            foreach (Match match in regex.Matches(result))
            {
                number.Add(match.Groups["Number"].Value);
            }
            if (number.Count != 7) return this.getHistory();
            Match indexMatch = index.Match(result);
            dic.Add(string.Format("20{0}{1}", indexMatch.Groups["Year"].Value, indexMatch.Groups["Index"].Value), string.Join(",", number));
            return dic;
        }

        /// <summary>
        /// 从马会官网站获取历史记录
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getHistory()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string url = "http://bet.hkjc.com/contentserver/jcbw/cmc/last30draw.json";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            Console.WriteLine("Result:{0}", result);
            if (result.Substring(0, 1) != "[") result = result.Substring(1);
            if (!result.StartsWith("[")) return dic;
            JArray list = JArray.Parse(result);
            foreach (JObject item in list)
            {
                string id = item.Value<string>("id");
                string no = item.Value<string>("no");
                string sno = item.Value<string>("sno");
                no = no + "+" + sno;

                string index = "20" + id.Replace("/", "");
                string number = string.Join(",", no.Split('+').Select(t => t.PadLeft(2, '0')));
                dic.Add(index, number);
            }
            return dic;
        }

        protected override Dictionary<string, string> GetResult(Dictionary<string, string> dic)
        {
            return dic.ToDictionary(t => t.Key, t => t.Value.Replace('+', ','));
        }
    }
}
