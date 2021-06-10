using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using BW.Common.Lottery;
using BW.Common.Lottery.Limited;
using SP.Studio.Json;
using SP.Studio.Data;
using BW.Common.Users;
using BW.Common.Sites;

using BW.GateWay.Lottery;
using SP.Studio.Model;
using SP.Studio.Array;
using SP.Studio.Core;
namespace BW.Agent
{
    /// <summary>
    /// 用户的彩票设定、操作
    /// </summary>
    partial class LotteryAgent
    {
        /// <summary>
        /// 从客户端传递过来的json中获取投注订单对象（不存入数据库）
        /// </summary>
        /// <param name="json"></param>
        /// <param name="isRebate">是否允许开启返点</param>
        /// <returns>如果返回Rebate属性为0，则表示是非web程序，请在外部赋值</returns>
        public List<LotteryOrder> GetLotteryOrderList(string json, bool isRebate = true, DateTime? datetime = null)
        {
            Hashtable[] list = JsonAgent.GetJList(json);
            if (list == null)
            {
                base.Message("投注内容不正确");
                return null;
            }
            List<LotteryOrder> orderList = new List<LotteryOrder>();
            foreach (Hashtable ht in list)
            {
                LotteryOrder order = new LotteryOrder(ht, datetime);
                int rebate = this.GetUserRebate(order.Type);
                if (ht.ContainsKey("index") && string.IsNullOrEmpty(order.Index))
                {
                    base.Message("{0}期已封单，{1}", ht["index"], order.Remark);
                    return null;
                }
                if (rebate != int.MinValue)
                {
                    if (rebate == 0)
                    {
                        base.Message("该彩种暂未开放");
                        return null;
                    }

                    // 自定义返点判断
                    int betRebate = ht.GetValue("rebate", 0);
                    if (betRebate == 0 || betRebate >= rebate)
                    {
                        betRebate = rebate;
                        if (SiteInfo.Setting.MaxBetRebate > SiteInfo.Setting.MinRebate &&
                          betRebate > SiteInfo.Setting.MaxBetRebate) betRebate = SiteInfo.Setting.MaxBetRebate;

                        order.Rebate = betRebate;
                        order.BetReturn = (rebate - betRebate) / 2000M;
                    }
                    else if (betRebate <= SiteInfo.Setting.MinRebate)
                    {
                        order.Rebate = SiteInfo.Setting.MinRebate;
                        order.BetReturn = (rebate - order.Rebate) / 2000M;
                    }
                    else
                    {
                        if (SiteInfo.Setting.MaxBetRebate > SiteInfo.Setting.MinRebate &&
                           betRebate > SiteInfo.Setting.MaxBetRebate) betRebate = SiteInfo.Setting.MaxBetRebate;
                        order.Rebate = betRebate;
                        order.BetReturn = (rebate - betRebate) / 2000M;
                    }
                }

                orderList.Add(order);
            }
            return orderList;
        }

        /// <summary>
        /// 获取当前用户在某个彩种中的返点（使用缓存）
        /// </summary>
        /// <param name="type"></param>
        /// <returns>返回0表示该彩种未开放</returns>
        public int GetUserRebate(LotteryType type)
        {
            if (SiteInfo == null || UserInfo == null) return int.MinValue;
            if (type.GetCategory().FullRebate) return 2000;
            LotterySetting setting = SiteInfo.LotteryList.Where(t => t.Game == type).FirstOrDefault();
            if (setting == null) return 0;
            return Utils.GetRebate(SiteInfo.Setting.MaxRebate, UserInfo.Rebate, setting.MaxRebate);
        }

