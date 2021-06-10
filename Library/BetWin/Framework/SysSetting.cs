using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Timers;
using System.Web;
using System.Diagnostics;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Games;
using BW.Common.Lottery;

namespace BW.Framework
{
    /// <summary>
    /// 系统全局缓存
    /// </summary>
    public sealed class SysSetting
    {
        #region ============= 静态方法，定时器  =============


        /// <summary>
        /// 彩票采集器的定时器工具
        /// </summary>
        private static Timer lotteryTimer = new Timer(3000);

        /// <summary>
        /// 计时器
        /// </summary>
        private static int timerIndex = 0;

        /// <summary>
        /// 开奖定时器
        /// </summary>
        private static int rewardTimerIndex = 0;

        /// <summary>
        /// 系统是否已经开始运行
        /// </summary>
        public readonly static bool START = false;


        static SysSetting()
        {
            try
            {
                SysSetting.GetSetting().Install();
                if (string.IsNullOrEmpty(SysSetting.GetSetting().DbConnection)) return;

                if (SysSetting.GetSetting().Platform == "WEB")
                {
                    lotteryTimer.Elapsed += lotteryTimer_Elapsed;
                    lotteryTimer.Start();
                }
            }
            finally
            {
                START = true;
            }
        }

        /// <summary>
        /// 彩种名字缓存
        /// </summary>
        //private static Dictionary<string, string> _lotteryNameCache;
        /// <summary>
        /// 玩法缓存
        /// </summary>
        //private static Dictionary<int, LotteryPlayer> _playerCache;
        /// <summary>
        /// 开奖器是否正在执行
        /// </summary>
        private static bool rewardTimerStatus = false;
        /// <summary>
        /// 派奖 + 追号服务生成器（只在Service里面运行）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void rewardTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //if (_lotteryNameCache == null)
            //{
            //    _lotteryNameCache = LotteryAgent.Instance().GetLotterySettingList().ToDictionary(t => string.Format("{0}-{1}", t.SiteID, t.Game), t => t.Name);
            //}

            //if (_playerCache == null)
            //{
            //    _playerCache = LotteryAgent.Instance().GetLotteryPlayerList().ToDictionary(t => t.ID, t => t);
            //}

            // 派奖 + 追号
            if (!rewardTimerStatus)
            {
                rewardTimerStatus = true;
                try
                {
                    // 追号和合买订单生成 30秒一次
                    if (rewardTimerIndex % 10 == 0)
                    {
                        try
                        {
                            Console.WriteLine("[{0}]生成追号订单", DateTime.Now);
                            LotteryAgent.Instance().BuildChaseOrder();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            SystemAgent.Instance().AddErrorLog(0, ex, "追号失败");
                        }

                        //try
                        //{
                        //    LotteryAgent.Instance().BuildUnitedOrder();
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.WriteLine(ex.Message);
                        //    SystemAgent.Instance().AddErrorLog(0, ex, "合买生成订单失败");
                        //}
                    }

                    // 自动撤单，1分钟一次
                    if (rewardTimerIndex % 20 == 0)
                    {
                        try
                        {
                            Console.WriteLine("[{0}]提交自动撤单", DateTime.Now);
                            LotteryAgent.Instance().Revoke();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            SystemAgent.Instance().AddErrorLog(0, ex, "自动撤单失败");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    SystemAgent.Instance().AddErrorLog(0, ex, "执行开奖定时任务失败");
                }
                finally
                {
                    rewardTimerStatus = false;
                    rewardTimerIndex++;
                }
            }
        }


        /// <summary>
        /// 上一次获取彩果的时间
        /// </summary>
        private static DateTime lastResultTime = new DateTime(2000, 1, 1);
        /// <summary>
        /// 彩票的定时器（3秒执行一次）
        /// 从数据库中读取开奖结果放入内存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void lotteryTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                List<ResultNumber> resultNumberList = LotteryAgent.Instance().GetResultNumber(lastResultTime);
                if (resultNumberList.Count != 0)
                {
                    lastResultTime = DateTime.Now.AddMinutes(-1);
                    resultNumberList.ForEach(t =>
                    {
                        SysSetting.GetSetting().SaveResultNumber(t);
                    });
                }
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(0, ex, "获取彩果失败");
            }
        }

        public static bool _timer_run_status { get; private set; }
        /// <summary>
        /// 系统任务执行
        /// 1、根据用户的活动时间更新在线状态
        /// 2、执行批量出款任务
        /// 3、检查第三方接口的出款状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_timer_run_status) return;
                _timer_run_status = true;
                // 1分钟执行一次
                if (timerIndex % 60 == 0)
                {
                    // 执行批量出款订单
                    UserAgent.Instance().ExecWithdrawOrder();

                    // 检查第三方接口出款的状态
                    UserAgent.Instance().CheckWithdrawStatus();
                }

                // 10分钟执行一次
                if (timerIndex % 600 == 0)
                {
                    //# 系统自动锁定未绑定银行卡且没有资金流水超过设定天数的账户，为0表示不执行该锁定条件
                    SystemAgent.Instance().TaskLockNoBankUser();

                    // 运行活动任务
                    SiteAgent.Instance().PlanRun();

                    // 检查契约状态
                    UserAgent.Instance().CheckContackLockStatus();
                }

