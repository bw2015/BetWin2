using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

using BW.Agent;
using BW.Common;
using BW.Common.Lottery;
using SP.Studio.Net;

using SP.Studio.Xml;
using SP.Studio.Core;
using SP.Studio.IO;
using SP.Studio.ErrorLog;
using SP.Studio.Model;
using System.Diagnostics;

namespace BetWinSpider
{
    class Program
    {
        private const string Gateway = "http://a8.to/LotteryResult.ashx?Type=";

        /// <summary>
        /// 多线程任务数量
        /// </summary>
        private static int TASK = 3;

        /// <summary>
        /// 最后一期的采集数据
        /// </summary>
        static Dictionary<LotteryType, string> dic = new Dictionary<LotteryType, string>();

        /// <summary>
        /// 系统彩种
        /// </summary>
        static Dictionary<LotteryType, string> siteLottery = new Dictionary<LotteryType, string>();

        /// <summary>
        /// 当前所有的站点
        /// </summary>
        static int[] SiteList
        {
            get
            {
                return SystemAgent.Instance().GetSiteList().Where(t => t.Status == BW.Common.Sites.Site.SiteStatus.Normal).Select(t => t.ID).ToArray();
            }
        }

        /// <summary>
        /// 保存错误日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        static void SaveErrorLog(Exception ex, string message)
        {
            string path = Application.StartupPath + @"\ErrorLog\" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            FileAgent.CreateDirectory(path, true);
            string errorId;
            int httpCode;
            string content = string.Format("[{0}] {1}\n\r{2}", DateTime.Now, message, ErrorAgent.CreateDetail(ex, out errorId, out httpCode));
            File.AppendAllText(path, content, Encoding.UTF8);
        }

        static void Main(string[] args)
        {
            Console.Title = "开奖器";
            Console.WriteLine("当前站点：{0}", string.Join(",", SiteList));

            foreach (object t in Enum.GetValues(typeof(LotteryType)))
            {
                LotteryType type = (LotteryType)t;
                if (type.GetCategory().SiteLottery)
                {
                    siteLottery.Add(type, null);
                }
                else
                {
                    dic.Add(type, null);
                }
            }

            string game = ConfigurationManager.AppSettings["game"];
            if (!string.IsNullOrEmpty(game))
            {
                siteLottery = siteLottery.Where(t => game.Split(',').Contains(t.Key.ToString())).ToDictionary(t => t.Key, t => t.Value);
            }

            TASK = args.Get("task", 3);

            Console.WriteLine("倒计时3秒开启...");
            System.Threading.Thread.Sleep(3000);

            OpenData();
        }

        /// <summary>
        /// 官方彩种采集
        /// </summary>
        /// <param name="type"></param>
        static void GetData(LotteryType type)
        {
            while (true)
            {
                string url = Gateway + type;
                string result;
                try
                {
                    result = NetAgent.DownloadData(url, Encoding.UTF8);
                    XElement root = XElement.Parse(result);
                    XElement itemLast = root.Elements().LastOrDefault();
                    if (itemLast == null || dic[type] == itemLast.GetAttributeValue("key")) continue;

                    LotteryCategoryAttribute category = type.GetCategory();

                    foreach (XElement item in root.Elements())
                    {
                        string index = item.GetAttributeValue("key");
                        string number = category.GetResultNumber(item.GetAttributeValue("value"));

                        if (LotteryAgent.Instance().SaveResultNumber(type, index, number))
                        {
                            Console.WriteLine("{0} {1}:{2}", type.GetDescription(), index, number);
                            dic[type] = index;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0}:{1}", type, ex.Message);
                    SystemAgent.Instance().AddErrorLog(0, ex, string.Format("[官方彩开奖错误] Type:{0}", type));
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 系统彩开奖
        /// </summary>
        static void OpenData()
        {
            // 多线程任务
            Dictionary<int, List<OpenTask>> task = new Dictionary<int, List<OpenTask>>();
            int index = 0;
            foreach (LotteryType t in siteLottery.Select(t => t.Key))
            {
                foreach (int siteId in SiteList)
                {
                    int taskID = index % TASK;
                    if (!task.ContainsKey(taskID)) task.Add(taskID, new List<OpenTask>());
                    task[taskID].Add(new OpenTask(t, siteId));
                    index++;
                }
            }

            System.Threading.Tasks.Parallel.ForEach(task, item =>
            {
                while (true)
                {
                    foreach (OpenTask t in item.Value)
                    {
                        OpenData(t.Type, t.SiteID);
                    }
                    System.Threading.Thread.Sleep(1500);
                }
            });
        }

        /// <summary>
        /// 系统彩种开奖
        /// </summary>
        /// <param name="type"></param>
        static void OpenData(LotteryType type)
        {
            foreach (int siteId in SiteList)
            {
                OpenData(type, siteId);
            }
            System.Threading.Thread.Sleep(1500);
        }

        /// <summary>
        /// 系统彩种开奖
        /// </summary>
        /// <param name="type"></param>
        /// <param name="siteId"></param>
        static void OpenData(LotteryType type, int siteId)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            LotteryAgent.Instance().MessageClean();
            try
            {
                string index;
                if (!LotteryAgent.Instance().IsNeedOpen(siteId, type, out index))
                {
                    return;
                }
                string number;
                if (LotteryAgent.Instance().OpenResultNumber(siteId, type, index, out number))
                {
                    Console.WriteLine("{0} {1}:{2}：{3} 开奖成功 耗时:{4}ms", type.GetDescription(), siteId, index, number, sw.ElapsedMilliseconds);
                }
                else
                {
                    Console.WriteLine("{0} {1}:{2} 开奖失败 耗时:{3}ms", type.GetDescription(), siteId, index, sw.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}-{1}:{2}", siteId, type, ex.Message);
                SystemAgent.Instance().AddErrorLog(siteId, ex, string.Format("[系统彩开奖错误] Type:{0}", type));
            }
        }
    }
}
