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

using SP.Studio.IO;
using SP.Studio.Core;
using SP.Studio.Xml;
using BW.Agent;
using BW.Common.Games;
using BW.Common.Users;
using BW.Common.Systems;

using SP.Studio.Net;

namespace GameLogService.Log
{
    /// <summary>
    /// AG
    /// </summary>
    public class AG : IDisposable
    {
        private GameInterface game;

        /// <summary>
        /// 本地导入的数量
        /// </summary>
        private int count = 0;


        private Stopwatch sw;

        /// <summary>
        /// 日志的根目录
        /// </summary>
        private string gPath
        {
            get
            {
                return ConfigurationManager.AppSettings["agPath"];
            }
        }

        private BW.GateWay.Games.AG Setting
        {
            get
            {
                return (BW.GateWay.Games.AG)this.game.Setting;
            }
        }

        public AG(GameInterface game)
        {
            this.sw = new Stopwatch();
            this.sw.Start();
            this.game = game;
            FileAgent.CreateDirectory(this.gPath, false);
        }

        #region ================  导入相关  =============

        private List<GameAccount> accountList;
        /// <summary>
        /// 开始导入日志文件
        /// </summary>
        public void Import()
        {
            this.accountList = new List<GameAccount>();

            List<string> gpath = new List<string>();
            foreach (string type in new string[] { "AGIN", "HUNTER", "XIN", "YOPLAY" })
            {
                for(int index = 3; index >= 0; index--)
                {
                    string date = DateTime.Now.AddDays(index * -1).ToString("yyyyMMdd");
                    gpath.Add(string.Format(@"{0}\{1}\{2}", this.gPath, type, date));
                }
            }

            foreach (string dateFolder in gpath)
            {
                if (!Directory.Exists(dateFolder))
                {
                    continue;
                }
                XElement complete = null;
                string completeFile = dateFolder + @"\complete.config";
                if (File.Exists(completeFile))
                {
                    complete = XElement.Parse(File.ReadAllText(completeFile, Encoding.UTF8));
                }
                else
                {
                    complete = new XElement("root");
                }

                string folderType = Regex.Match(dateFolder, "(?<Type>AGIN|HUNTER|XIN|YOPLAY)").Value;
                if (Program.debug) Console.WriteLine(folderType + " " + dateFolder);

                foreach (string file in Directory.GetFiles(dateFolder, "*.xml").OrderBy(t => t))
                {
                    FileInfo info = new FileInfo(file);
                    string fileName = file.Substring(file.LastIndexOf(@"\") + 1);

                    XElement item = complete.Elements().Where(t => t.GetAttributeValue("file") == fileName).FirstOrDefault();
                    if (item == null)
                    {
                        item = new XElement("item");
                        item.SetAttributeValue("file", fileName);
                        item.SetAttributeValue("size", info.Length);
                        complete.Add(item);
                    }
                    else
                    {
                        if (item.GetAttributeValue("size", 0) == info.Length) continue;
                        item.SetAttributeValue("size", info.Length);
                    }

                    XElement root = XElement.Parse(string.Concat("<root>", File.ReadAllText(file, Encoding.UTF8), "</root>"));
                    foreach (XElement row in root.Elements())
                    {
                        switch (row.GetAttributeValue("dataType"))
                        {
                            case "BR":  // 百家乐游戏
                                switch (folderType)
                                {
                                    case "AGIN":
                                        this.ImportVideoLog(row);
                                        break;
                                    case "YOPLAY":
                                        this.ImportSlotLog(row);
                                        break;
                                }
                                break;
                            case "HSR": // 捕鱼王
                            case "EBR": // 电子游戏
                                this.ImportSlotLog(row);
                                break;
                        }
                    }
                }

                File.WriteAllText(completeFile, complete.ToString(), Encoding.UTF8);
            }

            foreach (var item in this.accountList.GroupBy(t => t.UserID).Select(t => new { UserID = t.Key, Time = t.Max(p => p.UpdateAt) }))
            {
                UserAgent.Instance().UpdateGameAccountMoney(item.UserID, GameType.AG);
            }
        }

        /// <summary>
        /// 导入真人游戏记录
        /// </summary>
        /// <param name="item"></param>
        private void ImportVideoLog(XElement item)
        {
            try
            {
                VideoLog log = new VideoLog(game.Type, item);
                GameAgent.Instance().MessageClean();
                if (GameAgent.Instance().ImportLog(log))
                {
                    if (log.UserID != 0) this.accountList.Add(new GameAccount() { UserID = log.UserID, Money = log.Balance, UpdateAt = log.EndAt });
                    count++;
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex);
            }
        }

        /// <summary>
        /// 导入电子游戏记录(电子游戏、捕鱼王）
        /// </summary>
        /// <param name="item"></param>
        private void ImportSlotLog(XElement item)
        {
            try
            {
                GameAgent.Instance().MessageClean();
                SlotLog log = new SlotLog(game.Type, item);
                if (GameAgent.Instance().ImportLog(log))
                {
                    if (log.UserID != 0 && log.Balance != decimal.MinusOne) this.accountList.Add(new GameAccount() { UserID = log.UserID, Money = log.Balance, UpdateAt = log.PlayAt });
                    count++;
                }
            }
            catch (Exception ex)
            {
                Utils.SaveLog(ex);
            }
        }


        #endregion

        public void Dispose()
        {
            Console.WriteLine("[{0}] {1}执行完毕,总共导入:{2}条日志\t耗时：{3}ms", DateTime.Now, this.game.Type.GetDescription(), this.count, this.sw.ElapsedMilliseconds);
        }
    }
}
