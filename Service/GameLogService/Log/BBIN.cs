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
using SP.Studio.Security;

namespace GameLogService.Log
{
    public class BBIN : IDisposable
    {
        private GameInterface game;

        /// <summary>
        /// 本地导入的数量
        /// </summary>
        private int count = 0;

        private Stopwatch sw;

        private BW.GateWay.Games.BBIN Setting
        {
            get
            {
                return (BW.GateWay.Games.BBIN)this.game.Setting;
            }
        }

        /// <summary>
        /// 上一次读取的时间
        /// </summary>
        private static Dictionary<string, DateTime> lastTime = new Dictionary<string, DateTime>();

        public BBIN(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();
            this.game = game;
        }

        /// <summary>
        /// 返回随机码（小写）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private string getRandom(int length)
        {
            return Guid.NewGuid().ToString("N").Substring(0, length);
        }

        /// <summary>
        /// 有日志的用户
        /// </summary>
        private List<int> userlist = new List<int>();

        public void Import(int count)
        {
            if (Program.debug)
            {
                this.ImportVideLog();
                return;
            }
            userlist.Clear();
            switch (count % 3)
            {
                case 0:
                    this.ImportVideLog();
                    break;
                case 1:
                    this.ImportSlotLog("1");
                    System.Threading.Thread.Sleep(3000);
                    this.ImportSlotLog("2");
                    System.Threading.Thread.Sleep(3000);
                    this.ImportSlotLog("3");
                    System.Threading.Thread.Sleep(3000);
                    break;
                case 2:
                    this.ImportSport();
                    break;
            }
            foreach (int userId in userlist.Where(t => t != 0).Distinct())
            {
                UserAgent.Instance().UpdateGameAccountMoney(userId, GameType.BBIN);
            }
        }

        /// <summary>
        /// 导入视频游戏记录
        /// </summary>
        public void ImportVideLog()
        {
            DateTime now;
            string result = this.GetResult("Video", out now);
            if (string.IsNullOrEmpty(result)) return;

            XElement root = XElement.Parse(result);
            if (root.GetAttributeValue("TotalNumber") == null) return;
            List<VideoLog> logList = new List<VideoLog>();
            foreach (XElement item in root.Elements("Record"))
            {
                if (item.Elements().Count() == 0) continue;
                VideoLog video = new VideoLog(this.game.Type, item);
                logList.Add(video);
                if (Program.debug) Console.WriteLine("找到日志:{0}", video.GameName);
            }

            foreach (var t in logList.Where(t => t.UserID != 0).GroupBy(t => t.UserID).Select(t => new { UserID = t.Key, UpdateAt = t.Max(p => p.CreateAt) }))
            {
                decimal balance = this.Setting.GetBalance(t.UserID);
                UserAgent.Instance().UpdateGameAccountMoney(0, t.UserID, this.game.Type, balance, t.UpdateAt);
                foreach (VideoLog log in logList.Where(p => p.UserID == t.UserID))
                {
                    log.Balance = balance;
                }
            }

            logList.ForEach(video =>
            {
                if (GameAgent.Instance().ImportLog(video))
                {
                    count++;
                    userlist.Add(video.UserID);
                }
                else
                {
                    if (Program.debug)
                    {
                        Console.WriteLine("导入日志失败,{0}", video.GameName);
                    }
                }
            });
            lastTime["Video"] = now;
        }

