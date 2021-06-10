using System;
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
using BW.Common;
using BW.Common.Games;
using BW.Common.Users;
using BW.Common.Systems;

using SP.Studio.Net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameLogService.Log
{
    /// <summary>
    /// 泛亚电竞
    /// </summary>
    public class BWGaming : IDisposable
    {
        private Stopwatch sw;

        private int count = 0;


        public GameType Type
        {
            get
            {
                return GameType.BWGaming;
            }
        }

        /// <summary>
        /// 开启了接口的对象
        /// </summary>
        private BW.GateWay.Games.BWGaming[] setting;

        public BWGaming(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();

            string[] setting = GameAgent.Instance().GetGameSetting(this.Type).Where(t => t.IsOpen && !string.IsNullOrEmpty(t.SettingString)).Select(t => t.SettingString).ToArray();

            this.setting = setting.Select(t => (BW.GateWay.Games.BWGaming)new GameInterface()
            {
                IsOpen = true,
                SettingString = t,
                Type = this.Type
            }.Setting).ToArray();
        }

        private static DateTime time = DateTime.Now.AddDays(-1);

        /// <summary>
        /// 导入游戏
        /// </summary>
        public void Import()
        {
            DateTime endTime = time.AddMinutes(30);
            if (endTime > DateTime.Now) endTime = DateTime.Now;
            //if (time > endTime) time = endTime.AddMinutes(-15);

            if (Program.debug) Console.WriteLine("{0}～{1}", time, endTime);
            foreach (BW.GateWay.Games.BWGaming game in this.setting)
            {
                try
                {
                    if (Program.debug) Console.WriteLine(game.ToString());
                    this.Import(game, DateTime.Parse("2018/7/6 14:47:15"), DateTime.Now, "UpdateAt");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            time = endTime.AddMinutes(-1);
        }

        private void Import(BW.GateWay.Games.BWGaming setting, DateTime startAt, DateTime endAt, string type = "CreateAt")
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("StartAt", startAt.ToString());
            data.Add("EndAt", endAt.ToString());
            data.Add("PageSize", "1024");
            data.Add("Type", type);

            string msg;
            JObject info;

            bool result = this.POST(setting.GATEWAY, setting.SECRETKEY, "log", "get", data, out msg, out info);
            if (!result)
            {
                if (Program.debug) Console.WriteLine(msg);
                return;
            }
            foreach (JObject dr in (JArray)info["list"])
            {
                XElement item = new XElement("item");
                item.SetAttributeValue("OrderID", dr["OrderID"].Value<int>());
                item.SetAttributeValue("UserName", dr["UserName"].Value<string>());
                item.SetAttributeValue("Category", dr["Category"].Value<string>());
                item.SetAttributeValue("League", dr["League"].Value<string>());
                item.SetAttributeValue("Match", dr["Match"].Value<string>());
                item.SetAttributeValue("Bet", dr["Bet"].Value<string>());
                item.SetAttributeValue("Content", dr["Content"].Value<string>());
                item.SetAttributeValue("BetAmount", dr["BetAmount"].Value<decimal>());
                item.SetAttributeValue("Status", dr["Status"].Value<string>());
                item.SetAttributeValue("CreateAt", dr["CreateAt"].Value<DateTime>());
                item.SetAttributeValue("Result", dr["Result"].Value<string>());
                item.SetAttributeValue("Money", dr["Money"].Value<decimal>());
                item.SetAttributeValue("UpdateAt", dr["UpdateAt"].Value<DateTime>());
                SportLog log = new SportLog(this.Type, item);
                if (GameAgent.Instance().ImportLog(log)) count++;
            }
        }

        /// <summary>
        /// 发送信息到API
        /// </summary>
        /// <param name="action">接口</param>
        /// <param name="method">动作</param>
        /// <param name="data">要发送的数据</param>
        /// <param name="msg">返回的信息</param>
        /// <param name="info">返回的数据</param>
        /// <returns></returns>
        private bool POST(string gateway, string secretKey, string action, string method, Dictionary<string, string> data, out string msg, out JObject info)
        {
            string url = gateway + action + "/" + method;

            using (WebClient wc = new WebClient())
            {
                try
                {
                    wc.Headers.Add("Authorization", secretKey);
                    wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
                    string postData = string.Join("&", data.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
                    byte[] resultData = wc.UploadData(url, "POST", Encoding.UTF8.GetBytes(postData));
                    string result = Encoding.UTF8.GetString(resultData);
                    JObject obj = (JObject)JsonConvert.DeserializeObject(result);
                    if ((int)obj["success"] != 1)
                    {
                        msg = (string)obj["msg"];
                        info = null;
                        return false;
                    }
                    msg = (string)obj["msg"];
                    if (obj["info"].GetType() == typeof(JValue))
                    {
                        info = null;
                    }
                    else
                    {
                        info = (JObject)obj["info"];
                    }
                    return true;
                }
                catch (WebException ex)
                {
                    info = null;
                    msg = string.Format("Error:{0}", ex.Message);
                    if (ex.Response != null)
                    {
                        StreamReader reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8);
                        if (reader != null)
                        {
                            msg += string.Format("\n<hr />\n{0}", reader.ReadToEnd());
                        }
                    }
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }
    }
}
