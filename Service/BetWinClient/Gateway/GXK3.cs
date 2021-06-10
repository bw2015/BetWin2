using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using SP.Studio.Net;
namespace BetWinClient.Gateway
{
    public partial class GXK3 : IGateway
    {
        /// <summary>
        /// 从广西福彩网官网获取
        /// </summary>
        /// <returns></returns>
        [Description("广西福彩网")]
        private Dictionary<string, string> getResultBygxfcw()
        {
            string url = "http://www.gxfcw.com/wdwj/k3zst.html";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            //<td>20160509029</td><td>3</td><td>3</td><td>4</td>
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Regex regex = new Regex(@"<td align=""center"" >(?<Date>\d{8})(?<Index>\d{3})</td>\r\n<td align=""center"" >(?<N1>[1-6])&nbsp(?<N2>[1-6])&nbsp(?<N3>[1-6])</td>");
            foreach (Match match in regex.Matches(result))
            {
                string index = string.Format("{0}-{1}", match.Groups["Date"].Value, match.Groups["Index"].Value);
                string number = string.Format("{0},{1},{2}", match.Groups["N1"].Value, match.Groups["N2"].Value, match.Groups["N3"].Value);
                dic.Add(index, number);
            }
            return dic;
        }

        /// <summary>
        /// 彩票控
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultByCPK()
        {
            //170907035
            return Utils.GetAPI(API.cpk, this.GetType()).ToDictionary(t => Regex.Replace(t.Key, @"(?<Date>\d{6})(?<Index>\d{3})", "20${Date}-${Index}"), t => t.Value);
        }

        private Dictionary<string, string> getResultByAPIPlus()
        {
            return Utils.GetAPI(API.opencai, this.GetType()).ToDictionary(t => Regex.Replace(t.Key, @"(?<Date>\d{8})(?<Index>\d{3})", "${Date}-${Index}"), t => t.Value);
        }
    }
}
