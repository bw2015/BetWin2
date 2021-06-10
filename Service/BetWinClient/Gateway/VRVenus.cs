﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using SP.Studio.Net;
using SP.Studio.Web;

namespace BetWinClient.Gateway
{
    /// <summary>
    /// VR金星1.5分
    /// </summary>
    public partial class VRVenus : IGateway
    {
        private static DateTime _lastTime = DateTime.MinValue;

        /// <summary>
        /// 从官网获取
        /// </summary>
        /// <returns></returns>
        [Description("VR竞速彩票")]
        private Dictionary<string, string> getResultByVideoracing()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (_lastTime > DateTime.Now.AddSeconds(WebAgent.GetRandom(2, 6) * -1)) return dic;
            //20170601340=4,3,4,2,2;
            string url = "http://videoracing.com/analy_3_1.aspx";
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            Regex regex = new Regex(@"(?<Date>\d{8})(?<Index>\d{3})=(?<Number>\d,\d,\d,\d,\d)");

            foreach (Match match in regex.Matches(result))
            {
                string index = match.Groups["Date"].Value + "-" + match.Groups["Index"].Value;
                string number = match.Groups["Number"].Value;
                if (dic.ContainsKey(index)) continue;
                dic.Add(index, number);
            }
            _lastTime = DateTime.Now;
            return dic;
        }
    }
}