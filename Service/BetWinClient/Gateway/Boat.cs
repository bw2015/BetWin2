using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SP.Studio.Net;
namespace BetWinClient.Gateway
{
    /// <summary>
    /// 幸运飞艇
    /// </summary>
    public partial class Boat : IGateway
    {
        private Dictionary<string, string> getResultByPlus()
        {
            Dictionary<string, string> dic = Utils.GetAPI(API.opencai, typeof(Boat));
            return dic.ToDictionary(t => Regex.Replace(t.Key, @"^(?<Date>\d{8})(?<Index>\d{3})$", "${Date}-${Index}"), t => t.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultBy969878cc()
        {
            string url = "http://m.fei111.com/api/getHistory.php?date=&" + Guid.NewGuid().ToString("N").Substring(0, 8).ToLower();
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            //"c_r":"6,4,1,10,5,9,8,7,3,2","c_t":"20170927099"
            //Regex regex = new Regex(@"""c_r"":""(?<Number>[^""]+)"",""c_t"":""(?<Date>\d{8})(?<Index>\d{3})""");
            //"preDrawIssue":20171021180,"preDrawCode":"07,09,05,08,02,10,06,03,01,04"
            Regex regex = new Regex(@"""preDrawIssue"":(?<Date>\d{8})(?<Index>\d{3}),""preDrawCode"":""(?<Number>[^""]+)""");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                string index = string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
                string number = string.Join(",", match.Groups["Number"].Value.Split(',').Select(t => t.PadLeft(2, '0')));
                dic.Add(index, number);
            }
            return dic;
        }
    }
}
