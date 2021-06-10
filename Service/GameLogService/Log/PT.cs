using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

using SP.Studio.IO;
using SP.Studio.Core;
using SP.Studio.Xml;
using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;
using BW.Common.Systems;

using SP.Studio.Text;
using SP.Studio.Net;
using SP.Studio.Json;

namespace GameLogService.Log
{
    public class PT : IDisposable
    {
        private GameInterface game;

        /// <summary>
        /// 本地导入的数量
        /// </summary>
        private int count = 0;

        private Stopwatch sw;

        private BW.GateWay.Games.PT Setting
        {
            get
            {
                return (BW.GateWay.Games.PT)this.game.Setting;
            }
        }

        public PT(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();
            this.game = game;
        }

        /// <summary>
        /// 上一次的时间
        /// </summary>
        private static DateTime? startTime;

        /// <summary>
        /// 导入记录
        /// </summary>
        public void Import()
        {
            if (startTime == null) startTime = GameAgent.Instance().GetLogStartTime(game.Type);

            if (startTime < DateTime.Now.AddDays(-1)) startTime = DateTime.Now.AddDays(-1);
            DateTime now = DateTime.Now;
            if (startTime < now.AddMinutes(-30))
            {
                now = startTime.Value.AddMinutes(30);
            }

            string url = string.Format("{0}customreport=getdata&reportname=PlayerGames&startdate={1}&enddate={2}&frozen=all",
                this.Setting.Gateway, startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (Program.debug) Console.WriteLine(url);

            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            if (!result.StartsWith("{"))
            {
                Utils.SaveLog(string.Format("[PT]读取数据发生错误\n\rURL:{0}\n\r{1}", url, result));
                return;
            };
            result = string.Concat("[", StringAgent.GetString(result, "\"result\":[", "]"), "]");

            Hashtable[] list = JsonAgent.GetJList(result);

            List<SlotLog> logList = new List<SlotLog>();
            foreach (Hashtable ht in list)
            {
                SlotLog slot = new SlotLog(this.game.Type, ht);
                if (GameAgent.Instance().ImportLog(slot))
                {
                    count++;
                    logList.Add(slot);
                }
            }

            foreach (var t in logList.Where(t => t.UserID != 0).GroupBy(t => t.UserID).Select(t => new { UserID = t.Key, UpdateAt = t.Max(p => p.PlayAt) }))
            {
                UserAgent.Instance().UpdateGameAccountMoney(t.UserID, GameType.PT);
            }
            startTime = now;
        }

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.game.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }
    }
}