        /// <summary>
        /// 获取用户在某个彩种中的返点（非web调用，无缓存）
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <param name="minRebate">系统的最低返点</param>
        /// <returns></returns>
        public int GetUserRebate(LotteryType type, int userId, out int minRebate)
        {
            if (userId == 0)
            {
                minRebate = 0;
                return 0;
            }
            using (DbExecutor db = NewExecutor())
            {
                int siteId = 0;
                siteId = UserAgent.Instance().GetSiteID(userId, db);
                LotterySetting setting = new LotterySetting() { SiteID = siteId, Game = type }.Info(db, t => t.SiteID, t => t.Game);
                Site.SiteSetting siteSetting = SiteAgent.Instance().GetSiteSetting(siteId, db);
                minRebate = siteSetting.MinRebate;
                if (setting == null) return 0;
                int userRebate = (int)db.ExecuteScalar(CommandType.Text, "SELECT Rebate FROM Users WHERE SiteID = @SiteID AND UserID = @UserID",
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId));
                int siteRebate = siteSetting.MaxRebate;
                return Utils.GetRebate(siteRebate, userRebate, setting.MaxRebate);
            }
        }


        /// <summary>
        /// 保存投注订单
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public bool SaveOrder(int userId, List<LotteryOrder> orders)
        {
            if (UserAgent.Instance().CheckUserLockStatus(userId, User.LockStatus.Bet, User.LockStatus.Contract))
            {
                base.Message("当前账户禁止投注");
                return false;
            }

            if (orders.Count == 0)
            {
                base.Message("没有投注内容");
                return false;
            }

            if (UserInfo != null && orders.FirstOrDefault().Type.GetCategory().NoTest && UserInfo.IsTest)
            {
                base.Message("当前彩种禁止测试帐号投注");
                return false;
            }

            if (orders.Exists(t => t.Bet == 0 || t.Money <= 0))
            {
                base.Message("投注内容错误");
                return false;
            }

            decimal money = orders.Sum(t => t.Money);
            if (money > UserAgent.Instance().GetUserMoney(userId))
            {
                base.Message("可用余额不足");
                return false;
            }

            User user = UserInfo ?? UserAgent.Instance().GetUserInfo(userId);
            int siteId = SiteInfo == null ? UserAgent.Instance().GetSiteID(userId) : SiteInfo.ID;
            Site site = SiteInfo ?? SiteAgent.Instance().GetSiteInfo(siteId);

            if (!orders[0].Type.GetCategory().FullRebate)
            {
                if (orders.Max(t => t.Rebate) > user.Rebate || orders.Min(t => t.Rebate) < site.Setting.MinRebate)
                {
                    base.Message("投注奖金组错误");
                    return false;
                }
            }


            decimal totalMoney = orders.Sum(t => t.Money);
            LotteryOrder lotteryOrder = orders[0];

            LotterySetting setting = this.GetLotterySettingInfo(siteId, lotteryOrder.Type);
            if (setting.MaxBet != decimal.Zero)
            {
                decimal totalBetMoney = (BDC.LotteryOrder.Where(t => t.SiteID == siteId && t.UserID == userId && t.Type == lotteryOrder.Type && t.Index == lotteryOrder.Index && t.Status == LotteryOrder.OrderStatus.Normal).Sum(t => (decimal?)t.Money)
                    ?? (decimal?)decimal.Zero).Value;
                totalMoney += totalBetMoney;
                if (totalMoney > setting.MaxBet)
                {
                    base.Message("单期最多下注{0}元，您在本期已下注{1}元", setting.MaxBet.ToString("n"), totalBetMoney.ToString("n"));
                    return false;
                }
            }

            Dictionary<LotteryOrder, IEnumerable<string>> limited = new Dictionary<LotteryOrder, IEnumerable<string>>();

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                try
                {
                    if (lotteryOrder.Type.GetCategory().BuildIndex)
                    {
                        string buildIndex = DateTime.Now.ToString("ddHHmmssfff");
                        orders.ForEach(t =>
                        {
                            t.Index = buildIndex;
                        });
                    }

                    foreach (LotteryOrder order in orders)
                    {
                        /// 限号功能只有在web程序里面投注才有作用
                        if (SiteInfo != null)
                        {
                            LotteryPlayer play = SiteInfo.LotteryPlayerInfo[order.PlayerID];
                            //LimitedSetting limitedSetting = SiteInfo.GetLimitedInfo(order.Type, play.Player.Limited);

                            //if (limitedSetting != null)
                            //{
                            //    SiteLimited siteLimited = this.GetSiteLimited(order.Type, order.Index, play.Player.Limited);
                            //    // 检查限号
                            //    IEnumerable<string> number = play.Player.ToLimited(order.Number);
                            //    if (number != null)
                            //    {
                            //        if (!LotteryAgent.Instance().CheckLimitedNumber(siteLimited, number, order.Mode * order.Times * play.Player.RewardMoney, limitedSetting.Money))
                            //        {
                            //            db.Rollback();
                            //            return false;
                            //        }
                            //        limited.Add(order, number);
                            //    }
                            //}

                            if (play.MaxBet != 0 && play.MaxBet < order.Bet)
                            {
                                base.Message("{0}单期最多可下{1}注", play.Name, play.MaxBet);
                                db.Rollback();
                                return false;
                            }
                        }

                        order.SiteID = siteId;
                        order.UserID = userId;

                        if (!this.SaveOrder(db, order))
                        {
                            base.Message("订单保存失败");
                            db.Rollback();
                            return false;
                        }

                        if (!UserAgent.Instance().LockMoney(db, userId, order.Money, MoneyLock.LockType.LotteryOrder, order.ID, "投注"))
                        {
                            db.Rollback();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    db.Rollback();
                    return false;
                }

                if (SiteInfo != null)
                {
                    foreach (KeyValuePair<LotteryOrder, IEnumerable<string>> keyValue in limited)
                    {
                        LotteryOrder order = keyValue.Key;
                        LotteryPlayer play = SiteInfo.LotteryPlayerInfo[order.PlayerID];
                        SiteLimited siteLimited = this.GetSiteLimited(order.Type, order.Index, play.Player.Limited);

                        this.AddLimitedNumber(siteLimited, keyValue.Value, order.Mode * order.Times * play.Player.RewardMoney * order.Rebate / 2000M);
                    }
                }

                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 保存即开型订单（投注时候计算结果）
        /// </summary>
        /// <param name="userId">投注用户</param>
        /// <param name="order">订单内容</param>
        /// <returns></returns>
        public bool SaveOrder(int userId, LotteryOrder order, out string resultNumber)
        {
            resultNumber = null;
            if (this.SaveOrder(userId, new List<LotteryOrder>() { order }))
            {
                // 如果是即开型小游戏
                if (order.Type.GetCategory().CategoryInfo.Open)
                {
                    IPlayer player = this.GetPlayerInfo(order.PlayerID).Player;
                    if (!this.OpenResultNumber(order.SiteID, order.Type, order.Index, out resultNumber, player))
                    {
                        this.Revoke(order.ID, userId);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 分布式保存订单
        /// </summary>
        /// <param name="db"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool SaveOrder(DbExecutor db, LotteryOrder order)
        {
            DbParameter orderId = NewParam("@OrderID", order.ID, DbType.Int32, 8, ParameterDirection.Output);
            db.ExecuteNonQuery(CommandType.StoredProcedure, "AddLotteryOrder",
                  NewParam("@SiteID", order.SiteID),
                  NewParam("@UserID", order.UserID),
                  NewParam("@Type", order.Type),
                  NewParam("@Index", order.Index),
                  NewParam("@PlayerID", order.PlayerID),
                  NewParam("@Number", order.Number),
                  NewParam("@Bet", order.Bet),
                  NewParam("@Mode", order.Mode),
                  NewParam("@Times", order.Times),
                  NewParam("@Money", order.Money),
                  NewParam("@CreateAt", order.CreateAt),
                  NewParam("@ChaseID", order.ChaseID),
                  NewParam("@UnitedID", order.UnitedID),
                  NewParam("@Rebate", order.Rebate),
                  NewParam("@BetReturn", order.BetReturn),
                  NewParam("@Remark", order.Remark),
                  orderId);
            if (orderId.Value == DBNull.Value) return false;
            order.ID = (int)orderId.Value;
            return true;
        }

        /// <summary>
        /// 再次购买订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool SaveOrder(int orderId, int userId)
        {
            LotteryOrder order;
            using (DbExecutor db = NewExecutor())
            {
                order = this.GetLotteryOrderInfo(db, orderId, userId);
                if (order == null || order.UserID != userId)
                {
                    base.Message("订单编号错误");
                    return false;
                }
            }
            int time;
            order.Index = Utils.GetLotteryBetIndex(order.Type, out time);
            order.CreateAt = DateTime.Now;
            return this.SaveOrder(order.UserID, new List<LotteryOrder>() { order });
        }

        /// <summary>
        /// 更新订单的开奖信息
        /// </summary>
        /// <param name="db"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool SaveOrderReward(DbExecutor db, LotteryOrder order)
        {
            return db.ExecuteNonQuery(CommandType.StoredProcedure, "UpdateLotteryOrderReward",
                NewParam("@OrderID", order.ID),
                NewParam("@UserID", order.UserID),
                NewParam("@Reward", order.Reward),
                NewParam("@IsLottery", order.IsLottery),
                NewParam("@Status", order.Status),
                NewParam("@ResultNumber", order.ResultNumber),
                NewParam("@LotteryAt", order.LotteryAt)) != 0;
        }

        /// <summary>
        /// 修改订单投注号码
        /// </summary>
        /// <param name="db"></param>
        /// <param name="order"></param>
        private bool SaveOrderNumber(DbExecutor db, LotteryOrder order)
        {
            return db.ExecuteNonQuery(CommandType.StoredProcedure, "UpdateLotteryOrderNumber",
               NewParam("@OrderID", order.ID),
               NewParam("@UserID", order.UserID),
               NewParam("@Type", order.Type),
               NewParam("@PlayerID", order.PlayerID),
               NewParam("@Number", order.Number)) != 0;
        }

        /// <summary>
        /// 保存单个投注订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool SaveOrder(int userId, LotteryOrder order)
        {
            return this.SaveOrder(userId, new List<LotteryOrder>() { order });
        }

        /// <summary>
        /// 保存追号订单
        /// </summary>
        /// <param name="chase">追号订单</param>
        /// <param name="items">追号的详情设置</param>
        /// <returns></returns>
        public bool SaveOrder(int userId, LotteryChase chase, params LotteryChaseItem[] items)
        {
            List<LotteryOrder> order = this.GetLotteryOrderList(chase.Content);
            if (order == null || order.Count == 0 || order.Exists(t => t.Money == decimal.Zero))
            {
                base.Message("投注内容错误");
                return false;
            }
            if (items.Count(t => t.Times < 1) > 0)
            {
                base.Message("倍数错误");
                return false;
            }

            if (UserAgent.Instance().CheckUserLockStatus(userId, User.LockStatus.Bet, User.LockStatus.Contract))
            {
                base.Message("当前账户禁止投注");
                return false;
            }

            if (UserInfo != null && order.FirstOrDefault().Type.GetCategory().NoTest && UserInfo.IsTest)
            {
                base.Message("当前彩种禁止测试帐号投注");
                return false;
            }

            decimal money = order.Sum(t => t.Money);
            LotteryType type = chase.Type;

            chase.SiteID = SiteInfo.ID;
            chase.UserID = UserInfo.ID;
            chase.CreateAt = DateTime.Now;
            chase.Money = money * items.Sum(t => t.Times);
            chase.Total = items.Length;

            decimal userMoney = UserAgent.Instance().GetUserMoney(userId);
            if (userMoney < chase.Money)
            {
                base.Message("可用余额不足");
                return false;
            }

            Dictionary<string, DateTime> indexDic = Utils.GetLotteryIndexStartTime(type, 120).ToDictionary(t => t.Index, t => t.ResultAt);

            IEnumerable<string> indexList = items.Select(t => t.Index).Except(indexDic.Select(t => t.Key));
            if (indexList.Count() != 0)
            {
                base.Message("{0}不支持追号", string.Join(",", indexList));
                return false;
            }
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                try
                {
                    if (!chase.Add(true, db))
                    {
                        db.Rollback();
                        base.Message("发生错误");
                        return false;
                    }

                    DateTime startAt = DateTime.Now;
                    foreach (LotteryChaseItem item in items)
                    {
                        item.ChaseID = chase.ID;
                        item.UserID = chase.UserID;
                        item.Type = chase.Type;
                        item.Money = money * item.Times;
                        item.SiteID = chase.SiteID;
                        item.CreateAt = DateTime.Now;
                        item.StartAt = indexDic[item.Index];
                        item.Add(true, db);

                        if (!UserAgent.Instance().LockMoney(db, userId, item.Money, MoneyLock.LockType.LotteryChase, item.ID, string.Format("[追号]{0}第{1}期", this.GetLotteryName(item.Type), item.Index)))
                        {
                            db.Rollback();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    db.Rollback();
                    return false;
                }

                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 保存微信投注
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SaveOrder(int userId, LotteryType type, string content)
        {
            string betIndex;
            return this.SaveOrder(userId, type, content, out betIndex);
        }

        /// <summary>
        /// 保存微信投注（返回当前投注期）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public bool SaveOrder(int userId, LotteryType type, string content, out string betIndex)
        {
            if ((int)type == 0 || !type.GetCategory().Wechat)
            {
                base.Message("不支持微信投注");
                betIndex = string.Empty;
                return false;
            }

            int siteId = UserAgent.Instance().GetSiteID(userId);

            if (!Utils.IsBet(type, out betIndex, siteId))
            {
                base.Message("当前期已封单");
                return false;
            }

            int times;
            string number;
            string data = PlayerFactory.GetPlayer(type, content, out number, out times, siteId);

            if (string.IsNullOrEmpty(data))
            {
                base.Message("格式错误");
                return false;
            }

            List<LotteryOrder> orders = LotteryAgent.Instance().GetLotteryOrderList(data);
            if (orders == null)
            {
                return false;
            }

            LotteryOrder order = orders.FirstOrDefault();
            int siteMinRebate;
            orders.ForEach(t =>
            {
                t.Remark = content;
                if (t.Rebate == 0) t.Rebate = LotteryAgent.Instance().GetUserRebate(t.Type, userId, out siteMinRebate);
            });

            return LotteryAgent.Instance().SaveOrder(userId, orders);
        }



        /// <summary>
        /// 获取用户近期的投注订单情况
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<LotteryOrder> GetLotteryOrderList(int userId, LotteryType type, int count)
        {
            if (!this.CheckLogin(userId))
            {
                base.Message("用户标识错误");
                return null;
            }

            int tableId = userId.GetTableID();
            return BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID && t.TableID == tableId && t.Type == type && t.UserID == userId).OrderByDescending(t => t.ID).Take(count).ToList();
        }

        /// <summary>
        /// 获取批量订单情况（跨站点）
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public List<LotteryOrder> GetLotteryOrderList(int[] orders)
        {
            return BDC.LotteryOrder.Where(t => orders.Contains(t.ID)).ToList();
        }

        /// <summary>
        /// 判断当前投注订单是否可以撤单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool IsRevoke(LotteryOrder order)
        {
            if (order.Status != LotteryOrder.OrderStatus.Normal) return false;
            return this.IsRevoke(order.Type, order.Index);
        }

        public bool IsRevoke(LotteryType type, string index)
        {
            DateTime resultAt = Utils.GetLotteryTime(type, index);
            if (resultAt == DateTime.MinValue) return false;

            int stopTime = type.GetCategory().StopTime;
            return DateTime.Now.AddHours(type.GetCategory().TimeDifference) < resultAt.AddSeconds(stopTime * -1);
        }

        /// <summary>
        /// 提交撤单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool LotteryOrderRevoke(LotteryOrder order)
        {
            if (!this.IsRevoke(order))
            {
                base.Message("该订单不允许撤单");
                return false;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                //#1 解锁用户资金
                if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.LotteryOrder, order.ID, "撤单"))
                {
                    db.Rollback();
                    return false;
                }

                db.ExecuteNonQuery(CommandType.StoredProcedure, "UpdateLotteryOrderRevoke",
                    NewParam("@OrderID", order.ID),
                    NewParam("@UserID", order.UserID));

                db.Commit();
            }

            //#3 扣除限号占用的额度

            //LotteryPlayer play = SiteInfo.LotteryPlayerInfo[order.PlayerID];
            //LimitedSetting limitedSetting = SiteInfo.GetLimitedInfo(order.Type, play.Player.Limited);
            //if (limitedSetting != null)
            //{
            //    SiteLimited siteLimited = this.GetSiteLimited(order.Type, order.Index, play.Player.Limited);
            //    // 检查限号
            //    IEnumerable<string> number = play.Player.ToLimited(order.Number);
            //    if (number != null)
            //    {
            //        foreach (string num in number)
            //        {
            //            this.RevokeLimitedNumber(siteLimited, num, play.Player.RewardMoney * order.Mode * order.Times * order.Rebate / 2000M);
            //        }
            //    }
            //}

            return true;
        }

        /// <summary>
        /// 管理员提交强制撤单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool LotteryOrderRevoke(int orderId)
        {
            if (AdminInfo == null)
            {
                base.Message("没有权限");
                return false;
            }

            LotteryOrder order = this.GetLotteryOrderInfo(orderId);
            if (order == null)
            {
                base.Message("订单编号错误");
                return false;
            }

            if (order.Status == LotteryOrder.OrderStatus.Revoke)
            {
                base.Message("已经撤单");
                return false;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                switch (order.Status)
                {
                    case LotteryOrder.OrderStatus.Normal:
                        //#1 解锁用户资金
                        if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.LotteryOrder, order.ID, "管理员操作撤单"))
                        {
                            base.Message("撤单失败，投注资金已被解锁");
                            db.Rollback();
                            return false;
                        }
                        break;
                    default:
                        //#1 退回已投注的金额
                        MoneyLog betLog = UserAgent.Instance().GetMoneyLogInfo(db, order.UserID, MoneyLog.MoneyType.Bet, order.ID, SiteInfo.ID);
                        if (betLog == null)
                        {
                            base.Message("没有投注记录");
                            db.Rollback();
                            return false;
                        }
                        //#2 退回投注金额
                        UserAgent.Instance().AddMoneyLog(db, order.UserID, Math.Abs(betLog.Money), MoneyLog.MoneyType.BetRevoke, order.ID, "撤单，退回投注金");

                        //#3 退回奖金
                        MoneyLog rewardLog = UserAgent.Instance().GetMoneyLogInfo(db, order.UserID, MoneyLog.MoneyType.Reward, order.ID, SiteInfo.ID);
                        if (rewardLog != null)
                        {
                            // #4 如果奖金不够扣则返回失败
                            if (!UserAgent.Instance().AddMoneyLog(db, order.UserID, Math.Abs(rewardLog.Money) * -1, MoneyLog.MoneyType.RewardRevoke, order.ID, "撤单，退回奖金"))
                            {
                                db.Rollback();
                                return false;
                            }
                        }

                        break;
                }
                //#2 修改状态
                db.ExecuteNonQuery(CommandType.StoredProcedure, "UpdateLotteryOrderRevoke",
                     NewParam("@OrderID", orderId),
                     NewParam("@UserID", order.UserID));

                db.Commit();
            }
            return true;
        }

        /// <summary>
        /// 系统提交自动撤单（服务中运行）
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private bool Revoke(int orderId, int userId)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                LotteryOrder order = new LotteryOrder() { ID = orderId }.Info(db);
                if (order == null || order.Status != LotteryOrder.OrderStatus.Normal)
                {
                    base.Message("撤单失败，编号或者状态错误");
                    db.Rollback();
                    return false;
                }

                if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.LotteryOrder, order.ID, "系统自动撤单"))
                {
                    base.Message("撤单失败，投注资金已被解锁");
                    db.Rollback();
                    return false;
                }

                db.ExecuteNonQuery(CommandType.StoredProcedure, "UpdateLotteryOrderRevoke",
                    NewParam("@OrderID", orderId),
                    NewParam("@UserID", userId));

                db.Commit();

                return true;
            }
        }

        /// <summary>
        /// 获取全局的中奖名单
        /// </summary>
        /// <param name="money"></param>
        /// <param name="count"></param>
        public List<RewardTip> GetRewardTip(decimal money, int count = 20)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "data_GetRewardTip",
                    NewParam("@Money", money));
                List<RewardTip> list = new List<RewardTip>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new RewardTip(dr));
                }
                return list;
            }
        }
    }
}
