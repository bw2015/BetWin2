using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using BW.Common;
using BW.Common.Lottery;
using BW.Common.Users;
using BW.GateWay.Lottery;
using BW.Framework;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.ErrorLog;

namespace BW.Agent
{
    /// <summary>
    /// 供服务端调用
    /// </summary>
    partial class LotteryAgent
    {

        /// <summary>
        /// 获取自上一次获取时间的新开奖结果
        /// </summary>
        /// <param name="lastTime"></param>
        /// <returns></returns>
        public List<ResultNumber> GetResultNumber(DateTime lastTime)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetResultNumber",
                    NewParam("@LastTime", lastTime));
                return ds.ToList<ResultNumber>();
            }
        }

        #region ============== 开奖方法，需兼容非web方法 ===================

        /// <summary>
        /// 指定订单号开奖
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool OpenReward(int orderId)
        {
            string number = null;
            using (DbExecutor db = NewExecutor())
            {
                object obj = db.ExecuteScalar(CommandType.StoredProcedure, "GetRewardResultNumber",
                        NewParam("@OrderID", orderId));
                if (obj == DBNull.Value)
                {
                    base.Message("订单{0}未开奖", orderId);
                    return false;
                }
                number = (string)obj;
            }
            if (string.IsNullOrEmpty(number))
            {
                base.Message("订单{0}未开奖", orderId);
                return false;
            }
            return this.OpenReward(orderId, number, 0);
        }

        /// <summary>
        /// 指定订单号开奖（非web程序）
        /// </summary>
        /// <param name="orderId">投注订单号</param>
        /// <param name="number">开奖号码</param>
        /// <param name="userId">所属的用户</param>
        /// <returns></returns>
        public bool OpenReward(int orderId, string number, int userId)
        {
            LotteryType type;
            bool isNeedRevoke = false;
            try
            {
                using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
                {
                    LotteryOrder order = this.GetLotteryOrderInfo(db, orderId, userId);
                    if (order.IsLottery)
                    {
                        db.Rollback();
                        base.Message("该订单已开奖");
                        return false;
                    }

                    LotterySetting lotterySetting = new LotterySetting(db.GetDataSet(CommandType.Text, "SELECT GameName,IsManual,SinglePercent,SingleReward FROM lot_Setting WHERE SiteID = @SiteID AND Game = @Type",
                           NewParam("@SiteID", order.SiteID),
                           NewParam("@Type", order.Type)));
                    if (!order.Type.GetCategory().SiteLottery)
                    {
                        if (lotterySetting.IsManual)
                        {
                            db.Rollback();
                            base.Message("已设置为手动开奖");
                            return false;
                        }
                        // 检查是否有自定义开奖
                        string _number = (string)db.ExecuteScalar(CommandType.Text, "SELECT Number FROM lot_SiteResultNumber WHERE SiteID = @SiteID AND [Type] = @Type AND [Index] = @Index",
                            NewParam("@SiteID", order.SiteID),
                            NewParam("@Type", order.Type),
                            NewParam("@Index", order.Index));
                        if (!string.IsNullOrEmpty(_number))
                        {
                            number = _number;
                        }
                    }

                    LotteryPlayer player = new LotteryPlayer() { ID = order.PlayerID }.Info(db);
                    IPlayer playerObject = PlayerFactory.GetPlayer(player.Code, out type);
                    bool isSingle = false;
                    int singleBet = player.GetSingleBet(lotterySetting.SinglePercent);
                    decimal singleReward = player.GetSingleReward(lotterySetting.SingleReward);

                    if (playerObject == null)
                    {
                        base.Message("玩法代码：{0}不正确", player.Code);
                        return false;
                    }
                    decimal reward = playerObject.Reward(order.Number, number, player.Reward);
                    if (order.Bet <= singleBet && singleReward != decimal.Zero && reward > singleReward)
                    {
                        isSingle = true;
                        reward = singleReward;
                    }

                    decimal betReturn;
                    order.ResultNumber = number;

                    if (!order.SetReward(reward, out betReturn))
                    {
                        this.SaveOrderReward(db, order);
                        db.Commit();
                        isNeedRevoke = true;
                        base.Message("撤单处理");
                        return false;
                    }

                    this.SaveOrderReward(db, order);
                    if (order.UnitedID != 0)
                    {
                        Dictionary<int, decimal> unitedUser = new Dictionary<int, decimal>();

                        //#1 合买订单
                        United united = new United() { ID = order.UnitedID }.Info(db);
                        if (united == null || united.SiteID != order.SiteID || united.Status != United.UnitedStatus.Order)
                        {
                            db.Rollback();
                            base.Message("合买订单编号错误");
                            return false;
                        }

                        // 佣金
                        decimal commissionMoney = decimal.Zero;

                        //#2 获取合买跟单列表
                        List<UnitedItem> unitedList = db.GetDataSet(CommandType.Text, "SELECT * FROM lot_UnitedItem WHERE SiteID = @SiteID AND UnitedID = @UnitedID AND Status = @Status",
                            NewParam("@SiteID", united.SiteID),
                            NewParam("@UnitedID", united.ID),
                            NewParam("@Status", United.UnitedStatus.Order)).ToList<UnitedItem>();

                        //#3 解锁资金并且扣款，修改跟单状态
                        foreach (UnitedItem item in unitedList)
                        {
                            //#3.1 解锁资金
                            if (!UserAgent.Instance().UnlockMoney(db, item.UserID, MoneyLock.LockType.LotteryUnited, item.ID, "彩票开奖"))
                            {
                                db.Rollback();
                                base.Message("解锁合买资金发生错误UserID:{0},Type:{1},SourceID:{2}", item.UserID, MoneyLock.LockType.LotteryUnited, item.ID);
                                return false;
                            }

                            //#3.2 扣款资金
                            if (!UserAgent.Instance().AddMoneyLog(db, item.UserID, Math.Abs(item.Money) * -1, MoneyLog.MoneyType.UnitedBet, item.ID,
                                    string.Format("{0} {1}，合买跟单编号:{2}", LotteryAgent.Instance().GetLotteryName(type, order.SiteID), player.Name, item.ID)))
                            {
                                db.Rollback();
                                base.Message("扣款错误");
                                return false;
                            }

                            //#3.3 修改状态
                            item.Status = United.UnitedStatus.Finish;
                            item.Update(db, t => t.Status);

                            //#3.4 派发奖金
                            if (reward != decimal.Zero)
                            {
                                decimal unitedItemMoney = reward * (decimal)item.Unit / (decimal)united.Total * (item.Rebate / 2000M);
                                if (united.Commission > decimal.Zero)
                                {
                                    decimal unitedItemCommission = Math.Min(decimal.Zero, (unitedItemMoney - item.Money) * united.Commission);
                                    commissionMoney += unitedItemCommission;
                                    unitedItemMoney -= unitedItemCommission;
                                }

                                if (unitedUser.ContainsKey(item.UserID))
                                {
                                    unitedUser[item.UserID] += unitedItemMoney;
                                }
                                else
                                {
                                    unitedUser.Add(item.UserID, unitedItemMoney);
                                }
                            }
                        }

                        //#4 解锁保底资金
                        if (united.Package > 0)
                        {
                            //#4.1 解锁资金
                            if (!UserAgent.Instance().UnlockMoney(db, united.UserID, MoneyLock.LockType.LotteryUnitedPackage, united.ID, "彩票开奖"))
                            {
                                db.Rollback();
                                base.Message("解锁合买保底资金发生错误UserID:{0},Type:{1},SourceID:{2}", united.UserID, MoneyLock.LockType.LotteryUnitedPackage, united.ID);
                                return false;
                            }

                            //#4.2 扣除保底投注金额
                            decimal packageMoney = Math.Min(united.Package, united.Remaining) * united.UnitMoney;
                            if (!UserAgent.Instance().AddMoneyLog(db, united.UserID, packageMoney * -1, MoneyLog.MoneyType.UnitedPackage, united.ID,
                                 string.Format("{0} {1}，合买保底,单号[{2}]", LotteryAgent.Instance().GetLotteryName(type, order.SiteID), player.Name, united.ID)))
                            {
                                db.Rollback();
                                base.Message("合买保底扣款错误");
                                return false;
                            }

                            //#4.3 计算保底可得奖金
                            if (reward != decimal.Zero)
                            {
                                decimal unitedPackageMoney = reward * Math.Min(united.Package, united.Remaining) / united.Total * (united.Rebate / 2000M);
                                if (unitedUser.ContainsKey(united.UserID))
                                {
                                    unitedUser[united.UserID] += unitedPackageMoney;
                                }
                                else
                                {
                                    unitedUser.Add(united.UserID, unitedPackageMoney);
                                }
                            }
                        }

                        //#5 派发奖金
                        if (unitedUser.Count != 0)
                        {
                            foreach (KeyValuePair<int, decimal> item in unitedUser)
                            {
                                UserAgent.Instance().AddMoneyLog(db, item.Key, item.Value, MoneyLog.MoneyType.UnitedReward, united.ID, string.Format("合买中奖，合买编号{0}", united.ID));
                            }
                        }

                        //#6 派发佣金
                        if (commissionMoney != decimal.Zero)
                        {
                            UserAgent.Instance().AddMoneyLog(db, united.UserID, commissionMoney, MoneyLog.MoneyType.UnitedCommission, united.ID, string.Format("合买佣金,合买编号:{0}", united.ID));
                        }
                    }
                    else
                    {
                        //#1 解锁资金
                        if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.LotteryOrder, order.ID, "彩票开奖"))
                        {
                            db.Rollback();
                            base.Message("解锁资金发生错误 UserID:{0},Type:{1},SourceID:{2}", order.UserID, MoneyLock.LockType.LotteryOrder, orderId);
                            return false;
                        }

                        //#2 扣除用户资金（同步增加提现流水）
                        if (!UserAgent.Instance().AddMoneyLog(db, order.UserID, Math.Abs(order.Money) * -1, MoneyLog.MoneyType.Bet, order.ID,
                            string.Format("{0} {1}，单号[{2}]", LotteryAgent.Instance().GetLotteryName(type, order.SiteID), player.Name, order.ID)))
                        {
                            db.Rollback();
                            base.Message("扣款错误");
                            return false;
                        }

                        //#3 返点
                        if (betReturn > decimal.Zero)
                        {
                            UserAgent.Instance().AddMoneyLog(db, order.UserID, betReturn, MoneyLog.MoneyType.Rebate, order.ID,
                                string.Format("投注单号[{0}]返点", order.ID));
                        }

                        //#4 奖金派发
                        if (order.Reward > decimal.Zero)
                        {
                            UserAgent.Instance().AddMoneyLog(db, order.UserID, order.Reward, MoneyLog.MoneyType.Reward, order.ID,
                                string.Format("{0} {1}，单号[{2}]{3}", lotterySetting.Name, player.Name, order.ID,isSingle ? "(单挑)" : ""));
                        }

                        //#5 上级返点
                        //SiteAgent.Instance()._lotteryBetAgent(db, order.ID);

                        //#6 如果是追号订单
                        if (order.ChaseID != 0)
                        {
                            this.UpdateChaseOrder(db, order.ChaseID, order.Index, order.Reward, order.Money);
                        }

                        //#8 添加返点数据
                        new LotteryBetAgent()
                        {
                            CreateAt = order.CreateAt,
                            Money = order.Money,
                            OrderID = order.ID,
                            SiteID = order.SiteID,
                            UserID = order.UserID,
                            Rebate = order.Rebate + (int)(order.BetReturn * 2000M)
                        }.Add(db);
                    }
                    db.Commit();

                    //#7 发送中奖通知
                    UserAgent.Instance().AddNotify(order.UserID, UserNotify.NotifyType.Lottery,
                        "{0}{1}期投注单号{2}已派奖，本期盈利{3}元", this.GetLotteryName(order.Type, order.SiteID), order.Index, order.ID, (order.Reward - order.Money).ToString("c"));

                    return true;
                }
            }
            finally
            {
                if (isNeedRevoke)
                {
                    this.Revoke(orderId, userId);
                }
            }
        }




        /// <summary>
        /// 开奖之后更新追号订单的状态
        /// </summary>
        /// <param name="db"></param>
        /// <param name="chaseId">追号编号</param>
        /// <param name="index">彩期</param>
        /// <param name="reward">中奖金额</param>
        /// <param name="betMoney">投注金额</param>
        private void UpdateChaseOrder(DbExecutor db, int chaseId, string index, decimal reward, decimal betMoney)
        {
            if (chaseId == 0) return;
            // 获取追号总订单
            LotteryChase chaseOrder = new LotteryChase() { ID = chaseId }.Info(db);
            if (chaseOrder == null || (chaseOrder.Status != LotteryChase.ChaseStatus.Normal && chaseOrder.Status != LotteryChase.ChaseStatus.Finish))
            {
                return;
            }
            // 当前追号子订单
            LotteryChaseItem item = new LotteryChaseItem() { ChaseID = chaseId, Index = index }.Info(db, t => t.ChaseID, t => t.Index);
            if (item == null || item.Status != LotteryChase.ChaseStatus.Finish)
            {
                return;
            }

            // 修改子订单的中奖金额以及状态为已开奖
            item.Status = LotteryChase.ChaseStatus.IsOpen;
            item.Reward = reward;
            item.Update(db, t => t.Reward, t => t.Status);

            // 如果是中奖后终止追号则对未投注的订单进行撤单处理
            if (chaseOrder.IsRewardStop && reward != decimal.Zero)
            {
                chaseOrder.Status = LotteryChase.ChaseStatus.Reward;

                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT ItemID FROM lot_ChaseItem WHERE SiteID = @SiteID AND ChaseID = @ChaseID AND Status = @Status",
                  NewParam("@SiteID", chaseOrder.SiteID),
                  NewParam("@ChaseID", chaseId),
                  NewParam("@Status", LotteryChase.ChaseStatus.Normal));

                foreach (int itemId in ds.ToList<int>())
                {
                    if (UserAgent.Instance().UnlockMoney(db, chaseOrder.UserID, MoneyLock.LockType.LotteryChase, itemId, "中奖后取消追号"))
                    {
                        new LotteryChaseItem() { ID = itemId, Status = LotteryChase.ChaseStatus.Reward }.Update(db, t => t.Status);
                    }
                }
                chaseOrder.Status = LotteryChase.ChaseStatus.Reward;

                // 已投注订单全部撤单
                ds = db.GetDataSet(CommandType.Text, "SELECT OrderID,UserID FROM lot_Order WHERE SiteID = @SiteID AND UserID = @UserID AND ChaseID = @ChaseID AND IsLottery = 0 AND Status = @Status AND [Index] != @Index",
                    NewParam("@SiteID", chaseOrder.SiteID),
                    NewParam("@UserID", chaseOrder.UserID),
                    NewParam("@ChaseID", chaseOrder.ID),
                    NewParam("@Status", LotteryOrder.OrderStatus.Normal),
                    NewParam("@Index", index));

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int orderId = (int)dr["OrderID"];
                    int userId = (int)dr["UserID"];
                    db.ExecuteNonQuery(CommandType.StoredProcedure, "UpdateLotteryOrderRevoke",
                        NewParam("@OrderID", orderId),
                        NewParam("@UserID", userId));

                    UserAgent.Instance().UnlockMoney(db, chaseOrder.UserID, MoneyLock.LockType.LotteryOrder, orderId, "中奖后撤单");
                }

            }
            chaseOrder.Reward += reward;
            chaseOrder.Update(db, t => t.Reward, t => t.Status);
        }



        /// <summary>
        /// 获取系统中待开奖的订单
        /// </summary>
        /// <returns></returns>
        public List<RewardOrder> GetRewardOrderList(int siteId = 0, int tableId = 0)
        {
            try
            {
                using (DbExecutor db = NewExecutor())
                {
                    DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetRewardOrder",
                        NewParam("@SiteID", siteId),
                        NewParam("@TableID", tableId));
                    List<RewardOrder> list = new List<RewardOrder>();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        list.Add(new RewardOrder(dr));
                    }
                    return list;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 开奖成功后的回调通知（非Web程序）
        /// </summary>
        public Dictionary<LotteryType, List<Action<LotteryType, int[]>>> OpenRewardCallback = new Dictionary<LotteryType, List<Action<LotteryType, int[]>>>();

        /// <summary>
        /// 派奖结果的计数器
        /// </summary>
        public Dictionary<bool, int> openRewardResult = new Dictionary<bool, int>();
        /// <summary>
        /// 多线程派奖
        /// </summary>
        public int OpenRewardByTable(int tableId)
        {
            List<RewardOrder> list = this.GetRewardOrderList(0, tableId);
            int total = list.Count;
            if (total == 0) return total;
            int consoleLeft = Console.CursorLeft;
            this.OpenReward(list);
            return total;
        }

        private void OpenReward(IEnumerable<RewardOrder> list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}]正在派奖", DateTime.Now).AppendLine();

            foreach (RewardOrder t in list)
            {
                sb.AppendFormat("{0}/{1}", t.OrderID, t.Number);
                try
                {
                    this.MessageClean();
                    bool success = this.OpenReward(t.OrderID, t.Number, t.UserID);
                    if (!success)
                    {
                        SystemAgent.Instance().AddErrorLog(0, new Exception(this.Message()), string.Format("[派奖错误] OrderID:{0},Number:{1}", t.OrderID, t.Number));
                    }
                    openRewardResult[success]++;
                    sb.AppendFormat(":{0}\t", success);
                }
                catch (Exception ex)
                {
                    openRewardResult[false]++;
                    SystemAgent.Instance().AddErrorLog(0, ex, string.Format("[派奖错误] OrderID:{0},Number:{1}", t.OrderID, t.Number));
                }
            }
            SystemAgent.Instance().AddSystemLog(0, sb.ToString());
        }

        // 多线程派奖测测试类
        /*
        /// <summary>
        /// 多线程派奖
        /// </summary>
        /// <param name="task">要启用的线程数量</param>
        /// <param name="siteId">站点ID（所有站点使用0）</param>
        /// <param name="players">玩法字典</param>
        /// <param name="lotteryName">彩种名字字典</param>
        public void OpenReward(int task, int siteId, Dictionary<int, LotteryPlayer> players, Dictionary<string, string> lotteryName)
        {
            List<RewardOrder> list = this.GetRewardOrderList(siteId).OrderBy(t => Guid.NewGuid()).Take(1024).ToList();
            Dictionary<int, decimal> returnMoney = new Dictionary<int, decimal>();
            List<LotteryOrder> orderList = new List<LotteryOrder>();

            Console.Write("计算奖金：");
            int currentLeft = Console.CursorLeft;
            int count = 0;
            int total = list.Count;
            if (total == 0) return;

            // 多线程运行
            System.Threading.Tasks.Parallel.For(0, task, index =>
            {
                using (DbExecutor db = NewExecutor())
                {
                    foreach (RewardOrder rewardOrder in list.Where(t => t.OrderID % task == index))
                    {
                        try
                        {

                            LotteryOrder order = new LotteryOrder() { ID = rewardOrder.OrderID }.Info(db);
                            if (order.IsLottery || !players.ContainsKey(order.PlayerID)) continue;

                            LotteryType type = order.Type;
                            string number = rewardOrder.Number;
                            // 如果是官方彩种则需要判断是否手动开奖以及使用自定义号码
                            if (!type.GetCategory().SiteLottery)
                            {

                            }

                            LotteryPlayer player = players[order.PlayerID];
                            IPlayer playerObject = PlayerFactory.GetPlayer(player.Code, out type);
                            if (playerObject == null) continue;


                            decimal reward = playerObject.Reward(order.Number, number, player.Reward);

                            decimal betReturn;
                            if (!order.SetReward(reward, out betReturn))
                            {
                                continue;
                            }

                            order.ResultNumber = number;

                            orderList.Add(order);
                            if (!returnMoney.ContainsKey(order.ID))
                            {
                                returnMoney.Add(order.ID, betReturn);
                            }
                            count++;
                            Console.CursorLeft = currentLeft;
                            Console.Write("{0}/{1}", count, total);
                        }
                        catch (Exception ex)
                        {
                            Console.Write(ex);
                        }
                    }
                }
            });
            Console.WriteLine();
            Console.Write("运行派奖：");
            currentLeft = Console.CursorLeft;
            count = 0;
            // 运行派奖
            orderList.ForEach(t =>
            {
                count++;
                Console.CursorLeft = currentLeft;
                Console.Write("{0}/{1}", count, total);
                string key = string.Format("{0}-{1}", t.SiteID, t.Type);
                string name = lotteryName.ContainsKey(key) ? lotteryName[key] : t.Type.GetDescription();
                this.OpenReward(t, returnMoney[t.ID], name, players[t.PlayerID].Name);
            });
            Console.WriteLine();
        }

        /// <summary>
        /// 派奖
        /// </summary>
        /// <param name="order">已经设定了开奖值的订单</param>
        /// <param name="lotteryName">彩种名字</param>
        /// <param name="playerName">玩法名称</param>
        /// <returns></returns>
        private bool OpenReward(LotteryOrder order, decimal betReturn, string lotteryName, string playerName)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                try
                {
                    order.Update(db, t => t.Reward, t => t.IsLottery, t => t.Status, t => t.ResultNumber, t => t.LotteryAt);
                    Console.WriteLine("更新订单:{0}ms", sw.ElapsedMilliseconds);

                    //#1 解锁资金
                    if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.LotteryOrder, order.ID, "彩票开奖"))
                    {
                        db.Rollback();
                        base.Message("解锁资金发生错误 UserID:{0},Type:{1},SourceID:{2}", order.UserID, MoneyLock.LockType.LotteryOrder, order.ID);
                        return false;
                    }
                    Console.WriteLine("解锁资金:{0}ms", sw.ElapsedMilliseconds);

                    //#2 扣除用户资金（同步增加提现流水）
                    if (!UserAgent.Instance().AddMoneyLog(db, order.UserID, Math.Abs(order.Money) * -1, MoneyLog.MoneyType.Bet, order.ID,
                        string.Format("{0} {1}，单号[{2}]", lotteryName, playerName, order.ID)))
                    {
                        db.Rollback();
                        base.Message("扣款错误");
                        return false;
                    }
                    Console.WriteLine("扣除用户资金:{0}ms", sw.ElapsedMilliseconds);

                    //#3 返点
                    if (betReturn > decimal.Zero)
                    {
                        UserAgent.Instance().AddMoneyLog(db, order.UserID, betReturn, MoneyLog.MoneyType.Rebate, order.ID,
                            string.Format("投注单号[{0}]返点", order.ID));
                    }
                    Console.WriteLine("返点:{0}ms", sw.ElapsedMilliseconds);

                    //#4 奖金派发
                    if (order.Reward > decimal.Zero)
                    {
                        UserAgent.Instance().AddMoneyLog(db, order.UserID, order.Reward, MoneyLog.MoneyType.Reward, order.ID,
                            string.Format("{0} {1}，单号[{2}]", lotteryName, playerName, order.ID));
                    }
                    Console.WriteLine("奖金派发:{0}ms", sw.ElapsedMilliseconds);

                    //#5 上级返点
                    SiteAgent.Instance()._lotteryBetAgent(db, order.ID);
                    Console.WriteLine("上级返点:{0}ms", sw.ElapsedMilliseconds);

                    //#6 如果是追号订单
                    if (order.ChaseID != 0)
                    {
                        this.UpdateChaseOrder(db, order.ChaseID, order.Index, order.Reward, order.Money);
                    }

                    Console.WriteLine("如果是追号订单:{0}ms", sw.ElapsedMilliseconds);

                    //#7 发送中奖通知
                    UserAgent.Instance().AddNotify(db, order.UserID, UserNotify.NotifyType.Lottery,
                        "{0}{1}期投注单号{2}已派奖，本期盈利{3}元", this.GetLotteryName(order.Type, order.SiteID), order.Index, order.ID, (order.Reward - order.Money).ToString("c"));

                    Console.WriteLine("发送中奖通知:{0}ms", sw.ElapsedMilliseconds);

                    db.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    db.Rollback();
                    return false;
                }
                finally
                {
                    Console.WriteLine("OrderID:{0}", order.ID);
                }
            }
        }
        */

        #endregion

        #region =============== 自动撤单 ================

        private static Dictionary<LotteryType, int> _revokeType = null;
        /// <summary>
        /// 自动撤单
        /// </summary>
        public void Revoke()
        {
            if (_revokeType == null)
            {
                _revokeType = new Dictionary<LotteryType, int>();
                foreach (LotteryType type in Enum.GetValues(typeof(LotteryType)))
                {
                    if (type.GetCategory().Revoke != 0)
                    {
                        _revokeType.Add(type, type.GetCategory().Revoke);
                    }
                }
            }
            if (_revokeType.Count == 0) return;

            List<string> result = new List<string>();
            result.Add("系统自动撤单");
            foreach (LotteryType type in _revokeType.Keys)
            {
                DateTime startTime = DateTime.Now.AddSeconds(_revokeType[type] * -1);
                DateTime resultTime = startTime.AddHours(-1);
                List<Tuple<int, int>> orders = new List<Tuple<int, int>>();
                using (DbExecutor db = NewExecutor())
                {
                    DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "data_GetRevokeOrder",
                        NewParam("@Type", type),
                        NewParam("@Time", _revokeType[type]));
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        orders.Add(new Tuple<int, int>((int)dr["OrderID"], (int)dr["UserID"]));
                    }

                }
                //var orders = BDC.LotteryOrder.Where(t => t.Status == LotteryOrder.OrderStatus.Normal && t.Type == type && t.CreateAt < startTime &&
                //    !BDC.ResultNumber.Where(p => p.Type == type && p.ResultAt > resultTime).Select(p => p.Index).Contains(t.Index)).Select(t => new { t.ID, t.UserID }).ToArray();
                foreach (var order in orders)
                {
                    int orderId = order.Item1;
                    int uesrId = order.Item2;

                    this.MessageClean();
                    if (this.Revoke(orderId, uesrId))
                    {
                        result.Add(string.Format("撤单成功，编号：{0}", orderId));
                    }
                    else
                    {
                        result.Add(string.Format("撤单失败，编号：{0}，原因：{1}", orderId, this.Message()));
                    }
                }
            }
            if (result.Count != 0)
            {
                SystemAgent.Instance().AddSystemLog(0, string.Join("\n", result));
            }
        }

        #endregion

        private Dictionary<string, List<BetAgentRate>> _betAgentRate = new Dictionary<string, List<BetAgentRate>>();

        /// <summary>
        /// 运行代理返点
        /// </summary>
        public int RunBetAgent()
        {
            DataRowCollection loglist;
            using (DbExecutor db = NewExecutor())
            {
                loglist = db.GetDataSet(CommandType.StoredProcedure, "GetBetAgentList").Tables[0].Rows;
            }

            int count = 0;
            foreach (DataRow dr in loglist)
            {
                count++;
                int userId = (int)dr["UserID"];
                int orderId = (int)dr["OrderID"];
                decimal money = Math.Abs((decimal)dr["Money"]);
                DateTime createAt = (DateTime)dr["CreateAt"];
                int rebate = (int)dr["Rebate"];

                List<BetAgentRate> list = this.GetBetAgentRate(userId, rebate);

                using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
                {
                    if (list != null)
                    {
                        list.ForEach(t =>
                        {
                            decimal rateMoney = money * t.Rate;
                            if (rateMoney != decimal.Zero)
                            {
                                UserAgent.Instance().AddMoneyLog(t.UserID, rateMoney, MoneyLog.MoneyType.BetAgent, orderId, string.Format("下级投注返点，单号：{0}", orderId));
                            }
                        });
                    }

                    db.ExecuteNonQuery(CommandType.Text, "UPDATE sys_Mark SET BetAgent = @OrderID,BetAgentAt = @BetAgentAt",
                       NewParam("@OrderID", orderId),
                       NewParam("@BetAgentAt", createAt));

                    db.ExecuteNonQuery(CommandType.Text, "UPDATE lot_BetAgent SET IsBet = 1 WHERE OrderID = @OrderID",
                        NewParam("@OrderID", orderId));

                    db.Commit();
                }
            }
            return count;
        }

        /// <summary>
        /// 获取上级的返点比例
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userRebate">订单所使用的返点</param>
        /// <returns></returns>
        private List<BetAgentRate> GetBetAgentRate(int userId, int userRebate = 0)
        {
            string key = string.Concat(userId, "-", userRebate);

            if (_betAgentRate.ContainsKey(key)) return _betAgentRate[key];
            List<BetAgentRate> list = new List<BetAgentRate>();
            using (DbExecutor db = NewExecutor())
            {
                // RETURN  UserID,Rebate
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetUserParentRebate",
                    NewParam("@UserID", userId));

                int rebate = 0;
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int newRebate = (int)dr["Rebate"];
                    if (rebate == 0)
                    {
                        rebate = userRebate == 0 ? newRebate : userRebate;
                        continue;
                    }

                    // 如果出现下级返点比上级高的情况则立刻返回空值
                    if (newRebate < rebate) return new List<BetAgentRate>();

                    decimal rate = (decimal)(newRebate - rebate) / 2000M;
                    rebate = newRebate;

                    if (rebate != decimal.Zero)
                    {
                        list.Add(new BetAgentRate((int)dr["UserID"], rate));
                    }
                }

                _betAgentRate[key] = list.Count == 0 ? null : list;
            }
            return list;
        }
    }
}
