using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Xml.Linq;
using System.Configuration;

using SP.Studio.Xml;
using SP.Studio.Net;
using System.Windows.Forms;
using System.IO;
using SP.Studio.Text;

namespace BetWinClient
{
    /// <summary>
    /// 采集工具类
    /// </summary>
    public static partial class Utils
    {
        /// <summary>
        /// 本地测试用的路径
        /// </summary>
        public static string TESTURL { private get; set; }


        private static Config _config;
        /// <summary>
        /// 配置文件
        /// </summary>
        internal static Config Configuration
        {
            get
            {
                if (_config == null)
                {
                    _config = new Config();
                }
                return _config;
            }
        }

        /// <summary>
        /// 是否属于要采集的游戏
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static bool IsGame(string game)
        {
            return Configuration.IsGame(game);
        }

        static Utils()
        {

        }

        private static string _debug;
        /// <summary>
        /// 当前是否处于调试模式
        /// </summary>
        public static bool debug
        {
            get
            {
                if (string.IsNullOrEmpty(_debug))
                {
                    _debug = ConfigurationManager.AppSettings["debug"] ?? "false";
                }
                return _debug == "true";
            }
        }


        /// <summary>
        /// 采集方法是否正在执行中
        /// </summary>
        public static Dictionary<string, bool> Run = new Dictionary<string, bool>();

        /// <summary>
        /// 从配置地址中获取彩票返回结果
        /// </summary>
        /// <param name="api">API类型</param>
        /// <param name="type">彩种类型</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetAPI(API api, Type type)
        {
            return Configuration.GetContent(api, type.Name.ToString());
        }

        /// <summary>
        /// 是否是付费接口
        /// </summary>
        /// <param name="api"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAPI(API api, Type type)
        {
            string key = string.Format("{0}.{1}", api, type);
            return Configuration.IsGame(key);
        }

        /// <summary>
        /// 20个开奖号码转化为5个号码（重庆时时彩规则）
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string GetOpenCode(string code)
        {
            int[] number = code.Split(',', '+').Select(t => int.Parse(t)).Take(20).OrderBy(t => t).ToArray();
            if (number.Length != 20) return code;
            int[] result = new int[5];

            for (int i = 0; i < result.Length; i++)
            {
                int num = 0;
                int start = number.Length / result.Length * i;
                for (int n = start; n < start + number.Length / result.Length; n++)
                {
                    num += number[n];
                }
                result[i] = num % 10;
            }
            return string.Join(",", result);
        }

        public static void WriteLine(object format, params object[] args)
        {
            if (debug)
            {
                if (args.Length == 0)
                {
                    Console.WriteLine(format);
                }
                else
                {
                    Console.WriteLine(format.ToString(), args);
                }
            }
        }

        /// <summary>
        /// 保存错误日志到文本文件
        /// </summary>
        /// <param name="content"></param>
        /// <param name="args"></param>
        public static void SaveErrorLog(string content, params object[] args)
        {
            lock (typeof(Utils))
            {
                if (args.Length != 0)
                    content = string.Format(content, args);
                string logFile = Application.StartupPath + @"\Log\";
                if (!Directory.Exists(logFile)) Directory.CreateDirectory(logFile);
                logFile += DateTime.Now.ToString("yyyyMMdd") + "-err.log";
                //string content = string.Format("{0}\n{1} : {2}\n\r", DateTime.Now, this.GetType().Name, result);
                File.AppendAllText(logFile, DateTime.Now + "\n\r" + content + "\n\r\n\r", Encoding.UTF8);

                if (Utils.debug) Console.WriteLine(content);
            }
        }

        /// <summary>
        /// VR系列获取第一条记录
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetVRTop1(string url, int numberCount = 10)
        {
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            result = StringAgent.GetString(result, "<div class=\"font_tittle_about\">", "</div>");
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(result)) return dic;

            Regex regex = new Regex(@"(?<Date>\d{8})(?<Index>\d{3})");
            Regex number = new Regex(@"<span class=""(orange_1|blue_1)"">(?<Number>\d{2})</span>");
            if (!regex.IsMatch(result) || number.Matches(result).Count != numberCount) return dic;

            string index = string.Format("{0}-{1}", regex.Match(result).Groups["Date"].Value, regex.Match(result).Groups["Index"].Value);
            List<string> num = new List<string>();
            foreach (Match match in number.Matches(result))
            {
                num.Add(match.Groups["Number"].Value);
            }
            dic.Add(index, string.Join(",", num));
            return dic;
        }


        /// <summary>
        /// 从500获取
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Get500(string name, Func<string, string> index, Func<string, string> number)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (DateTime.Now.Hour < 8) return dic;
            string url = string.Format("http://kaijiang.500.com/static/info/kaijiang/xml/{0}/{1}.xml?_A=TCMRFBIH{2}", name, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.Ticks);

            try
            {
                XElement result = XElement.Load(url);
                dic = result.Elements().ToDictionary(t => index(t.GetAttributeValue("expect")), t => number(t.GetAttributeValue("opencode")));
            }
            catch
            {
                dic = new Dictionary<string, string>();
            }
            return dic;
        }

        /// <summary>
        /// 获取VR的列表
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetVRList(string url, int numberCount = 10)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string result = NetAgent.DownloadData(url, Encoding.UTF8);

            List<string> regex = new List<string>();
            regex.Add(@"<div class=[""']css_tr[""']>");
            regex.Add(@"<div class=[""']css_td[""']>(?<Date>\d{4}\/\d{2}\/\d{2})</div>");
            regex.Add(@"<div class=[""']css_td3[""']>(?<Index>\d{3})</div>");
            for (int i = 0; i < numberCount; i++)
            {
                regex.Add(string.Format(@"<div class=[""']css_td2 redbb[""']>(?<Num{0}>\d+)</div>", i));
            }
            regex.Add("</div>");

            Regex reg = new Regex(string.Join(@"[\s\S]+?", regex));
            foreach (Match match in reg.Matches(result))
            {
                string index = string.Format("{0}-{1}", match.Groups["Date"].Value.Replace("/", ""), match.Groups["Index"].Value);
                List<string> number = new List<string>();
                for (int i = 0; i < numberCount; i++)
                {
                    number.Add(match.Groups["Num" + i].Value);
                }
                if (!dic.ContainsKey(index))
                    dic.Add(index, string.Join(",", number));
            }
            return dic;
        }

        
    }

    /// <summary>
    /// API接口
    /// </summary>
    public enum API
    {
        /// <summary>
        /// 开彩网
        /// </summary>
        opencai,
        /// <summary>
        /// 彩票控
        /// </summary>
        cpk,
        /// <summary>
        /// M彩
        /// </summary>
        mcai
    }
}
