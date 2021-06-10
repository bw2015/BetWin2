using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;

using SP.Studio.Data;
using SP.Studio.Model;
using SP.Studio.Core;
using SP.Studio.Json;

using BW.Common.Lottery;
using BW.Common.Users;

namespace BW.Agent
{
    /// <summary>
    /// 合买
    /// </summary>
    partial class LotteryAgent
    {
        /// <summary>
        /// 发布一个合买
        /// </summary>
        /// <param name="united"></param>
        /// <returns></returns>
        public bool AddUnited(int userId, United united)
        {
            united.UserID = userId;
            united.SiteID = SiteInfo.ID;
            united.CreateAt = DateTime.Now;

            if (string.IsNullOrEmpty(united.Title))
            {
                base.Message("请输入合买标题");
                return false;
            }
            if (united.Total == 0)
            {
                base.Message("总份数设置错误");
                return false;
            }

            List<LotteryOrder> orders = LotteryAgent.Instance().GetLotteryOrderList(united.Number, false);
            if (orders.Count == 0 || orders.Exists(t => t.Bet == 0))
            {
                base.Message("投注内容错误");
                return false;
            }

            united.Money = orders.Sum(t => t.Money);
            if (united.Money == decimal.Zero)
            {
                base.Message("投注金额错误");
                return false;
            }

            if (Math.Round(united.Money / united.Total, 2) * united.Total != united.Money)
            {
                base.Message("份数错误，金额不能被总份数整除到分");
                return false;
            }

            if (united.Buyed < united.Total * SiteInfo.Setting.UnitedMin)
            {
                base.Message("发起人认购不应小于{0}注", united.Total * SiteInfo.Setting.UnitedMin);
                return false;
            }

            united.Type = orders.First().Type;

            united.CloseAt = Utils.GetLotteryTime(united.Type, united.Index).AddSeconds(united.Type.GetCategory().StopTime * -1);
            if (united.CloseAt < DateTime.Now)
            {
                base.Message("{0}期已过封单时间", united.Index);
                return false;
            }

            if (united.Commission > 0.2M)
            {
                base.Message("佣金比例错误");
                return false;
            }

            int buyed = united.Buyed;

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                try
                {
                    united.Buyed = 0;
                    united.Rebate = this.GetUserRebate(united.Type);

                    //#1 添加合买订单
                    if (!united.Add(true, db))
                    {
                        db.Rollback();
                        return false;
                    }

                    if (united.Package != 0)
                    {
                        //#2 锁定保底金额
                        if (!UserAgent.Instance().LockMoney(db, userId, united.Money * (decimal)united.Package / (decimal)united.Total, MoneyLock.LockType.LotteryUnitedPackage, united.ID, "合买保底"))
                        {
                            db.Rollback();
                            return false;
                        }
                    }

                    //#3 发布者参加合买
                    if (!this.AddUnitedItem(db, userId, united.ID, buyed))
                    {
                        db.Rollback();
                        return false;
                    }

                    db.Commit();
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 参加合买
        /// </summary>
        /// <param name="db">数据库事务对象</param>
        /// <param name="userId">合买参与者</param>
        /// <param name="unitedId">合买对象</param>
        /// <param name="unit">购买的份额</param>
        /// <returns></returns>
        public bool AddUnitedItem(DbExecutor db, int userId, int unitedId, int unit)
        {
            if (unitedId == 0)
            {
                base.Message("合买编号错误");
                return false;
            }
            if (unit <= 0)
            {
                base.Message("份数输入错误");
                return false;
            }

            United united = new United() { ID = unitedId }.Info(db);
            if (united == null || united.SiteID != SiteInfo.ID || united.Status != United.UnitedStatus.Normal)
            {
                base.Message("合买订单错误");
                return false;
            }
            if (united.CloseAt < DateTime.Now)
            {
                base.Message("已封单");
                return false;
            }

            if (united.Buyed + unit > united.Total)
            {
                base.Message("剩余份额不足");
                return false;
            }

            decimal money = unit * united.UnitMoney;
            if (money == decimal.Zero)
            {
                base.Message("合买订单份数设置错误");
                return false;
            }

            UnitedItem item = new UnitedItem()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                UnitedID = unitedId,
                CreateAt = DateTime.Now,
                Money = money,
                Status = United.UnitedStatus.Normal,
                Unit = unit,
                Rebate = LotteryAgent.Instance().GetUserRebate(united.Type)
            };

            if (!item.Add(true, db))
            {
                base.Message("发生未知错误");
                return false;
            }

            if (!UserAgent.Instance().LockMoney(db, userId, unit * united.UnitMoney, MoneyLock.LockType.LotteryUnited, item.ID, "参与合买"))
            {
                return false;
            }

            united.Buyed += unit;
            united.Update(db, t => t.Buyed);

            return true;
        }

        /// <summary>
        /// 参加合买
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="unitedId"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool AddUnitedItem(int userId, int unitedId, int unit)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (!this.AddUnitedItem(db, userId, unitedId, unit))
                {
                    db.Rollback();
                    return false;
                }
                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 获取合买信息
        /// </summary>
        /// <param name="unitedId"></param>
        /// <returns></returns>
        public United GetlotteryUnitedInfo(int unitedId)
        {
            return BDC.United.Where(t => t.SiteID == SiteInfo.ID && t.ID == unitedId).FirstOrDefault();
        }

        /// <summary>
        /// 返回购买记录
        /// </summary>
        /// <param name="unitedId"></param>
        /// <returns></returns>
        public List<UnitedItem> GetLotteryUnitedList(int unitedId)
        {
            return BDC.UnitedItem.Where(t => t.SiteID == SiteInfo.ID && t.UnitedID == unitedId).ToList();
        }

        /// <summary>
        /// 是否已经参加了合买
        /// </summary>
        /// <param name="unitedId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsUnitedJoin(int unitedId, int userId)
        {
            return BDC.UnitedItem.Where(t => t.SiteID == SiteInfo.ID && t.UnitedID == unitedId && t.UserID == userId).Count() > 0;
        }

        /// <summary>
        /// 生成合买订单（非web程序调用）
        /// </summary>
        internal void BuildUnitedOrder()
        {
            IList<United> list = BDC.United.Where(t => t.Status == United.UnitedStatus.Normal && t.CloseAt < DateTime.Now).ToList();

            foreach (United united in list)
            {
                this.BuildUnitedOrder(united);
            }
        }

        /// <summary>
        /// 合买订单转换成为投注订单
        /// 此时不解锁资金，开奖时才解锁资金
        /// </summary>
        /// <param name="united"></param>
        private void BuildUnitedOrder(United united)
        {
            List<UnitedItem> itemList = BDC.UnitedItem.Where(t => t.SiteID == united.SiteID && t.UnitedID == united.ID && t.Status == United.UnitedStatus.Normal).ToList();

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                //#1 修改状态
                united.Status = United.UnitedStatus.Order;
                united.Update(db, t => t.Status);

                //#2 修改跟单状态
                itemList.ForEach(t =>
                {
                    t.Status = United.UnitedStatus.Order;
                    t.Update(db, p => p.Status);
                });

                Hashtable[] orders = JsonAgent.GetJList(united.Number);
                //#3 存入彩票订单
                orders.Select(t => new LotteryOrder(t)).ToList().ForEach(t =>
                {
                    t.SiteID = united.SiteID;
                    t.UserID = united.UserID;
                    t.Status = LotteryOrder.OrderStatus.Normal;
                    t.CreateAt = united.CreateAt;
                    t.UnitedID = united.ID;
                    t.Index = united.Index;
                    t.Type = united.Type;
                    t.Rebate = 2000;
                    t.BetReturn = decimal.Zero;

                    t.Add(db);
                });

                db.Commit();
            }
        }
    }
}
