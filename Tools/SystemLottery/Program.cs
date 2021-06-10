using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;

namespace SystemLottery
{
    class Program
    {
        /// <summary>
        /// 采集地址
        /// </summary>
        private static string gateway;

        static void Main(string[] args)
        {
            gateway = ConfigurationManager.AppSettings["gateway"];
            /// 密钥
            string host = ConfigurationManager.AppSettings["host"];
            // 彩种
            string type = ConfigurationManager.AppSettings["type"];

            if (string.IsNullOrEmpty(gateway))
            {
                Console.WriteLine("未配置采集地址 gateway");
                return;
            }
            if (string.IsNullOrEmpty(type))
            {
                Console.WriteLine("未配置彩种 type");
                return;
            }
            if (string.IsNullOrEmpty(host))
            {
                Console.WriteLine("未配置密钥 host");
                return;
            }
            while (true)
            {
                try
                {
                    Dictionary<string, string> dic = null;
                    switch (type)
                    {
                        case "Second45":
                            dic = GetSecond45();
                            break;
                        case "Tokyo15":
                            dic = GetTokyo15();
                            break;
                        default:
                            Console.WriteLine("type:{0} 无效", type);
                            break;
                    }
                    if (dic == null || dic.Count == 0) continue;

                    Console.WriteLine("[{0}] {1}    {2}", DateTime.Now, Utils.Upload(type, host, dic),
                        string.Join("&", dic.OrderByDescending(t => t.Key).Take(1).Select(t => t.Key + "=" + t.Value)));

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    System.Threading.Thread.Sleep(3000);
                }
            }

        }

        /// <summary>
        /// 幸运45秒
        /// </summary>
        /// <returns></returns>
        static Dictionary<string, string> GetSecond45()
        {
            //201705081584asdhp=76161
            //201708230544----3,3,7,0,4
            //201709251696----5,7,7,9,7
            Regex regex = new Regex(@"(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})(?<Index>\d+)(----|asdhp=)(?<Number>\d{5}|\d,\d,\d,\d,\d)");

            using (WebClient wc = new WebClient())
            {
                string url = gateway.Replace("${RANDOM}", Guid.NewGuid().ToString("N"));
                Console.WriteLine(url);

                string result = Encoding.UTF8.GetString(wc.DownloadData(url));
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (Match match in regex.Matches(result))
                {
                    DateTime date = DateTime.Parse(string.Concat(match.Groups["Year"].Value, "-", match.Groups["Month"].Value, "-", match.Groups["Day"].Value));
                    int dateIndex = int.Parse(match.Groups["Index"].Value);
                    date = date.AddSeconds(dateIndex * 45);
                    date = date.AddHours(-4);
                    string index = (((int)date.TimeOfDay.TotalSeconds / 45)).ToString();
                    if (index == "0")
                    {
                        date = date.AddDays(-1);
                        index = "1920";
                    }
                    index = date.ToString("yyyyMMdd") + "-" + index.PadLeft(4, '0');
                    string number = match.Groups["Number"].Value;
                    if (Regex.IsMatch(@"\d{5}", number))
                    {
                        number = string.Join(",", match.Groups["Number"].Value.Select(t => t));
                    }
                    if (!dic.ContainsKey(index)) dic.Add(index, number);
                }
                return dic.OrderByDescending(t => t.Key).Take(10).ToDictionary(t => t.Key, t => t.Value);
            }
        }

        /// <summary>
        /// 东京1.5分
        /// </summary>
        /// <returns></returns>
        static Dictionary<string, string> GetTokyo15()
        {
            //20170707539asdhp=77600
            Regex regex = new Regex(@"(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})(?<Index>\d+)(----|asdhp=)(?<Number>\d{5}|\d,\d,\d,\d,\d)");

            using (WebClient wc = new WebClient())
            {
                string result = Encoding.UTF8.GetString(wc.DownloadData(gateway));
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (Match match in regex.Matches(result))
                {
                    DateTime date = DateTime.Parse(string.Concat(match.Groups["Year"].Value, "-", match.Groups["Month"].Value, "-", match.Groups["Day"].Value));
                    string dateIndex = match.Groups["Index"].Value;
                    string index = date.ToString("yyyyMMdd") + "-" + dateIndex.PadLeft(3, '0');
                    string number = match.Groups["Number"].Value;
                    if (Regex.IsMatch(@"\d{5}", number))
                    {
                        number = string.Join(",", match.Groups["Number"].Value.Select(t => t));
                    }
                    if (!dic.ContainsKey(index)) dic.Add(index, number);
                }
                return dic.OrderByDescending(t => t.Key).Take(10).ToDictionary(t => t.Key, t => t.Value);
            }
        }
    }
}
