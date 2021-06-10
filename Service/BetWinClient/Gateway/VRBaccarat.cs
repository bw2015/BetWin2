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
    /// VR百家樂
    /// </summary>
    public partial class VRBaccarat : IGateway
    {
        /// <summary>
        /// 從官網獲取
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultByVR()
        {
            string url = "http://vr.a8.to/analy_6_1.aspx";

            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            Regex regex = new Regex(@"(?<Date>\d{8})(?<Index>\d{3})=0(?<N1>\d),0(?<N2>\d),0(?<N3>\d),0(?<N4>\d)");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                GroupCollection group = match.Groups;
                string index = string.Format("{0}-{1}", group["Date"].Value, group["Index"].Value);
                string number = string.Format("{0},{1},{2},{3}", group["N1"].Value, group["N2"].Value, group["N3"].Value, group["N4"].Value);

                dic.Add(index, number);
            }
            return dic;
        }
    }
}
