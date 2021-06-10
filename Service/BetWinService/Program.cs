using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Threading;

using System.Timers;
using Timer = System.Timers.Timer;

using BW.Framework;
using BW.Agent;
using BW.Common.Lottery;


namespace BetWinService
{
    /// <summary>
    /// BetWinService  开奖服务
    /// </summary>
    class Program
    {
        /// <summary>
        /// 系统定时器，1秒一次（可被外部事件加载）
        /// </summary>
        public static Timer timer = new Timer(1000);


        private static int timerIndex = 0;

        /// <summary>
        /// 反奖是否正在运行
        /// </summary>
        private static bool rewardStart = true;

        private static Stopwatch sw = new Stopwatch();

        /// <summary>
        /// 派奖次数
        /// </summary>
        private static ConcurrentDictionary<int, int> rewardCount = new ConcurrentDictionary<int, int>();

        static void Main(string[] args)
        {
            for (int i = 0; i < BetModule.TABLE_COUNT; i++)
            {
                rewardCount.TryAdd(i, 0);
            }
            // 系统定时器
            timer.Elapsed += SysSetting.timer_Elapsed;
            timer.Elapsed += SysSetting.rewardTimer_Elapsed;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            new Thread(AgentBet).Start();

            sw.Restart();
            Parallel.For(0, BetModule.TABLE_COUNT, tableId =>
            {
                while (true)
                {
                    try
                    {
                        LotteryAgent.Instance().OpenRewardByTable(tableId);
                        rewardCount[tableId]++;
                    }
                    catch (Exception ex)
                    {
                        SystemAgent.Instance().AddErrorLog(0, ex, "派奖发生错误");
                    }
                    finally
                    {
                        Thread.Sleep(500);
                    }

                    if (rewardCount.Min(t => t.Value) > 1024)
                    {
                        break;
                    }
                }
            });

            rewardStart = false;

            timer.Stop();
            timerIndex = 0;
            while (SysSetting._timer_run_status)
            {
                timerIndex++;
                if (timerIndex > 30) break;
                System.Threading.Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (consoleAgentBet.Count != 0)
            {
                Console.WriteLine(string.Join("\n", consoleAgentBet));
                consoleAgentBet.Clear();
            }
            Console.Write("[{0}]执行派奖任务\t", string.Join(",", rewardCount.Select(t => t.Value)));
            int success = LotteryAgent.Instance().openRewardResult[true];
            int faild = LotteryAgent.Instance().openRewardResult[false];
            int total = success + faild;
            if (total != 0)
            {
                LotteryAgent.Instance().openRewardResult[true] = 0;
                LotteryAgent.Instance().openRewardResult[false] = 0;
                Console.Write("{0}:{1}/{2}  速度：{3}单/秒", success, faild, total, Math.Round((double)total / sw.Elapsed.TotalSeconds, 2));
            }
            Console.WriteLine();
            sw.Restart();
        }

        private static Dictionary<int, List<BetAgentRate>> betAgentRate = new Dictionary<int, List<BetAgentRate>>();

        private static List<string> consoleAgentBet = new List<string>();

        /// <summary>
        /// 代理返点
        /// </summary>
        private static void AgentBet()
        {
            while (rewardStart)
            {
                try
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Restart();
                    using (LotteryAgent agent = new LotteryAgent())
                    {
                        int count = agent.RunBetAgent();
                        consoleAgentBet.Add(string.Format("执行返点\t总共{0}单，耗时：{1}ms\t速度{2}单/秒", count, sw.ElapsedMilliseconds, Math.Round((double)count / sw.Elapsed.TotalSeconds, 2)));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    SystemAgent.Instance().AddErrorLog(0, ex, "运行代理返点失败");
                }
                finally
                {
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }
    }
}
