using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Configuration;
using System.Diagnostics;

using System.Timers;

using BW.Framework;
using BW.Agent;


namespace WeChatService
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


        /// <summary>
        /// 开奖定时器（从数据库内读取最新的开奖结果,3秒执行一次）
        /// </summary>
        private static Timer rewardTimer = new Timer(3 * 1000);

        private static int timerIndex = 0;

        static void Main(string[] args)
        {

            // 系统定时器
            timer.Elapsed += SysSetting.timer_Elapsed;
            timer.Start();

            Stopwatch sw = new Stopwatch();
            while (timerIndex < 100)
            {
                timerIndex++;
                Console.Title = string.Format("第{0}次执行任务", timerIndex);
                sw.Restart();
                SysSetting.rewardTimer_Elapsed(null, null);

                System.Threading.Thread.Sleep(Math.Max(1000, 6000 - (int)sw.ElapsedMilliseconds));
            }

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
        /// 反奖是否正在运行
        /// </summary>
        private static bool rewardStart = true;

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
