using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Net;
using SP.Studio.Web;
using SP.Studio.Text;


namespace BetWinClient.Gateway
{
    /// <summary>
    /// VR快艇
    /// </summary>
    public partial class VRBoat : IGateway
    {

        private static DateTime _lastTop1Time = DateTime.MinValue;
        [Description("最新一期")]
        private Dictionary<string, string> getResultByTop1()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (_lastTop1Time > DateTime.Now.AddSeconds(WebAgent.GetRandom(2, 6) * -1)) return dic;

            string url = "http://videoracing.com/open_5_1.aspx";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            result = StringAgent.GetString(result, "<div class=\"font_tittle_about\">", "</div>");
            if (string.IsNullOrEmpty(result))
            {
                return dic;
            }
            Regex regexIndex = new Regex(@"(?<Date>201\d{5})(?<Index>\d{3})");
            Regex regexNumber = new Regex(@"\<span class=""(orange|blue)""\>(?<Number>01|02|03|04|05|06|07|08|09|10)</span>");

            if (regexIndex.IsMatch(result) && regexNumber.Matches(result).Count == 10)
            {
                string index = string.Format("{0}-{1}", regexIndex.Match(result).Groups["Date"].Value, regexIndex.Match(result).Groups["Index"].Value);
                List<string> number = new List<string>();
                foreach (Match num in regexNumber.Matches(result))
                {
                    number.Add(num.Groups["Number"].Value);
                }
                dic.Add(index, string.Join(",", number));
            }
            _lastTop1Time = DateTime.Now;
            return dic;
        }


        private static DateTime _lastDateTime = DateTime.MinValue;
        /// <summary>
        /// 获取当日所有
        /// </summary>
        /// <returns></returns>
        [Description("当日所有")]
        private Dictionary<string, string> getResultByDate()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (_lastDateTime > DateTime.Now.AddSeconds(WebAgent.GetRandom(120, 300) * -1)) return dic;

            string url = "http://videoracing.com/open_5_2.aspx";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            string[] resultList = StringAgent.GetStringValue(result, "<div class=\"css_tr\">", "                    </div>");

            Regex date = new Regex(@"201[789]/[01]\d/[0123]\d");
            Regex index = new Regex(@"<div class=""css_td3"">(?<Index>\d+)</div>");
            Regex number = new Regex(@"<div class=""css_td2 redbb"">(?<Number>01|02|03|04|05|06|07|08|09|10)</div>");
            foreach (string item in resultList)
            {
                if (!date.IsMatch(item) || !index.IsMatch(item) || number.Matches(item).Count != 10) continue;
                string itemIndex = string.Format("{0}-{1}", date.Match(item).Value.Replace("/", ""), index.Match(item).Groups["Index"].Value);
                List<string> num = new List<string>();
                foreach (Match match in number.Matches(item))
                {
                    num.Add(match.Groups["Number"].Value);
                }

                dic.Add(itemIndex, string.Join(",", num));
            }
            _lastDateTime = DateTime.Now;
            return dic;
        }
    }
}
