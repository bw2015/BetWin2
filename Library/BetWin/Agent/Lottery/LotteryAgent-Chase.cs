using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Json;
using SP.Studio.Data;

using BW.Common.Lottery;
using BW.Common.Users;

namespace BW.Agent
{
    /// <summary>
    /// 追号管理
    /// </summary>
    partial class LotteryAgent
    {
        /// <summary>
        /// 获取追号详情
        /// </summary>
        /// <param name="chaseId"></param>
        /// <returns></returns>
        public LotteryChase GetLotteryChaseInfo(int chaseId)
        {
            return BDC.LotteryChase.Where(t => t.SiteID == SiteInfo.ID && t.ID == chaseId).FirstOrDefault();
        }

        /// <summary>
        /// 获取追号的期号详情
        /// </summary>
        /// <param name="chaseId"></param>
        /// <returns></returns>
        public List<LotteryChaseItem> GetLotteryChaseItemList(int chaseId)
        {
            return BDC.LotteryChaseItem.Where(t => t.SiteID == SiteInfo.ID && t.ChaseID == chaseId).OrderBy(t => t.StartAt).ToList();
        }

        /// <summary>
        /// 撤单的锁
        /// </summary>
        private const string _lock_RevokeLotteryChase = "_lock_RevokeLotteryChase";
        /// <summary>
        /// 会员请求撤销追号订单
        /// </summary>
        /// <param name="chaseId"></param>
        /// <returns></returns>
        public bool RevokeLotteryChase(int userId, int chaseId)
        {
            lock (_lock_RevokeLotteryChase)
            {
                LotteryChase chase = this.GetLotteryChaseInfo(chaseId);

                if (chase == null || (userId != 0 && chase.UserID != userId))
                {
                    base.Message("编号错误");
                    return false;
                }

                if (chase.Status != LotteryChase.ChaseStatus.Normal)
                {
                    base.Message("当前状态不能撤单");
                    return false;
                }

                List<LotteryChaseItem> itemList = this.GetLotteryChaseItemList(chaseId);

                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    foreach (LotteryChaseItem item in itemList.Where(t => t.Status == LotteryChase.ChaseStatus.Normal))
                    {
                        item.Status = LotteryChase.ChaseStatus.Quit;
                        item.Update(db, t => t.Status);

                        if (!UserAgent.Instance().UnlockMoney(db, chase.UserID, MoneyLock.LockType.LotteryChase, item.ID, "撤销追号"))
                        {
                            db.Rollback();
                            return false;
                        }

                    }

                    chase.Status = LotteryChase.ChaseStatus.Quit;
                    chase.Update(db, t => t.Status);

                    db.Commit();
                }
                return true;
            }
        }



