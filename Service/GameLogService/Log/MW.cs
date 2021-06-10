using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;

using SP.Studio.IO;
using SP.Studio.Core;
using SP.Studio.Xml;
using BW.Agent;
using BW.Common.Games;
using BW.Common;
using BW.Common.Users;
using BW.Common.Systems;
using SP.Studio.Web;
using SP.Studio.Net;
using SP.Studio.Json;

namespace GameLogService.Log
{
    public class MW : IDisposable
    {
        private GameInterface game;

        /// <summary>
        /// 本地导入的数量
        /// </summary>
        private int count = 0;

        protected virtual BW.GateWay.Games.MW Setting
        {
            get
            {
                return (BW.GateWay.Games.MW)this.game.Setting;
            }
        }

        private Stopwatch sw;

        public MW(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();
            this.game = game;
        }

        private string POST(string method, Dictionary<string, string> data)
        {
            string url = Setting.APIDomain + "/" + this.GetType().Name + "/" + method;
            using (WebClient wc = new WebClient())
            {
                string auth = WebAgent.StringToBase64(string.Format("{0}:{1}", Setting.UserName, Setting.PassWord));
                wc.Headers.Add(HttpRequestHeader.Authorization, "Basic " + auth);
                return NetAgent.UploadData(url, string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value))), Encoding.UTF8, wc);
            }
        }

        /// <summary>
        /// 上一次读取的时间
        /// </summary>
        private static DateTime lastTime = DateTime.Now.AddDays(-1);

        /// <summary>
        /// 导入数据
        /// </summary>
        public void Import()
        {
            if (lastTime > DateTime.Now.AddMinutes(-15)) lastTime = DateTime.Now.AddMinutes(-15);
            string fromDate = lastTime.ToString("yyyy-MM-ddTHH:mm:ss");
            string toDate = lastTime.AddMinutes(15).ToString("yyyy-MM-ddTHH:mm:ss");

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("fromDate", fromDate);
            dic.Add("toDate", toDate);

            string result = this.POST("game/history", dic);
            if (Program.debug) Console.WriteLine("{0}-{1} {2}", fromDate, toDate, result);

            Hashtable[] list = JsonAgent.GetJList(result);
            List<SlotLog> logList = new List<SlotLog>();
            foreach (Hashtable ht in list)
            {
                logList.Add(new SlotLog(GameType.MW, ht));
            }

            logList.ForEach(slot =>
            {
                if (GameAgent.Instance().ImportLog(slot))
                {
                    count++;
                }
                else
                {
                    if (Program.debug)
                    {
                        Console.WriteLine("导入日志失败,{0}", slot.GameName);
                    }
                }
            });
            lastTime = lastTime.AddMinutes(14);
        }

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.game.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }
    }
}
