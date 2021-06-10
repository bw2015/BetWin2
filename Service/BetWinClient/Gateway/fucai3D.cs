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
using SP.Studio.Text;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// 福彩3D
    /// </summary>
    public partial class fucai3D : IGateway
    {
        /// <summary>
        /// 从福彩中心采集
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [Description("www.cwl.gov.cn")]
        private Dictionary<string, string> getResultByCWL()
        {
            string url = "http://www.cwl.gov.cn/cwl_admin/kjxx/findDrawNotice?name=3d&issueCount=30";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string result = string.Empty;
            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.Referer, "http://www.cwl.gov.cn/kjxx/fc3d/kjgg/");
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36");
                result = NetAgent.DownloadData(url, Encoding.UTF8);
            }
            Regex regex = new Regex(@"""code"":""(?<Index>\d{7})"".+?""red"":""(?<Number>\d,\d,\d)""");
            foreach (Match match in regex.Matches(result))
            {
                string index = match.Groups["Index"].Value;
                string number = match.Groups["Number"].Value;
                dic.Add(index, number);
            }
            return dic;
        }


    }
}