        /// <summary>
        /// 创建追号订单（在定时任务中创建）
        /// </summary>
        public void BuildChaseOrder()
        {
            List<LotteryChaseItem> list;
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                //lot_ChaseItem.StartAt < GETDATE() AND   追号之后全部加入订单
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT TOP 16 lot_ChaseItem.*,Content FROM lot_Chase JOIN lot_ChaseItem ON lot_Chase.ChaseID = lot_ChaseItem.ChaseID WHERE lot_ChaseItem.Status = @Status AND lot_ChaseItem.StartAt < GETDATE() ORDER BY StartAt ASC",
                    NewParam("@Status", LotteryChase.ChaseStatus.Normal));
                if (ds.Tables[0].Rows.Count == 0)
                {
                    db.Commit();
                    return;
                }
                list = ds.ToList<LotteryChaseItem>();
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Content = (string)ds.Tables[0].Rows[i]["Content"];
                }
            }

            list.ForEach(t =>
            {
                try
                {
                    this.MessageClean();
                    if (!this.SaveOrder(t))
                    {
                        SystemAgent.Instance().AddErrorLog(t.SiteID, new Exception(this.Message()), string.Format("[追号失败]ID:{0} {1}", t.ID, this.Message()));
                    }

                }
                catch (Exception ex)
                {
                    SystemAgent.Instance().AddErrorLog(t.SiteID, ex, string.Format("[追号错误]期号{0}", t.Index));
                }
            });
        }

        /// <summary>
        /// 追号订单自动投注（定时器触发）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="itemId">追号的子订单</param>
        /// <param name="orders"></param>
        /// <returns></returns>
        private bool SaveOrder(LotteryChaseItem item)
        {
            if (item == null)
            {
                base.Message("编号错误");
                return false;
            }
            if (item.Status != LotteryChase.ChaseStatus.Normal)
            {
                base.Message("追号ID:{0} {1} 状态错误", item.ID, item.Status);
                return false;
            }
            if (item.StartAt > DateTime.Now)
            {
                base.Message("追号ID:{0} StartAt:{1} 开奖时间大于当前时间", item.ID, item.StartAt);
                return false;

            }

            if (string.IsNullOrEmpty(item.Content))
            {
                base.Message("投注内容为空");
                return false;
            }
            Hashtable[] content = JsonAgent.GetJList(item.Content);
            if (content == null)
            {
                base.Message("投注内容错误：" + item.Content);
                return false;
            }
            int minRebate;
            // 用户在当前彩种的返点
            int rebate = this.GetUserRebate(item.Type, item.UserID, out minRebate);

            List<LotteryOrder> orders = content.Select(ht => new LotteryOrder(ht)).Select(p =>
            {
                p.Times *= item.Times;
                p.Money = p.Money * item.Times;
                p.Index = item.Index;
                p.UserID = item.UserID;
                p.SiteID = item.SiteID;
                p.ChaseID = item.ChaseID;
                p.CreateAt = DateTime.Now;
                if (p.Rebate == 0)
                {
                    p.BetReturn = decimal.Zero;
                    p.Rebate = rebate;
                }
                else
                {
                    p.BetReturn = (rebate - minRebate) / 2000M;
                    p.Rebate = minRebate;
                }
                return p;
            }).ToList();

            if (orders.Count == 0)
            {
                base.Message("投注内容错误：" + item.Content);
                return false;
            }
            int userId = item.UserID;
            int siteId = item.SiteID;
            int itemId = item.ID;

            decimal money = orders.Sum(t => t.Money);
            if (money != item.Money)
            {
                base.Message("金额不一致,追号金额{0},订单金额:{1}", item.Money, money);
                return false;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                LotteryChase chaseOrder = new LotteryChase() { ID = item.ChaseID }.Info(db);
                if (chaseOrder == null || chaseOrder.Status != LotteryChase.ChaseStatus.Normal)
                {
                    base.Message("总订单状态错误");
                    db.Rollback();
                    return false;
                }

                //#1 解锁部分资金
                if (!UserAgent.Instance().UnlockMoney(db, userId, MoneyLock.LockType.LotteryChase, item.ID, string.Format("投注{0}期", item.Index)))
                {
                    base.Message("解锁资金失败，编号:{0}", item.ID);
                    db.Rollback();
                    return false;
                }

                //#2 投注
                foreach (LotteryOrder order in orders)
                {
                    order.SiteID = siteId;
                    order.UserID = userId;

                    if (!this.SaveOrder(db, order))
                    {
                        db.Rollback();
                        return false;
                    }

                    if (!UserAgent.Instance().LockMoney(db, userId, order.Money, MoneyLock.LockType.LotteryOrder, order.ID, string.Format("追号订单{0},第{1}期", order.ChaseID, order.Index)))
                    {
                        this.MessageClean();
                        base.Message("锁定追号资金失败，需锁定金额：{0}元，追号订单{1},第{2}期", order.Money.ToString("n"), order.ChaseID, order.Index);
                        db.Rollback();
                        return false;
                    }
                }

                //#3 修改追号状态为已投注
                item.Status = LotteryChase.ChaseStatus.Finish;
                item.Update(db, t => t.Status);

                //#4 修改追号总状态
                chaseOrder.Count++;
                chaseOrder.BetMoney += item.Money;
                if (chaseOrder.Count == chaseOrder.Total) chaseOrder.Status = LotteryChase.ChaseStatus.Finish;
                chaseOrder.Update(db, t => t.Count, t => t.BetMoney, t => t.Status);

                db.Commit();
                return true;
            }
        }
    }
}
