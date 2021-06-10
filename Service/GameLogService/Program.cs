using BW.Agent;
using BW.Common.Games;
using BW.Common.Systems;
using GameLogService.Log;
using SP.Studio.ErrorLog;
using SP.Studio.IO;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogService
{
    internal class Program
    {
        /// <summary>
        /// 是否是调试模式
        /// </summary>
        public static bool debug = false;

        private static void Main(string[] args)
        {
            if (args.Contains("-debug")) debug = true;


            int count = 1;

            while (count < 1024)
            {
                try
                {
                    IEnumerable<GameInterface> gameList = SystemAgent.Instance().GetGameInterfaceList();

                    if (debug)
                    {
                        gameList = gameList.Where(t => args.Contains(t.Type.ToString()));
                    }
                    Parallel.ForEach(gameList, game =>
                    {
                        switch (game.Type)
                        {
                            case GameType.AG:
                                using (AG ag = new AG(game))
                                {
                                    ag.Import();
                                }
                                break;
                            case GameType.PT:
                                using (PT pt = new PT(game))
                                {
                                    pt.Import();
                                }
                                break;
                            case GameType.BBIN:
                                using (BBIN bbin = new BBIN(game))
                                {
                                    bbin.Import(count);
                                }
                                break;
                            case GameType.SunBet:
                                using (SunBet sunbet = new SunBet(game))
                                {
                                    sunbet.Import();
                                }
                                break;
                            case GameType.MW:
                                using (MW mw = new MW(game))
                                {
                                    mw.Import();
                                }
                                break;
                            case GameType.MG:
                                using (MG mg = new MG(game))
                                {
                                    mg.Import();
                                }
                                break;
                            case GameType.BWGaming:
                                using (BWGaming bw = new BWGaming(game))
                                {
                                    bw.Import();
                                }
                                break;
                            case GameType.OG:
                                using (OG og = new OG(game))
                                {
                                    og.Import();
                                }
                                break;
                        }
                    });
                }
                catch (Exception ex)
                {
                    Utils.SaveLog(ex);
                }

                Console.WriteLine("[{0}] 第{1}次执行完毕", DateTime.Now, count++);
                //System.Threading.Thread.Sleep(30 * 1000);
            }

            Console.WriteLine("[{0}]任务执行完毕，重启准备下一次执行", DateTime.Now);
        }
    }
}