        /// <summary>
        /// 导入电子游戏记录
        /// </summary>
        public void ImportSlotLog(string subgamekind = "1")
        {
            DateTime now;
            Dictionary<string, string> extendData = new Dictionary<string, string>();
            extendData.Add("subgamekind", subgamekind);
            string result = this.GetResult("Slot", out now, extendData);
            if (string.IsNullOrEmpty(result)) return;

            XElement root = XElement.Parse(result);
            if (root.GetAttributeValue("TotalNumber") == null) return;
            List<SlotLog> logList = new List<SlotLog>();
            foreach (XElement item in root.Elements("Record"))
            {
                if (item.Elements().Count() == 0) continue;
                SlotLog slot = new SlotLog(this.game.Type, item);
                logList.Add(slot);
                if (Program.debug) Console.WriteLine("找到日志:{0}", slot.GameName);
            }

            foreach (var t in logList.Where(t => t.UserID != 0).GroupBy(t => t.UserID).Select(t => new { UserID = t.Key, UpdateAt = t.Max(p => p.CreateAt) }))
            {
                decimal balance = this.Setting.GetBalance(t.UserID);
                UserAgent.Instance().UpdateGameAccountMoney(0, t.UserID, this.game.Type, balance, t.UpdateAt);
                foreach (SlotLog log in logList.Where(p => p.UserID == t.UserID))
                {
                    log.Balance = balance;
                }
            }

            logList.ForEach(slot =>
            {
                if (GameAgent.Instance().ImportLog(slot))
                {
                    userlist.Add(slot.UserID);
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
            string timeKey = "Slot" + "-" + string.Join("&", extendData.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            lastTime[timeKey] = now;
        }

        /// <summary>
        /// 导入体育记录
        /// </summary>
        public void ImportSport()
        {
            DateTime now;
            List<SportLog> sportList = GameAgent.Instance().GetSportLogByNone();

            List<DateTime> timeList = new List<DateTime>();
            timeList.Add(DateTime.Now.AddHours(-12).Date);
            sportList.ForEach(t =>
            {
                DateTime date = t.PlayAt.AddHours(-12).Date;
                if (!timeList.Contains(date)) timeList.Add(date);
            });

            foreach (DateTime queryTime in timeList)
            {
                lastTime["Sport"] = queryTime;
                string result = this.GetResult("Sport", out now);
                XElement root = XElement.Parse(result);
                if (root.GetAttributeValue("TotalNumber") == null) return;

                List<SportLog> logList = new List<SportLog>();
                foreach (XElement item in root.Elements("Record"))
                {
                    if (item.Elements().Count() == 0) continue;
                    SportLog sport = new SportLog(this.game.Type, item);
                    logList.Add(sport);
                    if (Program.debug) Console.WriteLine("找到日志:{0}", sport.Result);
                }

                if (Program.debug)
                {
                    Console.WriteLine("日志数量:{0}", logList.Count);
                }

                logList.ForEach(sport =>
                {
                    if (GameAgent.Instance().ImportLog(sport))
                    {
                        userlist.Add(sport.UserID);
                        count++;
                    }
                    else
                    {
                        if (Program.debug)
                        {
                            Console.WriteLine("导入日志失败,{0}", sport.GameType);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 查询没有开奖的体育订单
        /// </summary>
        public void ImportSport2()
        {
            List<SportLog> list = GameAgent.Instance().GetSportLogByNone();

            list.ForEach(t =>
            {
                this.GetResultBySport(t.PlayAt);
            });
        }

        /// <summary>
        /// 查询体育注单变化日志
        /// </summary>
        /// <param name="time"></param>
        private void GetResultBySport(DateTime time)
        {
            time = time.AddHours(-12);
            string url = string.Format("{0}?Method=BetRecordByModifiedDate3", this.Setting.GateWay);
            string date = time.ToShortDateString();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("website", this.Setting.website);
            dic.Add("start_date", date);
            dic.Add("end_date", date);
            dic.Add("starttime", time.AddMinutes(-2).ToString("HH:mm:ss"));
            dic.Add("endtime", time.AddMinutes(2).ToString("HH:mm:ss"));
            dic.Add("gamekind", "1");
            string source = this.Setting.website + this.Setting.BetRecordByModifiedDate3 + time.ToString("yyyyMMdd");
            dic.Add("key", string.Concat(this.getRandom(4), MD5.toMD5(source), this.getRandom(1)).ToLower());

            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            string result = this.GetResult(data, "BetRecordByModifiedDate3");
        }

        /// <summary>
        /// 获取日志服务器返回结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="now">当前查询的时间终点</param>
        /// <param name="extendData">扩展的数据查询</param>
        /// <returns></returns>
        private string GetResult(string type, out DateTime now, Dictionary<string, string> extendData = null)
        {
            string timeKey = type;
            if (extendData != null)
            {
                timeKey += "-" + string.Join("&", extendData.Select(t => string.Format("{0}={1}", t.Key, t.Value)));
            }

            DateTime startTime = lastTime.ContainsKey(timeKey) ? lastTime[timeKey] : DateTime.MinValue;
            string gamekind = null;
            switch (type)
            {
                case "Video":
                    gamekind = "3";
                    break;
                case "Slot":
                    gamekind = "5";
                    break;
                case "Sport":
                    gamekind = "1";
                    break;
                case "Lottery":
                    type = "Slot";
                    gamekind = "12";
                    break;
            }

            // 当前时间（美东），5分钟之内的游戏不去采集，可能正在进行中
            DateTime timeNow = DateTime.Now.AddHours(-12).AddMinutes(-5);

            // 要查询的结束时间
            DateTime endTime = DateTime.MinValue;

            // 需要增加一天
            bool nextDate = false;

            if (type != "Sport")
            {
                // 开始时间（美东）
                if (startTime == DateTime.MinValue)
                {
                    startTime = GameAgent.Instance().GetLogStartTime(game.Type, type).AddHours(-12);
                }

                if (startTime.TimeOfDay.TotalSeconds > 30) startTime = startTime.AddSeconds(-30);

                if (startTime.AddHours(12).Date < DateTime.Now.Date)
                {
                    endTime = startTime.AddHours(8);
                }
                else
                {
                    endTime = startTime.AddMinutes(30);
                }
                if (endTime > timeNow) endTime = timeNow;

                if (startTime.Date != endTime.Date)
                {
                    endTime = endTime.Date.AddSeconds(-1);
                    nextDate = true;
                }
            }

            string date = startTime.ToShortDateString();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            string username = string.Empty;
            dic.Add("website", this.Setting.website);
            dic.Add("username", username);
            dic.Add("uppername", this.Setting.uppername);
            dic.Add("rounddate", date);
            if (type != "Sport")
            {
                dic.Add("starttime", startTime.ToString("HH:mm:ss"));
                dic.Add("endtime", endTime.ToString("HH:mm:ss"));
            }
            dic.Add("gamekind", gamekind);
            if (extendData != null)
            {
                foreach (KeyValuePair<string, string> item in extendData)
                {
                    dic.Add(item.Key, item.Value);
                }
            }
            //验证码(需全小写)，组成方式如下: key=A+B+C(验证码组合方式) A= 无意义字串长度4码 B=MD5(website+ username + KeyB + YYYYMMDD)
            //C=无意义字串长度1码 YYYYMMDD为美东时间(GMT-4)(20160330)
            string source = this.Setting.website + username + this.Setting.BetRecord + DateTime.Now.AddHours(-12).ToString("yyyyMMdd");
            dic.Add("key", string.Concat(this.getRandom(1), MD5.toMD5(source), this.getRandom(8)).ToLower());

            if (Program.debug) Console.WriteLine(source);
            string data = string.Join("&", dic.Select(t => string.Format("{0}={1}", t.Key, t.Value)));

            string result = this.GetResult(data);

            if (!result.StartsWith("<"))
            {
                Utils.SaveLog(string.Format("[BBIN]读取数据发生错误\n\rURL:{0}&{1}\n\r{1}", data, result));
                now = startTime;
                return null;
            };
            if (result.Contains("<Code>44003</Code>"))
            {
                if (Program.debug) Console.WriteLine("频繁请求，停止30秒后继续");
                now = startTime;
                System.Threading.Thread.Sleep(60 * 1000);
                return null;
            }

            if (nextDate)
            {
                now = startTime.Date.AddDays(1);
            }
            else
            {
                now = endTime;
            }
            return result;
        }

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.game.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// 拼接接口地址
        /// </summary>
        /// <param name="data"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private string GetResult(string data, string method = "BetRecord")
        {
            string url = string.Format("{0}{1}?{2}", this.Setting.GateWay, method, data);
            if (Program.debug) Console.WriteLine(url);
            string result = NetAgent.DownloadData(url, Encoding.UTF8);
            if (Program.debug) Console.WriteLine(result);
            return result;
        }
    }
}
