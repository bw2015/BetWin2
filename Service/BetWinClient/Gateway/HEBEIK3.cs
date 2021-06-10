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
    /// 河北快三
    /// </summary>
    public partial class HEBEIK3 : IGateway
    {
        /// <summary>
        /// 从河北福彩网官网获取
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getResultByyzfcw()
        {
            //
            string url = "http://www.yzfcw.com/game/k3SelectDate";
            string date = DateTime.Now.ToString("yyyyMMdd");
            string result = NetAgent.UploadData(url, string.Format("selectDate={0}", date), Encoding.UTF8);

            //"lotteryIssue":"63","lotteryNumber":"3,4,6"
            //time":1491314410000,"timezoneOffset":-480,"year":117},"gameId":9,"gameName":"快3","lotteryId":519639,"lotteryIssue":"81","lotteryNumber":"2,5,6"
            Regex regex = new Regex(@"""time"":(?<Date>\d+).+?""lotteryIssue"":""(?<Index>\d+)"",""lotteryNumber"":""(?<N1>[1-6]),(?<N2>[1-6]),(?<N3>[1-6])""");

            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (Match match in regex.Matches(result))
            {
                long time = long.Parse(match.Groups["Date"].Value);
                DateTime datetime = new DateTime(1970, 1, 1, 8, 0, 0).AddMilliseconds(time);

                string index = string.Format("{0}-{1}", datetime.ToString("yyyyMMdd"), match.Groups["Index"].Value.PadLeft(3, '0'));
                string number = string.Concat(match.Groups["N1"].Value, match.Groups["N2"].Value, match.Groups["N3"].Value);

                dic.Add(index, number);
            }
            return dic;
        }
    }
}