                _timer_run_status = false;
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(0, ex, "系统任务执行错误");
            }
            finally
            {
                _timer_run_status = false;
            }

            timerIndex++;
        }

        #endregion

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string DbConnection { get; private set; }

        /// <summary>
        /// 快照数据库连接字符串
        /// </summary>
        public string SNAPConnection { get; private set; }

        /// <summary>
        /// 图片服务器地址 包括http://域名 最后没有斜杠
        /// </summary>
        public string imgServer { get; private set; }

        /// <summary>
        /// 系统中所有的授权域名
        /// </summary>
        private Dictionary<string, int> SiteDomain = new Dictionary<string, int>();

        /// <summary>
        /// 彩票的开奖时间模板
        /// </summary>
        internal Dictionary<LotteryType, List<TimeTemplate>> LotteryTimeTemplate { get; private set; }

        /// <summary>
        /// 系统的老虎机列表
        /// </summary>
        internal IEnumerable<SlotGame> SlogGameList { get; private set; }

        /// <summary>
        /// 特殊彩种的开奖时间
        /// </summary>
        internal Dictionary<LotteryType, List<StartTime>> LotteryStartTime { get; private set; }

        /// <summary>
        /// 开奖号码缓存
        /// </summary>
        internal Dictionary<LotteryType, Dictionary<string, string>> ResultNumber { get; private set; }

        /// <summary>
        /// 所属平台
        /// </summary>
        private string Platform { get; set; }

        public void Install()
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0) return;
            if (ConfigurationManager.ConnectionStrings["DbConnection"] == null) return;
            string connection = this.DbConnection;
            if (string.IsNullOrEmpty(connection))
            {
                connection = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            }

            this.Platform = ConfigurationManager.AppSettings["platform"];
            this.imgServer = ConfigurationManager.AppSettings["imgserver"];

            SP.Studio.Data.DataModule.DbConnection = this.DbConnection = connection;
            if (ConfigurationManager.ConnectionStrings["SNAPConnection"] == null)
            {
                this.SNAPConnection = this.DbConnection;
            }
            else
            {
                this.SNAPConnection = ConfigurationManager.ConnectionStrings["SNAPConnection"].ConnectionString;
            }
            this.LotteryTimeTemplate = LotteryAgent.Instance().GetTimeTemplateList();
            this.LotteryStartTime = LotteryAgent.Instance().GetStartTimeList();
            this.ResultNumber = new Dictionary<LotteryType, Dictionary<string, string>>();
            this.SlogGameList = GameAgent.Instance().GetSlotGameList();
        }

        #region =========== 私有方法 ============

        /// <summary>
        /// 保存开奖内容到缓存对象
        /// </summary>
        /// <param name="result"></param>
        private void SaveResultNumber(ResultNumber result)
        {
            if (!this.ResultNumber.ContainsKey(result.Type)) this.ResultNumber.Add(result.Type, new Dictionary<string, string>());
            if (!this.ResultNumber[result.Type].ContainsKey(result.Index)) this.ResultNumber[result.Type].Add(result.Index, result.Number);
        }

        /// <summary>
        /// 从缓存中获取开奖结果
        /// </summary>
        /// <param name="type"></param>
        /// <param name="index"></param>
        /// <param name="siteID">如果是系统彩则需要指定站点</param>
        /// <returns></returns>
        internal string GetResultNumber(LotteryType type, string index, int siteID = 0)
        {
            if (string.IsNullOrEmpty(index)) return null;
            if (!this.ResultNumber.ContainsKey(type)) return null;

            if (type.GetCategory().SiteLottery)
            {
                int site = GetSiteID();
                if (site == 0) site = siteID;
                if (site == 0) return null;
                index = string.Concat(site, ":", index);
            }

            if (this.ResultNumber[type] == null || !this.ResultNumber[type].ContainsKey(index)) return null;
            if (!type.GetCategory().SiteLottery)
            {
                if (HttpContext.Current != null)
                {
                    Dictionary<string, string> siteNumber = LotteryAgent.Instance().GetSiteResultNumber(type);
                    if (siteNumber.ContainsKey(index)) return siteNumber[index];
                }
            }
            return this.ResultNumber[type][index];
        }

        #endregion

        #region ============= 站内方法 ===============

        /// <summary>
        /// 获取当前站点的ID
        /// </summary>
        /// <returns></returns>
        internal int GetSiteID()
        {
            HttpContext context = HttpContext.Current;
            if (context == null) return 0;
            lock (this.SiteDomain)
            {
                string domain = context.Request.Url.Authority;
                if (!string.IsNullOrEmpty(context.Request.Headers[BetModule.SITEID]))
                {
                    domain = context.Request.Headers[BetModule.SITEID];
                }
                if (this.SiteDomain.ContainsKey(domain)) return this.SiteDomain[domain];

                Regex regex = new Regex(@"1\d{3}$");
                if (!regex.IsMatch(domain)) return 0;

                int siteId = int.Parse(regex.Match(domain).Value);
                this.SiteDomain.Add(domain, siteId);
                return siteId;
            }
        }

        /// <summary>
        /// 获取图片服务器的路径
        /// </summary>
        public string GetImage(string path)
        {
            if (string.IsNullOrEmpty(path)) path = "/images/space.gif";

            if (path.StartsWith("http") || path.StartsWith("//")) return path;

            return string.Format("{0}{1}", this.imgServer, path);
        }

        #endregion

        public static SysSetting GetSetting()
        {
            return Nested.intance;
        }

        class Nested
        {
            internal readonly static SysSetting intance = new SysSetting();
        }
    }
}
