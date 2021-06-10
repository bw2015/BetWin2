using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Data;


using BW.GateWay.Planning;
using BW.Common.Reports;
using BW.Common.Sites;
using BW.Common.Admins;
using BW.Common.Users;
using BW.Common.Games;

using SP.Studio.Data;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Xml;

namespace BW.Agent
{
    partial class SiteAgent
    {
        /// <summary>
        /// 获取全部活动列表
        /// </summary>
        /// <returns></returns>
        public List<Planning> GetPlanList()
        {
            List<Planning> list = BDC.Planning.Where(t => t.SiteID == SiteInfo.ID).ToList();
            foreach (XElement item in IPlan.setting.Elements("item"))
            {
                PlanType type = item.GetAttributeValue("PlanType").ToEnum<PlanType>();
                if (list.Exists(t => t.Type == type)) continue;

                list.Add(new Planning()
                {
                    Type = type
                });
            }
            return list;
        }

        /// <summary>
        /// 获取活动对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Planning GetPlanInfo(PlanType type)
        {
            return this.GetPlanInfo(SiteInfo.ID, type);
        }

        /// <summary>
        /// 获取活动对象（非web程序）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Planning GetPlanInfo(int siteId, PlanType type)
        {
            Planning plan = BDC.Planning.Where(t => t.SiteID == siteId && t.Type == type).FirstOrDefault();

            return plan ?? new Planning()
            {
                SiteID = SiteInfo.ID,
                Type = type
            };
        }

        /// <summary>
        /// 保存活动设置
        /// </summary>
        /// <param name="plan"></param>
        /// <returns></returns>
        public bool SavePlanInfo(Planning plan)
        {
            plan.SiteID = SiteInfo.ID;
            if (plan.ID == 0)
            {
                if (plan.Add())
                {
                    AdminInfo.Log(AdminLog.LogType.Info, "添加活动设置,{0}", plan.PlanSetting.ToString());
                    return true;
                }
            }
            else
            {
                if (plan.Update() != 0)
                {
                    AdminInfo.Log(AdminLog.LogType.Info, "修改活动设置,{0}", plan.PlanSetting.ToString());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 活动执行的任务锁定对象
        /// </summary>
        private static object _planLockObject = new object();

        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns></returns>
        public void PlanRun()
        {
            lock (_planLockObject)
            {
                List<Planning> list = BDC.Planning.Where(t => t.IsOpen).ToList();

                foreach (Planning plan in list)
                {
                    try
                    {
                        this.PlanRun(plan.SiteID, plan.PlanSetting);
                    }
                    catch (Exception ex)
                    {
                        SystemAgent.Instance().AddErrorLog(plan.SiteID, ex, "执行活动任务失败");
                    }
                }
            }
        }

        /// <summary>
        /// 执行活动内容（10分钟执行一次）
        /// 在 SysSetting 定时器内调用
        /// </summary>
        /// <param name="siteId">所属站点</param>
        /// <param name="plan">活动设置</param>
        /// <returns></returns>
        public bool PlanRun(int siteId, IPlan plan)
        {

            bool success = false;
            switch (plan.Type)
            {
                case PlanType.VideoGameAgent:
                    success = this._videoGameAgent(siteId, plan);
                    break;
                case PlanType.VideoGame:
                    success = this._videoGame(siteId, plan);
                    break;
                case PlanType.SlotGameAgent:
                    success = this._slotGameAgent(siteId, plan);
                    break;
                case PlanType.SlotGame:
                    success = this._slotGame(siteId, plan);
                    break;
                case PlanType.SportGameAgent:
                    success = this._sportGameAgent(siteId, plan);
                    break;
                case PlanType.SportGame:
                    success = this._sportGame(siteId, plan);
                    break;
                case PlanType.WagesAgent:
                    success = this._lotteryWages(siteId, plan);
                    break;
                case PlanType.LotteryConsumption:
                    success = this._lotteryConsumption(siteId, plan);
                    break;
                case PlanType.LotteryLossBrokerage:
                    success = this._lotteryLossBrokerage(siteId, plan);
                    break;
                case PlanType.LossWages:
                    success = this._lotteryLossWages(siteId, plan);
                    break;
                case PlanType.GameWages:
                    success = this._gameWages(siteId, plan);
                    break;
            }
            return success;
        }



        #region ========== 第三方游戏上级和自身返点  ==============

        /// <summary>
        /// 真人视讯上级返水
        /// </summary>
        /// <returns></returns>
        private bool _videoGameAgent(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 360 || now > 420)
            {
                base.Message("真人视讯代理返水未到执行时间");
                return false;
            }
            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }
            return this._runGameAgent(siteId, status, sourceId, plan, "[{0}]真人视讯下级返水");
        }

        /// <summary>
        /// 真人游戏会员反水
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _videoGame(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 360 || now > 420)
            {
                base.Message("真人视讯会员返水未到执行时间");
                return false;
            }
            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }

            decimal minMoney = plan.Value.Get(IPlan.KEY_MINMONEY, decimal.Zero);

            Dictionary<int, decimal> list = BDC.VideoLog.Where(t => t.SiteID == siteId && t.EndAt > DateTime.Now.Date.AddDays(-1) && t.EndAt < DateTime.Now.Date).GroupBy(t => t.UserID).Select(t => new
            {
                UserID = t.Key,
                Money = t.Sum(p => p.BetAmount)
            }).Where(t => t.Money >= minMoney).ToDictionary(t => t.UserID, t => t.Money);

            if (status == null)
                status = this.AddPlanStatus(siteId, plan.Type, sourceId, list.Count);

            return this._runUserRebate(list, status, plan, "{0}真人游戏消费{1}元返水");
        }

