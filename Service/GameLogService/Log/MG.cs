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
    public class MG : IDisposable
    {
        private GameInterface game;

        /// <summary>
        /// 本地导入的数量
        /// </summary>
        private int count = 0;

        protected virtual BW.GateWay.Games.MG Setting
        {
            get
            {
                return (BW.GateWay.Games.MG)this.game.Setting;
            }
        }

        private Stopwatch sw;

        public MG(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();
            this.game = game;
        }

        /// <summary>
        /// 上一次读取的时间
        /// </summary>
        private static DateTime lastTime = DateTime.Now.AddDays(-3);

        /// <summary>
        /// 导入数据
        /// </summary>
        public void Import()
        {
            try
            {
                DateTime endTime = lastTime.AddMinutes(30);
                if (endTime > DateTime.Now) endTime = DateTime.Now;
                if (lastTime.AddMinutes(3) > endTime) return;
                string result = this.Setting.GetLog(lastTime.AddMinutes(-3), endTime);
                Hashtable ht = JsonAgent.GetJObject(result);
                if (ht == null || !ht.ContainsKey("data")) return;
                Hashtable[] list = JsonAgent.GetJList(ht["data"].ToString());
                foreach (Hashtable t in list)
                {
                    SlotLog log = new SlotLog(GameType.MG, t);
                    if (GameAgent.Instance().ImportLog(log)) this.count++;
                    //Console.WriteLine("{5}  用户{0} {1} 时间：{2} 投注：{3}/{4}", log.UserID, log.GameName, log.PlayAt, log.BetAmount, log.Money, log.BillNo);
                }
                lastTime = endTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.game.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }
    }
}
