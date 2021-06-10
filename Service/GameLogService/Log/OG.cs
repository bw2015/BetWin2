using BW.Agent;
using BW.Common.Games;
using BW.Common.Systems;
using Newtonsoft.Json.Linq;
using SP.Studio.Core;
using SP.Studio.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogService.Log
{
    public class OG : IDisposable
    {

        private GameInterface game;

        /// <summary>
        /// 本地导入的数量
        /// </summary>
        private int count = 0;


        private Stopwatch sw;


        private BW.GateWay.Games.OG Setting
        {
            get
            {
                return (BW.GateWay.Games.OG)this.game.Setting;
            }
        }

        public OG(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();
            this.game = game;
        }
        private string timePath
        {
            get
            {
                return System.Windows.Forms.Application.StartupPath + @"\OG.log";
            }
        }
        /// <summary>
        /// 上一次读取的时间
        /// </summary>
        private DateTime startTime
        {
            get
            {
                if (!File.Exists(timePath)) return DateTime.Now.AddDays(-1);
                string content = File.ReadAllText(timePath);
                DateTime _startTime;
                DateTime.TryParse(content, out _startTime);
                if (_startTime < DateTime.Now.AddDays(-1)) return DateTime.Now.AddDays(-1);
                return _startTime.AddMinutes(-5);
            }
            set
            {
                File.WriteAllText(timePath, value.ToString());
            }
        }

        /// <summary>
        /// 开始导入
        /// </summary>
        public void Import()
        {
            if (DateTime.Now.TimeOfDay.TotalMinutes > 4 * 60 && DateTime.Now.TimeOfDay.TotalMinutes < 5 * 60 + 30) return;
            DateTime endTime = startTime.AddMinutes(10);
            if (endTime > DateTime.Now) endTime = DateTime.Now;
            if (startTime.AddMinutes(2) > endTime) return;

            Console.WriteLine("查询时间段：{0}~{1}", startTime, endTime);
            string result = this.Setting.GetLog(startTime, endTime);
            // Console.WriteLine(result);
            JArray list = JArray.Parse(result);
            foreach (JObject t in list)
            {
                //Console.WriteLine(t);
                VideoLog log = new VideoLog(GameType.OG, t);
                if (GameAgent.Instance().ImportLog(log)) this.count++;
                //Console.WriteLine("{5}  用户{0} {1} 时间：{2} 投注：{3}/{4}", log.UserID, log.GameName, log.StartAt, log.BetAmount, log.Money, log.BillNo);
            }
            startTime = endTime;
            System.Threading.Thread.Sleep(10 * 1000);
        }

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.game.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }
    }
}