        /// <summary>
        /// 电子游戏的上级返水
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _slotGameAgent(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 360 || now > 420)
            {
                base.Message("电子游戏返水未到执行时间");
                return false;
            }
            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }
            return this._runGameAgent(siteId, status, sourceId, plan, "[{0}]电子游戏下级返水");
        }


        /// <summary>
        /// 电子游戏会员反水
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _slotGame(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 30 || now > 90)
            {
                base.Message("电子游戏会员返水未到执行时间");
                return false;
            }
            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }

            decimal minMoney = plan.Value.Get(IPlan.KEY_MINMONEY, decimal.Zero);

            Dictionary<int, decimal> list = BDC.SlotLog.Where(t => t.SiteID == siteId && t.PlayAt > DateTime.Now.Date.AddDays(-1) && t.PlayAt < DateTime.Now.Date).GroupBy(t => t.UserID).Select(t => new
            {
                UserID = t.Key,
                Money = t.Sum(p => p.BetAmount)
            }).Where(t => t.Money >= minMoney).ToDictionary(t => t.UserID, t => t.Money);

            if (status == null)
                status = this.AddPlanStatus(siteId, plan.Type, sourceId, list.Count);

            return this._runUserRebate(list, status, plan, "{0}电子游戏消费{1}元返水");
        }

        /// <summary>
        /// 体育游戏的上级返水
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _sportGameAgent(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 360 || now > 420)
            {
                base.Message("体育游戏返水未到执行时间");
                return false;
            }
            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }
            return this._runGameAgent(siteId, status, sourceId, plan, "[{0}]体育游戏下级返水");
        }

        /// <summary>
        /// 体育游戏的会员返水
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _sportGame(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 360 || now > 420)
            {
                base.Message("体育游戏会员返水未到执行时间");
                return false;
            }
            int sourceId = int.Parse(DateTime.Now.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }

            decimal minMoney = plan.Value.Get(IPlan.KEY_MINMONEY, decimal.Zero);

            Dictionary<int, decimal> list = BDC.SportLog.Where(t => t.SiteID == siteId && t.ResultAt > DateTime.Now.Date.AddDays(-1) && t.ResultAt < DateTime.Now.Date).GroupBy(t => t.UserID).Select(t => new
            {
                UserID = t.Key,
                Money = t.Sum(p => p.BetMoney)
            }).Where(t => t.Money >= minMoney).ToDictionary(t => t.UserID, t => t.Money);

            if (status == null)
                status = this.AddPlanStatus(siteId, plan.Type, sourceId, list.Count);

            return this._runUserRebate(list, status, plan, "{0}体育游戏消费{1}元返水");
        }

        #endregion

        #region =========== 活动执行实现方法  ===========

        /// <summary>
        /// 运行投注上级代理的级差返点
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        internal bool _lotteryBetAgent(DbExecutor db, int orderId)
        {
            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_LotteryBetAgent",
                NewParam("@OrderID", orderId));
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return false;

            decimal userMoney = decimal.Zero;
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                int userId = (int)dr["UserID"];
                decimal money = (decimal)dr["Money"];
                if (money >= userMoney)
                {
                    UserAgent.Instance().AddMoneyLog(db, userId, money - userMoney, Common.Users.MoneyLog.MoneyType.BetAgent, orderId, string.Format("下级投注返点,投注订单号：{0}", orderId));
                }
                userMoney = money;
            }
            return true;
        }

        /// <summary>
        /// 运行上级返水
        /// </summary>
        /// <param name="list">有流水的用户列表</param>
        /// <param name="description">必须包含 {0}:日期</param>
        /// <returns></returns>
        private bool _runGameAgent(int siteId, PlanStatus status, int sourceId, IPlan plan, string description = "【{0}】下级返水")
        {
            Dictionary<int, decimal> list = new Dictionary<int, decimal>();
            using (DbExecutor db = NewExecutor())
            {
                decimal minMoney = plan.Value.ContainsKey(IPlan.KEY_MINMONEY) ? plan.Value[IPlan.KEY_MINMONEY] : decimal.Zero;
                decimal agent1 = plan.Value.ContainsKey("Agent1") ? plan.Value["Agent1"] : decimal.Zero;
                decimal agent2 = plan.Value.ContainsKey("Agent2") ? plan.Value["Agent2"] : decimal.Zero;
                decimal agent3 = plan.Value.ContainsKey("Agent3") ? plan.Value["Agent3"] : decimal.Zero;

                string procName = "plan_" + plan.Type;
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, procName,
                    NewParam("@SiteID", siteId),
                    NewParam("@Date", DateTime.Now.AddDays(-1).Date),
                    NewParam("@MinMoney", minMoney),
                    NewParam("@Agent1", agent1),
                    NewParam("@Agent2", agent2),
                    NewParam("@Agent3", agent3));
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add((int)dr["UserID"], (decimal)dr["Money"]);
                }
            }
            if (status == null) status = this.AddPlanStatus(siteId, plan.Type, sourceId, list.Count);

            foreach (KeyValuePair<int, decimal> agentItem in list)
            {
                int userId = agentItem.Key;
                decimal money = agentItem.Value;

                bool success = false;
                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, userId, plan.MoneyType, sourceId))
                    {
                        if (UserAgent.Instance().AddMoneyLog(db, userId, money, plan.MoneyType, sourceId,
                            string.Format(description, DateTime.Now.AddDays(-1).ToLongDateString())))
                        {
                            success = true;
                        }
                    }
                    db.Commit();
                }

                if (success)
                    this.UpdatePlanStatus(status);
            }

            return true;
        }

        /// <summary>
        /// 运行会员反水
        /// </summary>
        /// <param name="list"></param>
        /// <param name="status"></param>
        /// <param name="plan"></param>
        /// <param name="description">必须包含：{0}：日期   {1}：消费金额</param>
        /// <returns></returns>
        private bool _runUserRebate(Dictionary<int, decimal> list, PlanStatus status, IPlan plan, string description = "{0}消费{1}元反水")
        {
            decimal rate = plan.Value.Get("Rate", decimal.Zero);

            int siteId = status.SiteID;
            int sourceId = status.SourceID;

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                foreach (KeyValuePair<int, decimal> item in list)
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, item.Key, plan.MoneyType, sourceId))
                    {
                        UserAgent.Instance().AddMoneyLog(db, item.Key, item.Value * rate, plan.MoneyType, sourceId,
                            string.Format(description, DateTime.Now.AddDays(-1).ToLongDateString(), item.Value.ToString("n")));

                        this.UpdatePlanStatus(status);
                    }
                }
                db.Commit();
            }
            return true;
        }

        /// <summary>
        /// 总代的日工资
        /// </summary>
        /// <param name="siteId">站点参数</param>
        /// <param name="plan">活动的参数设定</param>
        /// <returns></returns>
        internal bool _lotteryWages(int siteId, IPlan plan, bool isManage = false)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (!isManage && (now < 120 || now > 180))
            {
                base.Message("未到日工资发放时间");
                return false;
            }

            int sourceId = int.Parse(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (!isManage && status != null && status.Total == status.Count)
            {
                this._lotteryWagesByContract(siteId, DateTime.Now, plan);
                base.Message("任务已经运行");
                return false;
            }

            int period = (int)plan.Value["Period"];
            if (period < 1)
            {
                base.Message("周期设置错误");
                return false;
            }

            if (sourceId % period != 0)
            {
                base.Message("未到工资执行周期");
                return false;
            }
            // 转换参数设定，(流水要求,日工资金额)
            SortedDictionary<decimal, decimal> setting = new SortedDictionary<decimal, decimal>();
            Regex regex = new Regex(@"Money(?<ID>\d+)");
            foreach (KeyValuePair<string, decimal> item in plan.Value)
            {
                if (!regex.IsMatch(item.Key)) continue;
                string id = regex.Match(item.Key).Groups["ID"].Value;
                string key = "Agent" + id;
                if (plan.Value.ContainsKey(key))
                {
                    if (!setting.ContainsKey(item.Value))
                        setting.Add(item.Value, plan.Value[key]);
                }
            }

            // 获取可以得到日工资的总代列表（用户ID，工资金额）
            Dictionary<int, decimal> userlist = new Dictionary<int, decimal>();
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_Wages",
                    NewParam("@SiteID", siteId),
                    NewParam("@Date", DateTime.Now.Date));

                if (status == null)
                    status = this.AddPlanStatus(siteId, plan.Type, sourceId, ds.Tables[0].Rows.Count);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //总代ID
                    int userId = (int)dr["UserID"];
                    //当日流水
                    decimal money = (decimal)dr["Money"];

                    decimal reward = decimal.Zero;
                    foreach (KeyValuePair<decimal, decimal> item in setting.Where(t => t.Key <= money))
                    {
                        decimal rewardMoney = item.Value;

                        if (rewardMoney < decimal.One)
                        {
                            rewardMoney = money * item.Value;
                        }

                        if (rewardMoney > reward) reward = rewardMoney;
                    }

                    if (reward == decimal.Zero) continue;

                    userlist.Add(userId, reward);
                }
            }

            // 发放日工资
            foreach (KeyValuePair<int, decimal> item in userlist)
            {
                bool success = false;

                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, item.Key, plan.MoneyType, sourceId))
                    {
                        if (UserAgent.Instance().AddMoneyLog(db, item.Key, item.Value, plan.MoneyType, sourceId,
                            string.Format("{0} 日工资", DateTime.Now.AddDays(-1).ToLongDateString())))
                        {
                            success = true;
                        }
                    }
                    else
                    {
                        success = false;
                    }
                    db.Commit();
                }

                if (success)
                    this.UpdatePlanStatus(status);
            }

            this._lotteryWagesByContract(siteId, DateTime.Now, plan);

            return true;
        }

        /// <summary>
        /// 契约工资发放
        /// </summary>
        private void _lotteryWagesByContract(int siteId, DateTime date, IPlan plan)
        {
            List<Contract> list;
            using (DbExecutor db = NewExecutor())
            {
                list = db.GetDataSet(CommandType.StoredProcedure, "plan_ContractList",
                    NewParam("@SiteID", siteId),
                    NewParam("@Type", Contract.ContractType.WagesAgent)).ToList<Contract>();
            }
            int sourceId = int.Parse(date.AddDays(-1).ToString("yyyyMMdd"));
            PlanStatus status = this.GetPlanStatus(siteId, PlanType.WagesContract, sourceId);

            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return;
            }

            if (status == null) status = this.AddPlanStatus(siteId, PlanType.WagesContract, sourceId, list.Count);

            list.ForEach(t =>
            {
                this._lotteryWagesByContract(t, date, sourceId, plan);
            });

            try
            {
                int success = UserAgent.Instance().ExecContractLog(siteId, Contract.ContractType.WagesAgent, sourceId);
                for (int i = 0; i < success; i++)
                {
                    this.UpdatePlanStatus(status);
                }
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(siteId, ex, "发放契约工资失败");
            }
        }

        /// <summary>
        /// 运行契约工资的计算（只生成待处理列表）
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private bool _lotteryWagesByContract(Contract contract, DateTime date, int sourceId, IPlan plan)
        {
            //#1 获取默认的工资配置
            SortedDictionary<decimal, string> setting = new SortedDictionary<decimal, string>();
            Regex regex = new Regex(@"Money(?<ID>\d+)");
            foreach (KeyValuePair<string, decimal> item in plan.Value)
            {
                if (!regex.IsMatch(item.Key)) continue;
                string id = regex.Match(item.Key).Groups["ID"].Value;
                string key = "Agent" + id;
                if (plan.Value.ContainsKey(key))
                {
                    if (!setting.ContainsKey(item.Value))
                        setting.Add(item.Value, key);
                }
            }

            //#2 如果甲方等于乙方则返回
            if (contract.User1 == contract.User2) return false;

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                //#3.1 获取乙方的团队流水总额
                decimal money = (decimal)db.ExecuteScalar(CommandType.StoredProcedure, "plan_WagesUser",
                       NewParam("@SiteID", contract.SiteID),
                       NewParam("@Date", date.Date),
                       NewParam("@UserID", contract.User2));

                // #3.2 获取契约配置内容
                Dictionary<string, decimal> dic = contract.Setting.Where(t => !string.IsNullOrEmpty(t.Key)).ToDictionary(t => t.Key, t => t.MaxValue);

                // #3.3 获取日工资比例
                decimal reward = decimal.Zero;
                foreach (KeyValuePair<decimal, string> item in setting.Where(t => t.Key <= money))
                {
                    decimal rewardMoney = dic.ContainsKey(item.Value) ? dic[item.Value] : decimal.Zero;
                    if (rewardMoney == decimal.Zero) continue;

                    if (rewardMoney < decimal.One)
                    {
                        rewardMoney = money * rewardMoney;
                    }

                    if (rewardMoney > reward) reward = rewardMoney;
                }

                // #3.4 添加契约转账日志（待转账）
                UserAgent.Instance().AddContractLog(db, contract, reward, sourceId, string.Format("流水{0}元", money.ToString("n")), money);

                db.Commit();

                return true;
            }
        }

        /// <summary>
        /// 彩票的日消费奖励
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _lotteryConsumption(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 30 || now > 90)
            {
                base.Message("未到日消费返点时间");
                return false;
            }

            DateTime date = DateTime.Now.Date.AddDays(-1);

            int sourceId = int.Parse(date.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }

            // 转换参数设定，(流水要求,日工资金额)
            SortedDictionary<decimal, decimal> setting = new SortedDictionary<decimal, decimal>();
            Regex regex = new Regex(@"Money(?<ID>\d+)");
            foreach (KeyValuePair<string, decimal> item in plan.Value)
            {
                if (!regex.IsMatch(item.Key) || item.Value == decimal.Zero) continue;
                string id = regex.Match(item.Key).Groups["ID"].Value;
                string key = "Reward" + id;
                if (plan.Value.ContainsKey(key))
                {
                    if (!setting.ContainsKey(item.Value))
                        setting.Add(item.Value, plan.Value[key]);
                }
            }

            if (setting.Count == 0) return false;

            // 获取可以得到消费返点的用户列表
            Dictionary<int, decimal> userlist = new Dictionary<int, decimal>();
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_LotteryConsumption",
                    NewParam("@SiteID", siteId),
                    NewParam("@Date", date),
                    NewParam("@MinMoney", setting.Min(t => t.Key)));

                if (status == null)
                    status = this.AddPlanStatus(siteId, plan.Type, sourceId, ds.Tables[0].Rows.Count);

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int userId = (int)dr["UserID"];
                    // 投注量
                    decimal money = (decimal)dr["Money"];

                    decimal reward = decimal.Zero;
                    foreach (KeyValuePair<decimal, decimal> item in setting.Where(t => t.Key <= money))
                    {
                        if (item.Value > reward) reward = item.Value;
                    }

                    if (reward == decimal.Zero) continue;

                    userlist.Add(userId, reward);
                }
            }

            // 发放消费奖励
            foreach (KeyValuePair<int, decimal> item in userlist)
            {
                bool success = false;

                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, item.Key, plan.MoneyType, sourceId))
                    {
                        if (UserAgent.Instance().AddMoneyLog(db, item.Key, item.Value, plan.MoneyType, sourceId,
                            string.Format("{0} 消费奖励", date.ToLongDateString())))
                        {
                            success = true;
                        }
                    }

                    db.Commit();
                }

                if (success)
                {
                    UserAgent.Instance().AddNotify(item.Key, UserNotify.NotifyType.Plan, string.Format("{0}消费奖励{1}元已经发放到您的账户", date.ToLongDateString(), item.Value.ToString("n")));
                    this.UpdatePlanStatus(status);
                }
            }

            return true;
        }

        /// <summary>
        /// 执行首次充值奖励
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="money">充值金额</param>
        /// <returns></returns>
        internal bool _firstRecharge(int userId, decimal money)
        {
            int siteId = UserAgent.Instance().GetSiteID(userId);
            //#1 获取活动设置
            Planning planning = BDC.Planning.Where(t => t.SiteID == siteId && t.IsOpen && t.Type == PlanType.FirstRecharge).FirstOrDefault();
            if (planning == null) { return false; }

            IPlan plan = planning.PlanSetting;

            decimal reward = decimal.Zero;
            foreach (KeyValuePair<string, decimal> item in plan.Value.Where(t => t.Key.StartsWith("Money")))
            {
                if (money >= item.Value)
                {
                    decimal _reward = plan.Value[item.Key.Replace("Money", "Reward")];
                    if (reward < _reward) reward = _reward;
                }
            }

            // 没有达到奖励条件
            if (reward == decimal.Zero) return false;

            //#2 判断是否是首充
            if (BDC.RechargeOrder.Where(t => t.SiteID == siteId && t.UserID == userId && t.IsPayment).Count() != 1)
            {
                return false;
            }

            //#3 发放奖励
            if (!UserAgent.Instance().ExistsMoneyLog(userId, plan.MoneyType, 0))
            {
                return UserAgent.Instance().AddMoneyLog(userId, reward, plan.MoneyType, 0, "首充奖励");
            }

            // 已经发放过了
            return false;
        }

        /// <summary>
        /// 绑定银行卡赠送
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal bool _bankAccount(int userId)
        {
            int siteId = UserAgent.Instance().GetSiteID(userId);
            //#1 获取活动设置
            Planning planning = BDC.Planning.Where(t => t.SiteID == siteId && t.IsOpen && t.Type == PlanType.BankAccount).FirstOrDefault();
            if (planning == null) { return false; }

            IPlan plan = planning.PlanSetting;
            if (plan.Value["Reward"] == decimal.Zero) return false;

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_BankAccount",
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId),
                    NewParam("@MoneyType", plan.MoneyType),
                    NewParam("@Check", plan.Value["Check"] != decimal.Zero));

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return false;

                if (!UserAgent.Instance().ExistsMoneyLog(db, userId, plan.MoneyType, 0))
                {
                    if (!UserAgent.Instance().AddMoneyLog(db, userId, plan.Value["Reward"], plan.MoneyType, 0, "绑定银行卡赠送"))
                    {
                        db.Rollback();
                        return false;
                    }
                    db.Commit();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 单日亏损上级的返佣
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        internal bool _lotteryLossBrokerage(int siteId, IPlan plan)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (now < 0 || now > 240)
            {
                base.Message("未到单日亏损上级的返佣时间");
                return false;
            }

            int sourceId = int.Parse(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return false;
            }

            // 转换参数设定，(亏损金额,上三级的奖励金额)
            SortedDictionary<decimal, decimal[]> setting = new SortedDictionary<decimal, decimal[]>();
            Regex regex = new Regex(@"Level(?<ID>\d+)");
            foreach (KeyValuePair<string, decimal> item in plan.Value)
            {
                if (!regex.IsMatch(item.Key) || item.Value == decimal.Zero) continue;
                string id = regex.Match(item.Key).Groups["ID"].Value;
                List<decimal> agentMoney = new List<decimal>();

                for (int level = 1; level <= 3; level++)
                {
                    string key = string.Format("Agent{0}{1}", id, level);
                    agentMoney.Add(plan.Value.ContainsKey(key) ? plan.Value[key] : decimal.Zero);
                }

                setting.Add(item.Value, agentMoney.ToArray());
            }

            if (setting.Count == 0) return false;

            DateTime startAt = DateTime.Now.Date.AddDays(-1);
            DateTime endAt = DateTime.Now.Date;

            var list = BDC.UserDateMoney.Where(t => t.SiteID == siteId && t.Date == startAt.Date).GroupBy(t => new
            {
                t.UserID,
                t.Type
            }).Select(t => new
            {
                t.Key.UserID,
                t.Key.Type,
                Money = t.Sum(p => p.Money)
            }).ToArray();

            IEnumerable<UserReport> report = list.GroupBy(t => t.UserID).Select(t =>
            {
                Dictionary<MoneyLog.MoneyType, decimal> data = list.Where(p => p.UserID == t.Key).ToDictionary(p => p.Type, p => p.Money);
                return new UserReport(t.Key, data, null);
            }).Where(t => Math.Abs(t.Money) >= setting.Min(p => p.Key));

            if (status == null)
                status = this.AddPlanStatus(siteId, plan.Type, sourceId, report.Count());

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                foreach (UserReport user in report.Where(t => t.Money < decimal.Zero))
                {
                    bool success = false;
                    decimal[] reward = setting.Where(t => t.Key <= Math.Abs(user.Money)).LastOrDefault().Value;
                    int[] pagentList = UserAgent.Instance().GetUserParentList(user.UserID).ToArray();

                    for (int index = 0; index < reward.Length; index++)
                    {
                        if (reward[index] == decimal.Zero) continue;
                        if (pagentList.Length <= index) continue;
                        int parentId = pagentList[index];

                        if (!UserAgent.Instance().ExistsMoneyLog(db, parentId, plan.MoneyType, sourceId))
                        {
                            if (UserAgent.Instance().AddMoneyLog(db, parentId, reward[index], plan.MoneyType, sourceId,
                                  string.Format("下级[{0}]在{1}亏损{2}元奖励",
                                      UserAgent.Instance().GetUserName(user.UserID),
                                      DateTime.Now.AddDays(-1).ToLongDateString(),
                                      Math.Abs(user.Money).ToString("n")
                                  )))
                            {
                                success = true;
                            }
                        }
                    }
                    if (success)
                        this.UpdatePlanStatus(status);
                }

                db.Commit();
            }
            return true;
        }



        #endregion

        #region =========  契约分红    =================

        /// <summary>
        /// 获取可以得到分红金额
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="money">亏损金额</param>
        /// <param name="moneyType">要求的业绩类型</param>
        /// <param name="betMoney">销售额（下级投注总额）</param>
        /// <returns></returns>
        public decimal GetBounsMoney(int userId, decimal money, int moneyType, decimal betMoney = decimal.Zero)
        {
            if (money > 0) return decimal.Zero;
            money = Math.Abs(money);
            User user = UserAgent.Instance().GetUserInfo(userId);
            Contract contract = UserAgent.Instance().GetContractInfo(Contract.ContractType.Bouns, userId, user.AgentID);
            if (contract == null) return decimal.Zero;

            IPlan plan = this.GetPlanInfo(PlanType.Bonus).PlanSetting;

            SortedDictionary<decimal, string> setting = new SortedDictionary<decimal, string>();

            Regex regex = new Regex(@"Money(?<ID>\d+)");
            foreach (KeyValuePair<string, decimal> item in plan.Value)
            {
                if (!regex.IsMatch(item.Key)) continue;
                string id = regex.Match(item.Key).Groups["ID"].Value;
                string key = "Agent" + id;
                if (plan.Value.ContainsKey(key))
                {
                    if (!setting.ContainsKey(item.Value))
                        setting.Add(item.Value, key);
                }
            }

            // #3.2 获取契约配置内容
            Dictionary<string, decimal> dic = contract.Setting.Where(t => !string.IsNullOrEmpty(t.Key)).ToDictionary(t => t.Key, t => t.MaxValue);

            decimal bouns = decimal.Zero;
            decimal outstanding = money;
            switch (moneyType)
            {
                case 1:
                    // 销售业绩
                    outstanding = betMoney;
                    break;
            }
            foreach (KeyValuePair<decimal, string> item in setting.Where(t => t.Key <= outstanding))
            {
                // 分红比例
                decimal rewardMoney = dic.ContainsKey(item.Value) ? dic[item.Value] : decimal.Zero;
                if (rewardMoney == decimal.Zero) continue;
                bouns = money * rewardMoney;
            }

            return bouns;
        }


        /// <summary>
        /// 执行总代分红
        /// </summary>
        /// <param name="startAt">本次的开始时间</param>
        /// <param name="endAt">比选择的时间多了一天</param>
        /// <returns></returns>
        public bool BounsRun(int siteId, DateTime startAt, DateTime endAt)
        {
            if (startAt >= endAt)
            {
                base.Message("日期范围错误");
                return false;
            }

            if (endAt > DateTime.Now)
            {
                base.Message("结束时间大于当前时间");
                return false;
            }

            int startSourceId = int.Parse(startAt.ToString("yyyyMMdd")) * 100;
            int sourceId = startSourceId + (int)(endAt - startAt).TotalDays;

            PlanStatus status = this.GetPlanStatus(siteId, PlanType.Bonus, sourceId);
            List<PlanBouns> list = this.GetPlanBouns(startAt, endAt);
            if (status == null)
            {
                int? recrodSourceId = BDC.PlanStatus.Where(t => t.SiteID == siteId && t.Type == PlanType.Bonus).OrderByDescending(t => t.SourceID).Select(t => (int?)t.SourceID).FirstOrDefault();
                if (recrodSourceId != null)
                {
                    DateTime start, end;
                    if (!this.getDateTime(recrodSourceId.Value, out start, out end)) return false;
                    if (startAt < end)
                    {
                        base.Message("存在该日期区间内的分红记录，请勿重复发放。上次发放的时间范围：{0}～{1}", start.ToLongDateString(), end.ToLongDateString());
                        return false;
                    }
                }
                status = this.AddPlanStatus(siteId, PlanType.Bonus, sourceId, list.Count);
            }

            Planning plan = this.GetPlanInfo(PlanType.Bonus);
            foreach (PlanBouns item in list)
            {
                bool success = false;
                decimal bouns = item.GetBouns(plan.PlanSetting.Value);
                // 发放总代分红奖金
                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, item.UserID, MoneyLog.MoneyType.TopLossAgent, sourceId))
                    {
                        UserAgent.Instance().AddMoneyLog(db, item.UserID, bouns, MoneyLog.MoneyType.TopLossAgent, sourceId,
                            string.Format("{0}～{1}分红，{2}", startAt.ToLongDateString(), endAt.AddDays(-1).ToLongDateString(), item.ToString()));

                        db.Commit();
                        success = true;
                    }
                    else
                    {
                        db.Rollback();
                    }
                }

                if (success)
                    this.UpdatePlanStatus(status);
            }

            this.createBounsContract(siteId, sourceId);

            return true;
        }


        /// <summary>
        /// 执行契约分红（只生成待转账列表）
        /// </summary>
        /// <param name="moneyType">业绩类型</param>
        private void createBounsContract(int siteId, int sourceId)
        {
            // 获取签订了契约转账的用户列表
            List<Contract> list;
            using (DbExecutor db = NewExecutor())
            {
                list = db.GetDataSet(CommandType.StoredProcedure, "plan_ContractList",
                    NewParam("@SiteID", siteId),
                    NewParam("@Type", Contract.ContractType.Bouns)).ToList<Contract>();
            }

            PlanStatus status = this.GetPlanStatus(siteId, PlanType.BonusContract, sourceId);
            if (status != null && status.Total == status.Count)
            {
                return;
            }
            DateTime start, end;
            if (!this.getDateTime(sourceId, out start, out end)) return;
            if (status == null) status = this.AddPlanStatus(siteId, PlanType.BonusContract, sourceId, list.Count);
            Planning plan = this.GetPlanInfo(PlanType.Bonus);

            list.ForEach(t =>
            {
                PlanBouns planBouns = this.GetPlanBouns(start, end, t.User2);
                decimal bouns = planBouns.GetBouns(plan.PlanSetting.Value, t.Data);

                using (DbExecutor db = NewExecutor())
                {
                    // #3.4 添加契约转账日志（待转账）
                    UserAgent.Instance().AddContractLog(db, t, bouns, sourceId, string.Format("【{0}～{1}】{2}",
                        start.ToLongDateString(), end.ToLongDateString(), planBouns.ToString()),
                        Math.Max(decimal.Zero, planBouns.Money));
                }
            });

            this.execBounsContract(siteId, sourceId, status);
        }

        /// <summary>
        /// 根据待转账列表开始发放契约分红
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="sourceId"></param>
        private void execBounsContract(int siteId, int sourceId, PlanStatus status)
        {
            int success = UserAgent.Instance().ExecContractLog(siteId, Contract.ContractType.Bouns, sourceId);
            for (int i = 0; i < success; i++)
            {
                this.UpdatePlanStatus(status);
            }
        }


        /// <summary>
        /// （工具方法）从来源编号中获取开始时间和结束时间
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private bool getDateTime(int sourceId, out DateTime start, out DateTime end)
        {
            string sourceTime = sourceId.ToString();
            Regex regex = new Regex(@"^(?<Year>\d{4})(?<Month>\d{2})(?<Day>\d{2})(?<Date>\d{2})$");

            if (!regex.IsMatch(sourceTime))
            {
                start = end = DateTime.MinValue;
                base.Message("数据发生错误，错误的数据内容：{0}", sourceTime);
                return false;
            }

            GroupCollection match = regex.Match(sourceTime).Groups;
            start = DateTime.Parse(match["Year"].Value + "-" + match["Month"].Value + "-" + match["Day"].Value);
            end = start.AddDays(int.Parse(match["Date"].Value));
            return true;
        }

        #endregion

        #region ===========  挂单工资   ==============

        /// <summary>
        /// 获取昨天的挂单流水
        /// </summary>
        /// <param name="siteId">所属站点</param>
        /// <param name="userId">上级（包括自己）</param>
        /// <param name="date">日期</param>
        /// <param name="member">人数</param>
        /// <returns></returns>
        private decimal GetLossMoney(int siteId, int userId, DateTime date, decimal memberMoney, out int member)
        {
            member = 0;
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_LossWages",
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId),
                    NewParam("@Date", date),
                    NewParam("@Member", memberMoney));
                if (ds.Tables[0].Rows.Count == 0) return decimal.Zero;
                member = (int)ds.Tables[0].Rows[0]["Member"];
                return (decimal)ds.Tables[0].Rows[0]["Money"];
            }
        }

        /// <summary>
        /// 运行挂单工资
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <param name="isManage">是否手动发放</param>
        /// <returns></returns>
        public bool _lotteryLossWages(int siteId, IPlan plan, bool isManage = false)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (!isManage && (now < 120 || now > 180))
            {
                base.Message("未到挂单工资发放时间");
                return false;
            }

            DateTime date = DateTime.Now.Date;

            int sourceId = int.Parse(date.AddDays(-1).ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (!isManage && status != null && status.Total == status.Count)
            {
                this._lossWagesContract(siteId, date, sourceId, plan);
                base.Message("任务已经运行");
                return false;
            }

            // 获取可以得到挂单工资的总代列表（用户ID，工资金额）
            Dictionary<int, decimal> userlist = new Dictionary<int, decimal>();
            Dictionary<int, string> userRemark = new Dictionary<int, string>();

            int[] users = BDC.User.Where(t => t.SiteID == siteId && t.AgentID == 0).Select(t => t.ID).ToArray();
            Regex regex = new Regex(@"Money(?<Level>\d)");
            decimal memberMoney = plan.Value["Member"];
            foreach (int userId in users)
            {
                int member;
                decimal lossMoney = this.GetLossMoney(siteId, userId, date, memberMoney, out member);   // 当前会员的挂单流水和有效金额
                decimal money = decimal.Zero;   //　可以得到的工资

                foreach (ItemSetting item in plan.SettingList)
                {
                    if (!regex.IsMatch(item.Key)) continue;
                    string level = regex.Match(item.Key).Groups["Level"].Value;
                    decimal itemMoney = item.Value; // 要求的流水金额
                    decimal itemMember = plan.Value["Member" + level];  // 要求的有效人数
                    decimal itemAgent = plan.Value["Agent" + level];    // 工资比例

                    if (itemMoney <= lossMoney && itemMember <= member)
                    {
                        if (itemAgent > decimal.One)
                        {
                            if (itemAgent > money) money = itemAgent;
                        }
                        else
                        {
                            if (lossMoney * itemAgent > money) money = lossMoney * itemAgent;
                        }
                    }
                }
                userlist.Add(userId, money);
                userRemark.Add(userId, string.Format("挂单流水：{0}，有效投注：{1}人", lossMoney.ToString("n"), member));
            }

            if (status == null) status = this.AddPlanStatus(siteId, plan.Type, sourceId, userlist.Count);

            foreach (KeyValuePair<int, decimal> user in userlist)
            {
                bool success = false;
                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, user.Key, MoneyLog.MoneyType.LossWagesAgent, sourceId))
                    {
                        success = UserAgent.Instance().AddMoneyLog(db, user.Key, user.Value, MoneyLog.MoneyType.LossWagesAgent, sourceId,
                            string.Format("[{0}]挂单工资，{1}", date.AddDays(-1).ToString("yyyy年M月d日"), userRemark.Get(user.Key, string.Empty)));
                    }
                    db.Commit();
                }
                if (success) this.UpdatePlanStatus(status);
            }

            this._lossWagesContract(siteId, date, sourceId, plan);
            return true;
        }

        /// <summary>
        /// 挂单工资契约发放
        /// </summary>
        /// <param name="siteId"></param>
        private void _lossWagesContract(int siteId, DateTime date, int sourceId, IPlan plan)
        {
            List<Contract> list;
            using (DbExecutor db = NewExecutor())
            {
                list = db.GetDataSet(CommandType.StoredProcedure, "plan_ContractList",
                    NewParam("@SiteID", siteId),
                    NewParam("@Type", Contract.ContractType.LossWages)).ToList<Contract>();
            }

            PlanStatus status = this.GetPlanStatus(siteId, PlanType.LossWagesContract, sourceId);

            if (status != null && status.Total == status.Count)
            {
                base.Message("任务已经运行");
                return;
            }

            if (status == null) status = this.AddPlanStatus(siteId, PlanType.LossWagesContract, sourceId, list.Count);
            decimal memberMoney = plan.Value["Member"];
            Regex regex = new Regex(@"Money(?<Level>\d)");
            foreach (Contract contract in list)
            {
                if (contract.User1 == contract.User2) continue;

                // #3.2 获取契约配置内容
                Dictionary<string, decimal> dic = contract.Setting.Where(t => !string.IsNullOrEmpty(t.Key)).ToDictionary(t => t.Key, t => t.MaxValue);
                int member;
                decimal lossMoney = this.GetLossMoney(siteId, contract.User2, date, memberMoney, out member);
                decimal money = decimal.Zero;
                foreach (ItemSetting item in plan.SettingList)
                {
                    if (!regex.IsMatch(item.Key)) continue;
                    string level = regex.Match(item.Key).Groups["Level"].Value;
                    string agentKey = "Agent" + level;
                    decimal itemMoney = item.Value; // 要求的流水金额
                    decimal itemMember = plan.Value["Member" + level];  // 要求的有效人数
                    decimal itemAgent = dic.ContainsKey(agentKey) ? dic[agentKey] : decimal.Zero;    // 工资比例

                    if (itemMoney <= lossMoney && itemMember <= member)
                    {
                        if (itemAgent > decimal.One)
                        {
                            if (itemMoney > money) money = itemAgent;
                        }
                        else
                        {
                            if (money * itemAgent > money) money = lossMoney * itemAgent;
                        }
                    }
                }

                // #3.4 添加契约转账日志（待转账）
                if (UserAgent.Instance().AddContractLog(contract, money, sourceId,
                    string.Format("[{0}]挂单流水{1}元，有效人数{2}人", date.AddDays(-1).ToString("yyyy年M月d日"), lossMoney.ToString("n"), member), lossMoney))
                {
                    this.UpdatePlanStatus(status);
                }
            }

            try
            {
                int success = UserAgent.Instance().ExecContractLog(siteId, Contract.ContractType.WagesAgent, sourceId);
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(siteId, ex, "发放契约工资失败");
            }
        }

        #endregion

        #region ========== 第三方游戏工资 ==========

        /// <summary>
        /// 第三方日工资
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private bool _gameWages(int siteId, IPlan plan, bool isManage = false)
        {
            int now = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            if (!isManage && (now < 360 || now > 420))
            {
                base.Message("未到游戏工资发放时间");
                return false;
            }
            //  总代列表
            int[] users = BDC.User.Where(t => t.SiteID == siteId && t.AgentID == 0).Select(t => t.ID).ToArray();
            DateTime date = DateTime.Now.AddDays(-1).Date;
            int sourceId = int.Parse(date.ToString("yyyyMMdd"));

            PlanStatus status = this.GetPlanStatus(siteId, plan.Type, sourceId);
            if (status != null && status.Total == status.Count)
            {
                this._runGameWagesContract(siteId, date);
                return false;
            }
            if (status == null) status = this.AddPlanStatus(siteId, plan.Type, sourceId, users.Length);
            foreach (int userId in users)
            {
                string description;
                decimal amount;
                decimal money = this._getGameTeamReport(siteId, userId, date, plan.Value, out amount, out description);
                bool success = false;
                using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
                {
                    if (!UserAgent.Instance().ExistsMoneyLog(db, userId, plan.MoneyType, sourceId))
                    {
                        success = UserAgent.Instance().AddMoneyLog(db, userId, money, plan.MoneyType, sourceId, description);
                    }
                    db.Commit();
                }
                if (success) this.UpdatePlanStatus(status);
            }

            this._runGameWagesContract(siteId, date);

            return true;
        }

        /// <summary>
        /// 运行第三方游戏契约
        /// </summary>
        private void _runGameWagesContract(int siteId, DateTime date)
        {
            List<Contract> list;
            using (DbExecutor db = NewExecutor())
            {
                list = db.GetDataSet(CommandType.StoredProcedure, "plan_ContractList",
                    NewParam("@SiteID", siteId),
                    NewParam("@Type", Contract.ContractType.GameWages)).ToList<Contract>();
            }
            if (list.Count == 0) return;
            int sourceId = int.Parse(date.ToString("yyyyMMdd"));
            PlanStatus status = this.GetPlanStatus(siteId, PlanType.GameWagesContract, sourceId);
            if (status != null && status.Count == status.Total) return;
            if (status == null) status = this.AddPlanStatus(siteId, PlanType.GameWagesContract, sourceId, list.Count);

            foreach (Contract contract in list)
            {
                if (contract.User1 == contract.User2) continue;

                Dictionary<string, decimal> dic = contract.Setting.Where(t => !string.IsNullOrEmpty(t.Key)).ToDictionary(t => t.Key, t => t.MaxValue);

                string description;
                decimal amount;
                decimal money = this._getGameTeamReport(siteId, contract.User2, date, dic, out amount, out description);
                if (UserAgent.Instance().AddContractLog(contract, money, sourceId, description, amount))
                {
                    this.UpdatePlanStatus(status);
                }
            }

            try
            {
                int success = UserAgent.Instance().ExecContractLog(siteId, Contract.ContractType.GameWages, sourceId);
            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(siteId, ex, "发放游戏契约工资失败");
            }
        }

        /// <summary>
        /// 获取用户团队的第三方游戏业绩
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private Dictionary<GameType, decimal> _getGameTeamReport(int siteId, int userId, DateTime date)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_TeamGameByDate",
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId),
                    NewParam("@Date", date.Date));
                Dictionary<GameType, decimal> dic = new Dictionary<GameType, decimal>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dic.Add((GameType)dr["Type"], (decimal)dr["Amount"]);
                }
                return dic;
            }
        }

        /// <summary>
        /// 获取指定日期用户的游戏工资总额
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dic"></param>
        /// <param name="desc"></param>
        /// <param name="amount">流水总额</param>
        /// <returns></returns>
        private decimal _getGameTeamReport(int siteId, int userId, DateTime date, Dictionary<string, decimal> dic, out decimal amount, out string desc)
        {
            Dictionary<GameType, decimal> data = this._getGameTeamReport(siteId, userId, date);
            decimal money = amount = decimal.Zero;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("【{0}】", date.ToLongDateString()).AppendLine();
            foreach (KeyValuePair<GameType, decimal> item in data)
            {
                string type = item.Key.ToString();
                if (dic.ContainsKey(type))
                {
                    amount += item.Value;
                    money += dic[type] * item.Value;
                    sb.AppendLine(string.Format("{0}流水{1}元", item.Key.GetDescription(), item.Value.ToString("n")));
                }
            }
            desc = sb.ToString();
            return money;
        }

        /// <summary>
        /// 获取用户所在团队的第三方游戏投注总额
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private Dictionary<GameType, decimal> _getTeamGame(int siteId, int userId, DateTime date)
        {
            Dictionary<GameType, decimal> dic = new Dictionary<GameType, decimal>();
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_TeamGameByDate",
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId),
                    NewParam("@Date", date));
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dic.Add((GameType)dr["Type"], (decimal)dr["Amount"]);
                }
            }
            return dic;
        }


        #endregion

        #region ========= 单层分红 ===========

        /// <summary>
        /// 获取单层分红的业绩统计信息
        /// </summary>
        /// <param name="endAt">统计的截至时间（比统计时间要多1天）</param>
        /// <returns></returns>
        public List<PlanSingleBouns> GetPlanSingleBouns(DateTime startAt, DateTime endAt)
        {
            Planning plan = this.GetPlanInfo(PlanType.SingleBonus);
            if (!plan.IsOpen) { base.Message("未开启活动"); return null; }

            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_SingleBouns",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt));
                List<PlanSingleBouns> list = new List<PlanSingleBouns>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new PlanSingleBouns(dr));
                }
                return list;
            }
        }

        /// <summary>
        /// 获取会员活动中设定的总代分红业绩列表
        /// </summary>
        /// <param name="startAt"></param>
        /// <param name="endAt">统计的截至时间（比统计时间要多1天）</param>
        /// <returns></returns>
        public List<PlanBouns> GetPlanBouns(DateTime startAt, DateTime endAt)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_Bonus",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt));
                List<PlanBouns> list = new List<PlanBouns>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new PlanBouns(dr));
                }
                return list;
            }
        }

        /// <summary>
        /// 获取单个用户的团队数据
        /// </summary>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public PlanBouns GetPlanBouns(DateTime startAt, DateTime endAt, int userId)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "plan_Bonus",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt),
                    NewParam("@UserID", userId));
                if (ds.Tables[0].Rows.Count == 0) return default(PlanBouns);
                return new PlanBouns(ds.Tables[0].Rows[0]);
            }
        }

        #endregion

        /// <summary>
        /// 获取活动任务运行状态
        /// </summary>
        private PlanStatus GetPlanStatus(int siteId, PlanType type, int sourceId)
        {
            using (DbExecutor db = NewExecutor())
            {
                return new PlanStatus()
                {
                    SiteID = siteId,
                    Type = type,
                    SourceID = sourceId
                }.Info(db);
            }
        }

        /// <summary>
        /// 创建一个任务（不判断重复）
        /// </summary>
        private PlanStatus AddPlanStatus(int siteId, PlanType type, int sourceId, int total)
        {
            using (DbExecutor db = NewExecutor())
            {
                PlanStatus status = new PlanStatus()
                {
                    SiteID = siteId,
                    Type = type,
                    SourceID = sourceId,
                    Total = total,
                    CreateAt = DateTime.Now
                };
                status.Add(db);
                return status;
            }
        }

        /// <summary>
        /// 更新任务进度（自加1）
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool UpdatePlanStatus(PlanStatus status)
        {
            using (DbExecutor db = NewExecutor())
            {
                status.Count++;
                status.EndAt = DateTime.Now;
                return status.Update(db, t => t.Count, t => t.EndAt) != 0;
            }
        }
    }
}
