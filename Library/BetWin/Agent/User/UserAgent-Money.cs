using BW.Common.Admins;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Framework;
using BW.GateWay.Payment;
using BW.GateWay.Withdraw;
using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Data.Linq;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BW.Agent
{
    /// <summary>
    /// 用户操作资金相关
    /// </summary>
    public partial class UserAgent
    {
        /// <summary>
        /// 获取用户的可用余额（适用于非web程序）
        /// </summary>
        public decimal GetUserMoney(int userId, DbExecutor db = null)
        {
            return this.GetTotalMoney(userId, db).Money;
        }

        /// <summary>
        /// 获取用户的全部资金（无缓存）
        /// 如果存在用户缓存则更新该用户的资金缓存
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserMoney GetTotalMoney(int userId, DbExecutor db = null)
        {
            bool isNewExecutor;
            db = this.GetNewExecutor(db, out isNewExecutor);

            DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetUserMoney",
                  NewParam("@SiteID", this.GetSiteID(userId, db)),
                  NewParam("@UserID", userId));
            if (isNewExecutor) db.Dispose();

            UserMoney userMoney = new UserMoney(ds);
            if (UserInfo != null && UserInfo.ID == userId)
            {
                userMoney.Update(UserInfo);
            }
            return userMoney;
        }


        /// <summary>
        /// 锁定用户的资金（解锁资金加到用户余额上，不经过MoneyLog表）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="money">金额</param>
        /// <param name="db">事物操作对象</param>
        /// <param name="type">锁定类型</param>
        /// <param name="sourceId">关联对象ID</param>
        /// <param name="description">锁定备注信息</param>
        /// <returns></returns>
        public bool LockMoney(DbExecutor db, int userId, decimal money, MoneyLock.LockType type, int sourceId, string description)
        {
            if (money < 0) { base.Message("金额错误"); return false; }

            if (money > this.GetUserMoney(userId, db))
            {
                base.Message("待锁定余额不足");
                return false;
            }

            try
            {
                int siteId = this.GetSiteID(userId, null);
                MoneyLock moneyLock = new MoneyLock()
                {
                    SiteID = siteId,
                    UserID = userId,
                    Money = money,
                    Type = type,
                    SourceID = sourceId,
                    Description = description,
                    LockAt = DateTime.Now,
                };

                if (!moneyLock.Add(db))
                {
                    return false;
                }

                if (db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET Money = Money - @Money,LockMoney = LockMoney + @Money WHERE SiteID = @SiteID AND UserID = @UserID AND Money >= @Money",
                    NewParam("@Money", money),
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId)) != 1)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                base.Message(ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 锁定用户资金（内置事物）
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="money"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool LockMoney(int userid, decimal money, MoneyLock.LockType type, int sourceId, string description)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (this.LockMoney(db, userid, money, type, sourceId, description))
                {
                    db.Commit();
                    return true;
                }
                db.Rollback();
                return false;
            }
        }

        /// <summary>
        /// 解锁资金（解锁资金加到用户余额上，不经过MoneyLog表）
        /// </summary>
        /// <param name="db">数据库操作对象</param>
        /// <param name="userId">用户ID （如果SiteInfo为空，则根据用户ID获取SiteID）</param>
        /// <param name="type">类型</param>
        /// <param name="sourceId">来源ID （根据类型与来源ID找到锁定记录）</param>
        /// <param name="unlockDesc">解锁备注</param>
        /// <returns></returns>
        public bool UnlockMoney(DbExecutor db, int userId, MoneyLock.LockType type, int sourceId, string unlockDesc)
        {
            int siteId = this.GetSiteID(userId, db);

            MoneyLock moneyLock = new MoneyLock()
            {
                SiteID = siteId,
                UserID = userId,
                Type = type,
                SourceID = sourceId
            }.Info(db, t => t.SiteID, t => t.UserID, t => t.Type, t => t.SourceID);
            if (moneyLock == null || moneyLock.UnLockAt.Year > 2000) return false;

            moneyLock.UnLockAt = DateTime.Now;
            moneyLock.UnLockDesc = unlockDesc;
            moneyLock.Update(db, t => t.UnLockAt, t => t.UnLockDesc);


            if (db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET Money = Money + @Money,LockMoney = LockMoney - @Money WHERE SiteID = @SiteID AND UserID = @UserID AND LockMoney >= @Money",
                  NewParam("@Money", moneyLock.Money),
                  NewParam("@SiteID", siteId),
                  NewParam("@UserID", userId)) == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 解锁资金（新建Db对象）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <param name="unlockDesc"></param>
        /// <returns></returns>
        public bool UnlockMoney(int userId, MoneyLock.LockType type, int sourceId, string unlockDesc)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (this.UnlockMoney(db, userId, type, sourceId, unlockDesc))
                {
                    db.Commit();
                    return true;
                }
                else
                {
                    db.Rollback();
                    return false;
                }
            }
        }

        /// <summary>
        /// 管理员进行解锁资金操作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool UnlockMoney(int id)
        {
            MoneyLock moneyLock = new MoneyLock() { ID = id }.Info();
            if (moneyLock == null || moneyLock.SiteID != SiteInfo.ID)
            {
                base.Message("编号错误");
                return false;
            }
            if (moneyLock.UnLockAt.Year > 2000)
            {
                base.Message("该笔资金已经解锁");
                return false;
            }
            if (this.UnlockMoney(moneyLock.UserID, moneyLock.Type, moneyLock.SourceID, string.Format("管理员{0}解锁", AdminInfo.Name)))
            {
                AdminInfo.Log(AdminLog.LogType.Money, "对用户{0}的锁定资金解锁，类型：{1},金额:{2},来源编号:{3}", this.GetUserName(moneyLock.UserID), moneyLock.Type.GetDescription(), moneyLock.Money.ToString("n"), moneyLock.SourceID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 扣除或者增加用户款项（可用于非web程序）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId">用户ID</param>
        /// <param name="money">金额（正数增加，负数减少）</param>
        /// <param name="type">类型</param>
        /// <param name="sourceId">来源ID（与类型组成唯一值，数据库中有索引）</param>
        /// <param name="description">备注信息</param>
        /// <returns></returns>
        public bool AddMoneyLog(DbExecutor db, int userId, decimal money, MoneyLog.MoneyType type, int sourceId, string description)
        {
            int siteId = this.GetSiteID(userId, db);
            UserMoney userMoney = this.GetTotalMoney(userId, db);

            if (money < decimal.Zero && userMoney.Money + money < decimal.Zero)
            {
                base.Message("余额不足");
                return false;
            }

            MoneyLog moneyLog = new MoneyLog()
            {
                SiteID = siteId,
                UserID = userId,
                CreateAt = DateTime.Now,
                Description = description,
                Balance = userMoney.Balance + money,
                IP = IPAgent.IP,
                Money = money,
                SourceID = sourceId,
                Type = type
            };
            try
            {
                if (db.ExecuteNonQuery(CommandType.StoredProcedure, "InsertMoneyLog",
                    NewParam("@SiteID", moneyLog.SiteID),
                    NewParam("@UserID", moneyLog.UserID),
                    NewParam("@Money", moneyLog.Money),
                    NewParam("@Balance", moneyLog.Balance),
                    NewParam("@IP", moneyLog.IP),
                    NewParam("@Type", moneyLog.Type),
                    NewParam("@SourceID", moneyLog.SourceID),
                    NewParam("@Description", moneyLog.Description)) == 0)
                {
                    return false;
                }
                if (db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET Money = Money + @Money WHERE SiteID = @SiteID AND UserID = @UserID AND (@Money >= 0 OR Money + @Money >= 0)",
                    NewParam("@Money", money),
                    NewParam("@SiteID", siteId),
                    NewParam("@UserID", userId)) == 0)
                {
                    base.Message("余额不足");
                    return false;
                }

                Site.SiteSetting setting = SiteInfo == null ? SiteAgent.Instance().GetSiteSetting(siteId, db) : SiteInfo.Setting;
                Site.MoneyTypeSetting moneySetting = setting.MoneyTypeSetting.Find(t => t.ID == (int)type);

                if (moneySetting.NoTrunover && money > 0)
                {
                    if (!this.WithdrawLog(db, userId, money, description))
                    {
                        return false;
                    }
                }

                if (type == MoneyLog.MoneyType.Bet && setting.Turnover != decimal.Zero)
                {
                    if (!this.WithdrawLog(db, userId, Math.Abs(money) / setting.Turnover, description))
                    {
                        return false;
                    }
                }

                // 保存用户的日流水统计报表
                // 2017.07.27 修改至生成历史报表的时候才生成
                //db.ExecuteNonQuery(CommandType.StoredProcedure, "data_SaveUserDateReport",
                //    NewParam("@SiteID", siteId),
                //    NewParam("@UserID", userId),
                //    NewParam("@Date", moneyLog.CreateAt.Date),
                //    NewParam("@Type", moneyLog.Type),
                //    NewParam("@Money", moneyLog.Money));
            }
            catch (Exception ex)
            {
                base.Message(ex.Message);
                SystemAgent.Instance().AddErrorLog(siteId, ex, string.Format("AddMoneyLog({0},{1},{2},{3},{4})", userId, money, type, sourceId, WebAgent.Left(description, 50)));
                return false;
            }

            return true;
        }

        /// <summary>
        /// 直接操作用户资金
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool AddMoneyLog(int userId, decimal money, MoneyLog.MoneyType type, int sourceId, string description)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                if (!this.AddMoneyLog(db, userId, money, type, sourceId, description))
                {
                    db.Rollback();
                    return false;
                }

                db.Commit();
                return true;
            }
        }


        /// <summary>
        /// 判断资金记录是否存在
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public bool ExistsMoneyLog(DbExecutor db, int userId, MoneyLog.MoneyType type, int sourceId)
        {
            int siteId = this.GetSiteID(userId, db);
            return new MoneyLog() { SiteID = siteId, UserID = userId, Type = type, SourceID = sourceId, TableID = userId.GetTableID() }.Exists(db, t => t.SiteID, t => t.UserID, t => t.Type, t => t.SourceID, t => t.TableID);
        }

        /// <summary>
        /// 判断资金记录是否存在（自带数据库对象，适用于非Web程序）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public bool ExistsMoneyLog(int userId, MoneyLog.MoneyType type, int sourceId)
        {
            using (DbExecutor db = NewExecutor())
            {
                return this.ExistsMoneyLog(db, userId, type, sourceId);
            }
        }

        /// <summary>
        /// 根据类型，来源获取
        /// 如果SiteID 不等于0则支持非web程序
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public MoneyLog GetMoneyLogInfo(int userId, MoneyLog.MoneyType type, int sourceId, int siteId = 0)
        {
            if (siteId == 0) siteId = SiteInfo.ID;

            return BDC.MoneyLog.Where(t => t.SiteID == siteId && t.TableID == userId.GetTableID() && t.UserID == userId && t.Type == type && t.SourceID == sourceId).FirstOrDefault();
        }

        /// <summary>
        /// 获取来源
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public MoneyLog GetMoneyLogInfo(DbExecutor db, int userId, MoneyLog.MoneyType type, int sourceId, int siteId = 0)
        {
            if (siteId == 0) siteId = this.GetSiteID(userId);

            return new MoneyLog()
            {
                SiteID = siteId,
                UserID = userId,
                Type = type,
                SourceID = sourceId,
                TableID = userId.GetTableID()
            }.Info(db, t => t.SiteID, t => t.UserID, t => t.Type, t => t.SourceID);
        }

        /// <summary>
        /// 获取相同来源的资金记录数量（web程序专用）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="sourceId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetMoneyLogCount(int sourceId, params MoneyLog.MoneyType[] type)
        {
            using (DbExecutor db = NewExecutor())
            {
                return this.GetMoneyLogCount(db, SiteInfo.ID, sourceId, type);
            }
        }

        /// <summary>
        /// 获取相同来源的资金记录数量
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public int GetMoneyLogCount(int siteId, int sourceId, params MoneyLog.MoneyType[] type)
        {
            using (DbExecutor db = NewExecutor())
            {
                return this.GetMoneyLogCount(db, siteId, sourceId, type);
            }
        }

        public int GetMoneyLogCount(DbExecutor db, int siteId, int sourceId, params MoneyLog.MoneyType[] type)
        {
            if (siteId == 0 && SiteInfo != null) siteId = SiteInfo.ID;
            if (type.Length == 1)
            {
                return (int)db.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(*) FROM {0} WHERE SiteID = @SiteID AND SourceID = @SourceID AND Type = @Type",
                    typeof(MoneyLog).GetTableName()),
                    NewParam("@SiteID", siteId),
                    NewParam("@SourceID", sourceId),
                    NewParam("@Type", type[0]));
            }
            else
            {
                return (int)db.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(*) FROM {0} WHERE SiteID = @SiteID AND SourceID = @SourceID AND Type IN ({1})",
                    typeof(MoneyLog).GetTableName(), string.Join(",", type.Select(t => (byte)t))),
                NewParam("@SiteID", siteId),
                NewParam("@SourceID", sourceId));
            }
        }

        /// <summary>
        /// 提现额度的变化（日志+写入User表）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userid"></param>
        /// <param name="withdraw">本次需要变化的提现额度</param>
        /// <param name="description"></param>
        /// <returns></returns>
        public bool WithdrawLog(DbExecutor db, int userId, decimal withdraw, string description)
        {
            int siteId = this.GetSiteID(userId, db);
            UserMoney money = this.GetTotalMoney(userId, db);

            WithdrawLog log = new WithdrawLog()
            {
                SiteID = siteId,
                CreateAt = DateTime.Now,
                Withdraw = withdraw,
                Description = description,
                UserID = userId,
                Balance = Math.Max(decimal.Zero, Math.Min(money.Money, money.Withdraw + withdraw))
            };

            if (!log.Add(db)) return false;

            if (db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET Withdraw = @Money WHERE SiteID = @SiteID AND UserID = @UserID",
                NewParam("@Money", log.Balance),
                NewParam("@SiteID", siteId),
                NewParam("@UserID", userId)) != 1) return false;

            return true;
        }

        private object _rechargeLockObject = new object();
        /// <summary>
        /// 用户提交充值
        /// </summary>
        /// <returns>是否提交成功</returns>
        public bool Recharge(int userId, int payId, decimal money, string bankCode = null)
        {
            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(payId);
            if (payment == null || !payment.IsOpen)
            {
                base.Message("支付接口错误");
                return false;
            }
            long orderId = this.CreateRechargeOrder(userId, payId, money);
            if (orderId == 0) return false;
            lock (_rechargeLockObject)
            {
                IPayment gateway = payment.PaymentObject;
                gateway.OrderID = orderId.ToString();
                gateway.Money = money;
                gateway.Name = this.GetUserName(userId);
                gateway.GoGateway();
                return true;
            }
        }

        /// <summary>
        /// 提交订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool Recharge(long orderId)
        {
            lock (_rechargeLockObject)
            {
                RechargeOrder order = this.GetRechargeOrderInfo(orderId);
                if (order == null || order.IsPayment || order.CreateAt < DateTime.Now.AddDays(-12))
                {
                    base.Message("订单错误");
                    return false;
                }

                PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(order.PayID);
                if (payment == null || !payment.IsOpen)
                {
                    base.Message("支付接口错误");
                    return false;
                }

                IPayment gateway = payment.PaymentObject;
                gateway.OrderID = orderId.ToString();
                gateway.Money = order.Money;
                gateway.Name = UserAgent.Instance().GetUserName(order.UserID);
                gateway.GoGateway();
                return true;
            }
        }

        /// <summary>
        /// 修改充值金额
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        internal bool UpdateRechargeMoney(long orderId, decimal money)
        {
            RechargeOrder order = new RechargeOrder()
            {
                ID = orderId,
                Money = money
            };
            return order.Update(null, t => t.Money) != 0;
        }

        /// <summary>
        /// 创建一个充值订单号
        /// 2017.4.11 新增如果该充值渠道方式不产生订单直接返回payid
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="payId">支付接口</param>
        /// <param name="money">充值金额</param>
        /// <param name="createOrder">强制要求创建订单</param>
        /// <returns></returns>
        public long CreateRechargeOrder(int userId, int payId, decimal money, string description = null, bool createOrder = false)
        {
            if (userId == 0)
            {
                base.Message("用户名错误");
                return 0;
            }
            if (this.CheckUserLockStatus(userId, User.LockStatus.Recharge))
            {
                base.Message("当前账户禁止充值");
                return 0;
            }
            if (money <= 0)
            {
                base.Message("金额错误");
                return 0;
            }
            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(payId);
            if (money < payment.MinMoney || money > payment.MaxMoney)
            {
                base.Message("充值金额不在该渠道的允许范围内");
                return 0;
            }
            if (payment == null)
            {
                base.Message("充值渠道错误");
                return 0;
            }
            if (SiteInfo.Setting.RechargeNeedBank && this.GetBankAccountList(userId).Count == 0)
            {
                base.Message("需绑定银行卡后才可充值");
                return 0;
            }

            RechargeOrder order = new RechargeOrder();
            while (order.ID == 0 || order.Exists())
            {
                order.ID = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")) * 1000 + WebAgent.GetRandom(0, 1000);
            }
            order.SiteID = SiteInfo.ID;
            order.UserID = userId;
            order.CreateAt = DateTime.Now;
            order.Money = money;
            order.PayID = payId;
            order.Description = description;

            if (order.Add())
            {
                return order.ID;
            }

            return 0;
        }

        /// <summary>
        /// 申请提现
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="bankId"></param>
        /// <param name="payPassword"></param>
        /// <param name="withdrawFee">手续费</param>
        /// <param name="withdrawFeeDesc">手续费说明</param>
        /// <param name="appointment">预约提现的时间</param>
        /// <param name="source">提现来源</param>
        /// <param name="proc">处理撤单时候的自定义存储过程</param>
        /// <returns></returns>
        public bool Withdraw(int userId, decimal money, int bankId, string payPassword, decimal withdrawFee = decimal.Zero, string withdrawFeeDesc = null, DateTime? appointment = null, string source = null, string proc = null)
        {
            if (!this.CheckLogin(userId)) return false;

            if (Math.Round(money) != money)
            {
                base.Message("提现金额不能有小数");
                return false;
            }

            if (this.CheckUserLockStatus(userId, User.LockStatus.Withdraw))
            {
                base.Message("当前账户禁止提现");
                return false;
            }

            if (this.CheckUserLockStatus(userId, User.LockStatus.Contract))
            {
                base.Message("您有契约转账尚未完成");
                return false;
            }

            if (money < SiteInfo.Setting.WithdrawMin || money > SiteInfo.Setting.WithdrawMax)
            {
                base.Message("单笔提现金额在{0}～{1}之间", SiteInfo.Setting.WithdrawMin.ToString("c"), SiteInfo.Setting.WithdrawMax.ToString("c"));
                return false;
            }

            if (money > this.GetUserMoney(userId))
            {
                base.Message("提现金额大于可用余额");
                return false;
            }

            if (SiteInfo.Setting.Turnover != decimal.Zero && withdrawFee == decimal.Zero && money > UserInfo.Withdraw)
            {
                base.Message("提现金额大于可提现金额");
                return false;
            }

            if (money <= withdrawFee || withdrawFee < decimal.Zero)
            {
                base.Message("手续费错误");
                return false;
            }

            if (!SiteInfo.Setting.IsWithdrawTime)
            {
                base.Message("平台提款时间为{0}", SiteInfo.Setting.WithdrawTime);
                return false;
            }

            WithdrawOrder.WithdrawStatus[] faildStatus = new WithdrawOrder.WithdrawStatus[] { WithdrawOrder.WithdrawStatus.Faild, WithdrawOrder.WithdrawStatus.Return, WithdrawOrder.WithdrawStatus.Split };
            if (SiteInfo.Setting.WithdrawCount != 0 &&
                BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && !faildStatus.Contains(t.Status) && t.CreateAt > DateTime.Now.Date).Count() >= SiteInfo.Setting.WithdrawCount)
            {
                base.Message("单日提现次数不能超过{0}次", SiteInfo.Setting.WithdrawCount);
                return false;
            }

            if (!this.CheckPayPassword(userId, payPassword)) return false;

            if (bankId == 0)
            {

            }

            BankAccount bank = this.GetBankAccountInfo(bankId);
            if (bank == null || bank.UserID != userId)
            {
                base.Message("提现银行错误");
                return false;
            }
            if (!SiteInfo.Setting.WithdrawBankList.Contains(bank.Type))
            {
                base.Message("系统暂不支持{0}提现", bank.Type.GetDescription());
                return false;
            }
            if (!bank.IsWithdraw)
            {
                base.Message("当前提现帐号需绑定超过{0}小时方可使用", SiteInfo.Setting.CardTime);
                return false;
            }

            if (BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Status == WithdrawOrder.WithdrawStatus.None).Count() != 0)
            {
                base.Message("您有暂未处理的提现订单");
                return false;
            }

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                WithdrawOrder order = new WithdrawOrder()
                {
                    SiteID = SiteInfo.ID,
                    UserID = userId,
                    CreateAt = DateTime.Now,
                    Description = "申请提现",
                    Money = money - withdrawFee,
                    Fee = withdrawFee,
                    Status = WithdrawOrder.WithdrawStatus.None,
                    Bank = bank.Type,
                    BankName = string.IsNullOrEmpty(bank.Bank) ? bank.Type.GetDescription() : bank.Bank,
                    AccountName = UserInfo.AccountName,
                    AccountNumber = bank.Account,
                    Appointment = appointment == null ? DateTime.Now : appointment.Value,
                    Source = source,
                    SourceProc = proc
                };

                if (!order.Add(true, db))
                {
                    db.Rollback();
                    return false;
                }

                if (!this.LockMoney(db, userId, money - withdrawFee, MoneyLock.LockType.Withdraw, order.ID, "申请提现"))
                {
                    db.Rollback();
                    return false;
                }

                if (withdrawFee > decimal.Zero && !this.LockMoney(db, userId, withdrawFee, MoneyLock.LockType.WithdrawFee, order.ID, withdrawFeeDesc))
                {
                    db.Rollback();
                    return false;
                }

                if (!this.WithdrawLog(db, userId, money * -1, "申请提现"))
                {
                    db.Rollback();
                    return false;
                }

                db.Commit();
                return true;
            }


        }

        #region ============ 批量自动出款和状态检查  ==============

        /// <summary>
        /// 检查出款状态、处理批量出款的锁
        /// </summary>
        private const string _checkWithdrawStatus = "_checkWithdrawStatus";

        /// <summary>
        /// 检查所有使用第三方出款接口，状态为正在出款中 或者状态为待处理但是已标记出款接口（非Web，定时任务器内执行）
        /// </summary>
        internal void CheckWithdrawStatus()
        {
            lock (_checkWithdrawStatus)
            {
                var list = BDC.WithdrawOrder.Where(t => t.Status == WithdrawOrder.WithdrawStatus.Success);
                if (list.Count() == 0) return;

                List<string> result = new List<string>();
                result.Add("提现订单查询结果");
                Dictionary<int, WithdrawSetting> settingList = BDC.WithdrawSetting.ToDictionary(t => t.ID, t => t);

                foreach (WithdrawOrder order in list)
                {
                    if (!settingList.ContainsKey(order.WithdrawSettingID) || settingList[order.WithdrawSettingID].Type == WithdrawType.Manually) continue;

                    DateTime now = DateTime.Now;

                    this.MessageClean();
                    try
                    {
                        WithdrawSetting setting = settingList[order.WithdrawSettingID];

                        IWithdraw withdraw = WithdrawFactory.CreateWithdraw(setting.Type, setting.SettingString);
                        string msg;
                        WithdrawStatus status = withdraw.Query(order.ID.ToString(), out msg);

                        result.Add(string.Format("订单{0}查询结果：{1}", order.ID, status));
                        switch (status)
                        {
                            case WithdrawStatus.Return:
                                order.Status = WithdrawOrder.WithdrawStatus.Return;
                                this.CheckWithdrawStatus(order, msg);
                                break;
                            case WithdrawStatus.Success:
                                order.Status = WithdrawOrder.WithdrawStatus.Finish;
                                this.CheckWithdrawStatus(order, msg);
                                break;
                            case WithdrawStatus.Paymenting:
                                if (order.Status == WithdrawOrder.WithdrawStatus.None)
                                {
                                    order.Status = WithdrawOrder.WithdrawStatus.Success;
                                    this.CheckWithdrawStatus(order, msg);
                                }
                                break;
                        }
                        if (status == WithdrawStatus.Error)
                        {
                            SystemAgent.Instance().AddSystemLog(0, string.Format("提现订单{0}查询出错，{1}", order.ID, msg));
                        }

                        result.Add(this.Message());

                    }
                    catch (Exception ex)
                    {
                        result.Add(string.Format("订单{0}出错，{1}", order.ID, ex.Message));
                    }
                    System.Threading.Thread.Sleep(2000);
                }

                SystemAgent.Instance().AddSystemLog(0, string.Join("  \n", result));
            }
        }

        /// <summary>
        /// 处理标记为自动出款的订单（非Web，定时任务器内执行）
        /// </summary>
        /// <returns></returns>
        internal void ExecWithdrawOrder()
        {
            lock (_checkWithdrawStatus)
            {
                StringBuilder log = new StringBuilder();
                int[] list = BDC.WithdrawOrder.Where(t => t.Status == WithdrawOrder.WithdrawStatus.None && t.WithdrawSettingID != 0 && !t.IsManual).Select(t => t.ID).ToArray();
                if (list.Length == 0) return;
                foreach (int orderId in list)
                {
                    DateTime now = DateTime.Now;
                    this.ExecWithdrawOrder(orderId);
                    // 如果间隔周期小于1.5s则空转一下
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 处理单条批量提现
        /// </summary>
        /// <param name="orderId"></param>
        private void ExecWithdrawOrder(int orderId)
        {
            WithdrawOrder order = BDC.WithdrawOrder.Where(t => t.ID == orderId).FirstOrDefault();

            if (order.Status != WithdrawOrder.WithdrawStatus.None || order.IsManual)
            {
                SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款] 订单状态错误：{0}:{1}", order.ID, order.Status.GetDescription()));
                return;
            }

            WithdrawSetting setting = SiteAgent.Instance().GetWithdrawSettingInfo(order.SiteID, order.WithdrawSettingID);
            if (setting == null || setting.Type == WithdrawType.Manually || !setting.IsOpen)
            {
                if (setting == null)
                {
                    SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款] 提现接口错误：{0} WithdrawSettingID:{1}", order.ID, order.WithdrawSettingID));
                }
                else
                {
                    SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款] 提现接口错误：{0} setting.Type:{1} setting.IsOpen:{2}", order.ID, setting.Type, setting.IsOpen));
                }
                return;
            }

            IWithdraw withdraw = WithdrawFactory.CreateWithdraw(setting.Type, setting.SettingString);
            if (withdraw == null)
            {
                SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款] 提现接口为空：{0}", order.ID));
                return;
            }
            // 自动付款接口
            string msg;
            withdraw.BankCode = order.Bank;
            withdraw.Account = order.AccountName;
            withdraw.CardNo = order.AccountNumber;
            withdraw.Money = order.Money;
            withdraw.OrderID = order.ID.ToString();
            try
            {
                bool success = withdraw.Remit(out msg);

                SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款] 接口处理结果：{0} {1}:{2}", order.ID, success, msg));

                using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                {
                    if (success)
                    {
                        if (!this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Success, "批量出款"))
                        {
                            SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款警告] {0} 接口成功，但是标记状态错误", order.ID));
                            db.Rollback();
                            return;
                        }
                    }
                    else
                    {
                        if (!this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.None, WebAgent.Left(msg, 90)))
                        {
                            db.Rollback();
                            return;
                        }
                    }

                    if (!success)
                    {
                        order.IsManual = true;
                        order.Update(db, t => (object)t.IsManual);

                        SystemAgent.Instance().AddSystemLog(order.SiteID, string.Format("[批量出款警告] {0} 处理失败，需手工处理。{1}", order.ID, msg));
                    }
                    db.Commit();
                }

            }
            catch (Exception ex)
            {
                SystemAgent.Instance().AddErrorLog(order.SiteID, ex);
            }
        }

        /// <summary>
        /// 检查状态之后根据状态类型进行处理(成功、失败)
        /// </summary>
        /// <param name="order">本地订单状态</param>
        /// <param name="msg">远程网关返回的信息</param>
        private void CheckWithdrawStatus(WithdrawOrder order, string msg)
        {
            if (!new WithdrawOrder.WithdrawStatus[] { WithdrawOrder.WithdrawStatus.Return, WithdrawOrder.WithdrawStatus.Finish }.Contains(order.Status))
            {
                return;
            }
            string message = string.Empty;
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.Withdraw, order.ID, string.Format("提现状态：{0} {1}", order.Status.GetDescription(), msg)))
                {
                    db.Rollback();
                    return;
                }

                if (order.Fee > decimal.Zero)
                {
                    if (!UserAgent.Instance().UnlockMoney(db, order.UserID, MoneyLock.LockType.WithdrawFee, order.ID, string.Format("提现状态：{0} {1}", order.Status.GetDescription(), msg)))
                    {
                        db.Rollback();
                        return;
                    }
                }

                switch (order.Status)
                {
                    case WithdrawOrder.WithdrawStatus.Return:
                        message = string.Format("您的提现申请被拒绝，提现编号：{0}，拒绝原因：{1}", order.ID, msg);
                        break;
                    case WithdrawOrder.WithdrawStatus.Finish:
                        if (!UserAgent.Instance().AddMoneyLog(db, order.UserID, order.Money * -1, MoneyLog.MoneyType.Withdraw, order.ID, "提现到账"))
                        {
                            db.Rollback();
                            return;
                        }
                        if (order.Fee > decimal.Zero)
                        {
                            if (!UserAgent.Instance().AddMoneyLog(db, order.UserID, order.Fee * -1, MoneyLog.MoneyType.WithdrawFee, order.ID, "提现扣除手续费"))
                            {
                                db.Rollback();
                                return;
                            }
                        }
                        message = string.Format("您的提现已提交银行处理，请注意查收银行收款信息。提现编号：{0}，到账时间：{1}", order.ID, DateTime.Now);
                        break;
                }
                if (!this.AddWithdrawOrderLog(db, order, order.Status, msg))
                {
                    db.Rollback();
                    return;
                }

                db.Commit();
            }

            UserAgent.Instance().SendMessage(order.UserID, null, message);
        }

        #endregion


        /// <summary>
        /// 获取用户当日的提现次数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int GetWithdrawCount(int userId)
        {
            return BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.CreateAt > DateTime.Now.Date).Count();
        }

        /// <summary>
        /// 获取提现订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public WithdrawOrder GetWithdrawOrderInfo(int orderId)
        {
            return BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.ID == orderId).FirstOrDefault();
        }

        /// <summary>
        /// 根据订单编号获取提交的时间
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public DateTime GetWithdrawOrderDate(string orderId)
        {
            int id;
            if (!int.TryParse(orderId, out id)) return DateTime.MinValue;
            DateTime? createAt = BDC.WithdrawOrderLog.Where(t => t.WithdrawID == id && t.Status == WithdrawOrder.WithdrawStatus.Success).Select(t => (DateTime?)t.CreateAt).FirstOrDefault();
            return createAt == null ? DateTime.MinValue : createAt.Value;
        }

        /// <summary>
        /// 获取提现订单处理记录
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public List<WithdrawOrderLog> GetWithdrawLogList(int orderId)
        {
            return BDC.WithdrawOrderLog.Where(t => t.SiteID == SiteInfo.ID && t.WithdrawID == orderId).OrderBy(t => t.CreateAt).ToList();
        }

        /// <summary>
        /// 审核提现申请
        /// </summary>
        /// <param name="orderId">提现订单号</param>
        /// <param name="withdrawSettingId">选择的提现接口</param>
        /// <param name="action">操作 agree | reject</param>
        /// <returns></returns>
        public bool CheckWithdrawOrder(int orderId, int withdrawSettingId, string description, string action)
        {
            WithdrawOrder order = this.GetWithdrawOrderInfo(orderId);
            if (order == null)
            {
                base.Message("状态错误");
                return false;
            }
            order.WithdrawSettingID = withdrawSettingId;

            WithdrawSetting setting = SiteAgent.Instance().GetWithdrawSettingInfo(withdrawSettingId);
            if (setting == null || !setting.IsOpen)
            {
                base.Message("提现接口错误");
                return false;
            }
            IWithdraw withdraw = WithdrawFactory.CreateWithdraw(setting.Type, setting.SettingString);

            bool success = false;

            description = string.Format("[{0}] {1}", action, description);

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                switch (action)
                {
                    case "agree":
                        #region ======== 审核通过  =========

                        if (order.Status != WithdrawOrder.WithdrawStatus.None)
                        {
                            base.Message("状态错误");
                            db.Rollback();
                            return false;
                        }
                        if (withdraw != null)
                        {
                            // 自动付款接口
                            string msg;
                            withdraw.BankCode = order.Bank;
                            withdraw.Account = order.AccountName;
                            withdraw.CardNo = order.AccountNumber;
                            withdraw.Money = order.Money;
                            withdraw.OrderID = order.ID.ToString();

                            if (withdraw.Remit(out msg))
                            {
                                if (!this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Success, description))
                                {
                                    db.Rollback();
                                    return false;
                                }

                                success = true;
                            }
                            else
                            {
                                base.Message(msg);
                                db.Rollback();
                                return false;
                            }
                        }
                        else
                        {
                            // 手动打款
                            if (!this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Finish, description))
                            {
                                db.Rollback();
                                return false;
                            }
                            if (!this.FinishWithdrawOrder(db, order, description))
                            {
                                db.Rollback();
                                return false;
                            }
                            success = true;
                        }
                        db.Commit();

                        #endregion
                        break;
                    case "reject":
                        #region =========== 审核拒绝 ============
                        if (order.Status != WithdrawOrder.WithdrawStatus.None)
                        {
                            base.Message("状态错误");
                            db.Rollback();
                            return false;
                        }
                        if (!this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Faild, description))
                        {
                            db.Rollback();
                            return false;
                        }
                        if (!this.FaildWithdrawOrder(db, order, "提现失败"))
                        {
                            db.Rollback();
                            return false;
                        }
                        success = true;
                        db.Commit();
                        #endregion
                        break;
                    // 检查状态
                    case "check":
                        #region =========== 检查状态 ==============
                        if (withdraw != null)
                        {
                            string msg;
                            WithdrawStatus queryStatus = withdraw.Query(order.ID.ToString(), out msg);
                            switch (order.Status)
                            {
                                case WithdrawOrder.WithdrawStatus.None:
                                case WithdrawOrder.WithdrawStatus.Success:
                                    switch (queryStatus)
                                    {
                                        case WithdrawStatus.Success:
                                            if (this.FinishWithdrawOrder(db, order, msg) && this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Finish, msg))
                                            {
                                                db.Commit();
                                                success = true;
                                            }
                                            break;
                                        case WithdrawStatus.Return:
                                            if (this.FaildWithdrawOrder(db, order, msg) && this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Return, msg))
                                            {
                                                db.Commit();
                                                success = true;
                                            }
                                            break;
                                        case WithdrawStatus.Paymenting:
                                            if (order.Status == WithdrawOrder.WithdrawStatus.None && this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Success, msg))
                                            {
                                                db.Commit();
                                                success = true;
                                            }
                                            break;
                                        default:
                                            base.Message("远程接口返回结果{0}，信息：{1}", queryStatus.GetDescription(), msg);
                                            db.Rollback();
                                            break;
                                    }
                                    break;
                                default:
                                    base.Message("当前订单状态[{0}]不允许自动更改结果。\n\r 远程接口返回结果{1}，信息：{2}", order.Status.GetDescription(), queryStatus.GetDescription(), msg);
                                    db.Rollback();
                                    break;
                            }
                        }
                        else
                        {
                            base.Message("非自动出款接口，请手工查询");
                            db.Rollback();
                        }
                        #endregion
                        break;
                    case "return":
                        // 银行退单操作
                        #region ========== 手工操作退单 =========
                        if (this.ReturnWithdrawOrder(db, order, "手工退单") && this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Return, "手工退单"))
                        {
                            db.Commit();
                            success = true;
                        }
                        else
                        {
                            db.Rollback();
                        }
                        #endregion
                        break;
                    case "resubmit":
                        // 重新提交
                        #region ========= 银行退单后重新提交提现订单 ===========
                        if (order.Status != WithdrawOrder.WithdrawStatus.Return || this.GetWithdrawLogList(order.ID).LastOrDefault().CreateAt < DateTime.Now.AddDays(-1))
                        {
                            base.Message("只能重新提交24小时之内退单的订单");
                            db.Rollback();
                        }
                        else
                        {
                            this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Return, "重新提交提现申请");

                            //#1 重建记录
                            order.Status = WithdrawOrder.WithdrawStatus.None;
                            order.CreateAt = DateTime.Now;
                            order.Description = string.Format("退单重新提交，原提现编号：{0}", order.ID);
                            order.WithdrawSettingID = 0;
                            order.Add(true, db);

                            if (!this.LockMoney(db, order.UserID, order.Money, MoneyLock.LockType.Withdraw, order.ID, "重新提交提现")
                                ||
                              !this.LockMoney(db, order.UserID, order.Fee, MoneyLock.LockType.WithdrawFee, order.ID, "重新提交提现"))
                            {
                                base.Message("锁定金额失败");
                                db.Rollback();
                            }
                            else
                            {
                                this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.None, "重新提交提现申请，原提现ID：" + orderId);
                                success = true;
                                db.Commit();
                            }
                        }
                        #endregion
                        break;
                    case "split":
                        #region =========== 拆单 ==============
                        if (order.Status != WithdrawOrder.WithdrawStatus.None)
                        {
                            base.Message("状态错误");
                            return false;
                        }
                        if (SiteInfo.Setting.WithdrawUnit == 0)
                        {
                            base.Message("未设置拆单规则");
                            return false;
                        }
                        if (SiteInfo.Setting.WithdrawUnit >= order.Money)
                        {
                            base.Message("未达到拆单标准");
                            return false;
                        }
                        if (!this.UnlockMoney(db, order.UserID, MoneyLock.LockType.Withdraw, order.ID, "拆单"))
                        {
                            base.Message("解锁资金失败");
                            db.Rollback();
                            return false;
                        }
                        if (order.Fee != decimal.Zero && !this.UnlockMoney(db, order.UserID, MoneyLock.LockType.WithdrawFee, order.ID, "拆单"))
                        {
                            base.Message("解锁手续费失败");
                            db.Rollback();
                            return false;
                        }

                        this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.Split, string.Format("系统拆单"));

                        decimal withdrawMoney = order.Money;
                        decimal orderFee = order.Fee;
                        decimal totalMoney = withdrawMoney + orderFee;
                        decimal splitMoneyCount = 0;
                        decimal orderFeeRate = orderFee / totalMoney;
                        do
                        {
                            decimal unitOrderMoney = Math.Min(totalMoney, SiteInfo.Setting.WithdrawUnit);
                            order.Fee = orderFeeRate * unitOrderMoney;
                            order.Money = unitOrderMoney - order.Fee;
                            order.WithdrawSettingID = 0;
                            order.Add(true, db);

                            if (!this.LockMoney(db, order.UserID, order.Money, MoneyLock.LockType.Withdraw, order.ID, "提现"))
                            {
                                base.Message("锁定资金失败");
                                db.Rollback();
                                return false;
                            }

                            if (order.Fee != decimal.Zero && !this.LockMoney(db, order.UserID, order.Fee, MoneyLock.LockType.WithdrawFee, order.ID, "提现手续费"))
                            {
                                base.Message("锁定提现手续费失败");
                                db.Rollback();
                                return false;
                            }

                            splitMoneyCount++;
                            this.AddWithdrawOrderLog(db, order, WithdrawOrder.WithdrawStatus.None, string.Format("订单拆分{0}，总提金额：{1}元，手续费：{2}元", splitMoneyCount, withdrawMoney.ToString("n"), orderFee.ToString("n")));

                            totalMoney -= SiteInfo.Setting.WithdrawUnit;
                        } while (totalMoney > 0);

                        db.Commit();
                        success = true;
                        #endregion
                        break;
                    default:
                        base.Message("没有选择要进行的操作");
                        db.Rollback();
                        break;
                }
            }

            return success;
        }

        /// <summary>
        /// 专门处理自动出款接口
        /// 仅标记订单的出款接口，留待出款定时任务来进行处理
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="withdrawSettingId"></param>
        /// <param name="description"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool CheckWithdrawOrderList(int orderId, int withdrawSettingId, string description)
        {
            WithdrawOrder order = this.GetWithdrawOrderInfo(orderId);
            if (order == null || order.Status != WithdrawOrder.WithdrawStatus.None || order.WithdrawSettingID != 0)
            {
                base.Message("状态错误");
                return false;
            }
            order.WithdrawSettingID = withdrawSettingId;

            WithdrawSetting setting = SiteAgent.Instance().GetWithdrawSettingInfo(withdrawSettingId);
            if (setting == null || !setting.IsOpen || setting.Type == WithdrawType.Manually)
            {
                base.Message("提现接口错误");
                return false;
            }

            order.WithdrawSettingID = withdrawSettingId;
            order.Description = description;
            return order.Update(null, t => t.WithdrawSettingID, t => t.Description) != 0;
        }

        /// <summary>
        /// 完成订单后的资金操作
        /// 解锁+扣除资金
        /// </summary>
        /// <returns></returns>
        private bool FinishWithdrawOrder(DbExecutor db, WithdrawOrder order, string description)
        {
            if (!this.UnlockMoney(db, order.UserID, MoneyLock.LockType.Withdraw, order.ID, description))
            {
                return false;
            }
            if (!this.AddMoneyLog(db, order.UserID, order.Money * -1, MoneyLog.MoneyType.Withdraw, order.ID, "提现编号" + order.ID))
            {
                return false;
            }

            if (order.Fee != decimal.Zero)
            {
                if (!this.UnlockMoney(db, order.UserID, MoneyLock.LockType.WithdrawFee, order.ID, description))
                {
                    return false;
                }
                if (!this.AddMoneyLog(db, order.UserID, order.Fee * -1, MoneyLog.MoneyType.WithdrawFee, order.ID, "提现编号" + order.ID))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 提现失败（手工拒绝或者银行退单）
        /// 解锁资金 + 恢复提现额度
        /// </summary>
        /// <param name="db"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool FaildWithdrawOrder(DbExecutor db, WithdrawOrder order, string description)
        {
            if (!this.UnlockMoney(db, order.UserID, MoneyLock.LockType.Withdraw, order.ID, description))
            {
                return false;
            }
            if (order.Fee != decimal.Zero && !this.UnlockMoney(db, order.UserID, MoneyLock.LockType.WithdrawFee, order.ID, description))
            {
                return false;
            }

            if (!this.WithdrawLog(db, order.UserID, order.Money + order.Fee, string.Format("提现失败，编号{0}", order.ID)))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(order.SourceProc))
            {
                try
                {
                    return db.ExecuteNonQuery(CommandType.StoredProcedure, order.SourceProc,
                        NewParam("@OrderID", order.ID)) != 0;
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
        /// 手工操作退单
        /// </summary>
        /// <param name="db"></param>
        /// <param name="order"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        private bool ReturnWithdrawOrder(DbExecutor db, WithdrawOrder order, string description)
        {
            if (order.Status != WithdrawOrder.WithdrawStatus.Finish)
            {
                base.Message("当前状态不允许操作退单");
                return false;
            }

            if (!this.AddMoneyLog(db, order.UserID, order.TotalMoney, MoneyLog.MoneyType.WithdrawFaild, order.ID, description))
            {
                return false;
            }

            order.Status = WithdrawOrder.WithdrawStatus.Return;
            order.Update(db, t => t.Status);

            return true;
        }

        /// <summary>
        /// 添加提现状态变化日志（提现订单的状态变化一定要经过这个方法）
        /// 仅改变订单装以及日志添加，不涉及资金处理
        /// </summary>
        /// <param name="db"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool AddWithdrawOrderLog(DbExecutor db, WithdrawOrder order, WithdrawOrder.WithdrawStatus status, string description)
        {
            try
            {
                if (!new WithdrawOrderLog()
                {
                    AdminID = AdminInfo == null ? 0 : AdminInfo.ID,
                    CreateAt = DateTime.Now,
                    SiteID = order.SiteID,
                    Status = status,
                    Description = description,
                    UserID = order.UserID,
                    WithdrawID = order.ID
                }.Add(db))
                {
                    return false;
                }

                order.Status = status;
                order.Description = description;

                return order.Update(db, t => t.Status, t => t.Description, t => t.WithdrawSettingID) != 0;
            }
            catch (Exception ex)
            {
                base.Message(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 保存远程网关的订单号
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="systemId"></param>
        /// <returns></returns>
        public bool UpdateWithdrawOrderSystemID(int orderId, string systemId)
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.ExecuteNonQuery(CommandType.Text, "UPDATE usr_WithdrawOrder SET SystemID = @SystemID WHERE WithdrawID = @OrderID AND SystemID = ''",
                    NewParam("@SystemID", systemId),
                    NewParam("@OrderID", orderId)) == 1;
            }
        }

        /// <summary>
        /// 获取远程网关的订单号
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public string GetWithdrawOrderSystemID(int orderId)
        {
            return BDC.WithdrawOrder.Where(t => t.ID == orderId).Select(t => t.SystemID).FirstOrDefault();
        }

        /// <summary>
        /// 获取用户被锁定的资金
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<MoneyLock> GetMoneyLockList(int userId)
        {
            return BDC.MoneyLock.Where(t => t.UserID == userId && t.UnLockAt.Year < 2000).OrderByDescending(t => t.ID).ToList();
        }

        /// <summary>
        /// 获取用户指定类型被锁定的资金
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sourceId"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public List<MoneyLock> GetMoneyLockList(int userId, int sourceId, params MoneyLock.LockType[] types)
        {
            return BDC.MoneyLock.Where(t => t.UserID == userId && t.SourceID == sourceId && t.UnLockAt.Year < 2000 && types.Contains(t.Type)).OrderByDescending(t => t.ID).ToList();
        }

        /// <summary>
        /// 获取资金锁定信息
        /// 如果siteID不为0则适用于非web程序
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sourceId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MoneyLock GetMoneyLockInfo(int userId, int sourceId, MoneyLock.LockType type, int siteId = 0)
        {
            if (siteId == 0) siteId = SiteInfo.ID;
            return BDC.MoneyLock.Where(t => t.SiteID == siteId && t.UserID == userId && t.SourceID == sourceId && t.Type == type).FirstOrDefault();
        }

        /// <summary>
        /// 获取同来源的资金类型
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public List<MoneyLog> GetMoneyLogList(MoneyLog.MoneyType type, int sourceId)
        {
            return BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.Type == type && t.SourceID == sourceId).ToList();
        }

        /// <summary>
        /// 获取同来源的资金类型（指定用户）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="sourceId"></param>
        /// <returns></returns>
        public List<MoneyLog> GetMoneyLogList(int userId, MoneyLog.MoneyType type, int sourceId)
        {
            return BDC.MoneyLog.Where(t => t.TableID == Utils.GetTableID(userId) && t.SiteID == SiteInfo.ID && t.UserID == userId && t.Type == type && t.SourceID == sourceId).ToList();
        }

        /// <summary>
        /// 查看充值订单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RechargeOrder GetRechargeOrderInfo(long id)
        {
            return BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).FirstOrDefault();
        }

        /// <summary>
        /// 根据网关编号获取充值订单
        /// </summary>
        /// <param name="systemId"></param>
        /// <returns></returns>
        public RechargeOrder GetRechargeOrderInfo(string systemId)
        {
            return BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.SystemID == systemId).FirstOrDefault();
        }

        /// <summary>
        /// 根据金额时间查找一笔未支付的充值订单
        /// </summary>
        /// <param name="payId"></param>
        /// <param name="money"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <returns></returns>
        internal RechargeOrder GetRechargeOrderInfo(int payId, decimal money, DateTime startAt, DateTime endAt)
        {
            return BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.PayID == payId && t.Money == money && !t.IsPayment && t.CreateAt > startAt && t.CreateAt < endAt).FirstOrDefault();
        }

        /// <summary>
        /// 保存用户提交的转账信息
        /// </summary>
        /// <param name="rechargeId">充值订单编号</param>
        /// <param name="money"></param>
        /// <param name="name"></param>
        /// <param name="paymentTime"></param>
        /// <param name="systemId">系统订单号</param>
        /// <returns></returns>
        public bool SaveTransferOrder(long rechargeId, decimal money, string name, DateTime paymentTime, string description)
        {
            RechargeOrder order = UserAgent.Instance().GetRechargeOrderInfo(rechargeId);
            if (order == null)
            {
                base.Message("充值订单号错误");
                return false;
            }
            if (order.IsPayment)
            {
                base.Message("该订单已经充值");
                return false;
            }

            if (string.IsNullOrEmpty(name))
            {
                base.Message("请输入姓名");
                return false;
            }
            if (paymentTime > DateTime.Now.AddMinutes(10) || paymentTime < DateTime.Now.AddHours(-1))
            {
                base.Message("转账时间填写错误");
                return false;
            }

            return new TransferOrder()
            {
                SiteID = SiteInfo.ID,
                PayID = order.PayID,
                RechargeID = order.ID,
                UserID = order.UserID,
                Name = name,
                Money = money,
                CreateAt = DateTime.Now,
                Description = description,
                PaymentAt = paymentTime,
                Status = TransferOrder.TransferStatus.None
            }.Add();
        }

        /// <summary>
        /// 获取转账订单
        /// </summary>
        /// <param name="transferId"></param>
        /// <returns></returns>
        public TransferOrder GetTransferOrderInfo(int transferId)
        {
            return BDC.TransferOrder.Where(t => t.SiteID == SiteInfo.ID && t.ID == transferId).FirstOrDefault();
        }

        /// <summary>
        /// 根据银行转账信息获取转账记录（必须为待审核的记录）
        /// </summary>
        /// <param name="payId">支付方式</param>
        /// <param name="name">转账人心目</param>
        /// <param name="money">金额</param>
        /// <param name="date">查收到帐的时间</param>
        /// <returns></returns>
        public TransferOrder GetTransferOrderInfo(int payId, string name, decimal money, DateTime date)
        {
            return BDC.TransferOrder.Where(t => t.SiteID == SiteInfo.ID && t.PayID == payId && t.Status == TransferOrder.TransferStatus.None && t.Name == name && t.Money == money && t.CreateAt > date.AddHours(-1) && t.CreateAt < date.AddHours(1)).FirstOrDefault();
        }

        /// <summary>
        /// 处理转账订单（审核通过/拒绝）
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="money"></param>
        /// <param name="status"></param>
        /// <param name="systemId">流水号</param>
        /// <returns></returns>
        public bool CheckTransferOrder(int transferId, decimal money, TransferOrder.TransferStatus status, string systemId)
        {
            TransferOrder order = this.GetTransferOrderInfo(transferId);
            if (order == null || order.Status != TransferOrder.TransferStatus.None)
            {
                base.Message("转账订单状态错误");
                return false;
            }

            order.Status = status;
            order.CheckAt = DateTime.Now;

            switch (status)
            {
                case TransferOrder.TransferStatus.Faild:
                    if (order.Update(null, t => t.Status, t => t.CheckAt) == 1)
                    {
                        AdminInfo.Log(AdminLog.LogType.Money, "转账订单，编号：{0}，审核拒绝", transferId);
                        return true;
                    }
                    break;
                case TransferOrder.TransferStatus.Success:
                    if (string.IsNullOrEmpty(systemId))
                    {
                        base.Message("请输入流水号");
                        return false;
                    }
                    string checkDescription = string.Format("管理员{0}审核", AdminInfo.Name);

                    long rechargeId = order.RechargeID;
                    if (rechargeId == 0) rechargeId = this.CreateRechargeOrder(order.UserID, order.PayID, money, checkDescription, true);
                    if (rechargeId == 0) return false;

                    if (this.ConfirmRechargeOrderInfo(rechargeId, money, systemId, checkDescription))
                    {
                        order.SerialID = systemId;
                        order.RechargeID = rechargeId;
                        order.Amount = money;
                        order.Update(null, t => t.Amount, t => t.Status, t => t.CheckAt, t => t.SerialID, t => t.RechargeID);
                        AdminInfo.Log(AdminLog.LogType.Money, "转账订单，编号：{0}，审核通过", transferId);
                        return true;
                    }
                    break;
            }
            base.Message("审核订单提交错误");
            return false;
        }

        /// <summary>
        /// 标记转账订单已经处理完毕
        /// </summary>
        /// <param name="orderId">充值订单</param>
        /// <param name="payId">充值渠道</param>
        /// <param name="description">备注信息</param>
        /// <returns></returns>
        public bool UpdateTransferOrder(int payId, long rechargeId, string systemId, decimal amount, string description)
        {
            return BDC.TransferOrder.Update(new TransferOrder()
            {
                Amount = amount,
                SerialID = systemId,
                Status = TransferOrder.TransferStatus.Success,
                Description = description,
                CheckAt = DateTime.Now
            }, t => t.SiteID == SiteInfo.ID && t.RechargeID == rechargeId && t.PayID == payId && t.Status == TransferOrder.TransferStatus.None,
               t => t.Amount, t => t.Status, t => t.SerialID, t => t.Description, t => t.CheckAt) != 0;
        }

        /// <summary>
        /// 确认入账
        /// </summary>
        /// <param name="id">订单编号</param>
        /// <param name="amount">实际入账金额</param>
        /// <param name="systemId">网关的系统编号</param>
        /// <param name="description">备注信息</param>
        /// <returns></returns>
        public bool ConfirmRechargeOrderInfo(long id, decimal amount, string systemId, string description = null)
        {
            if (amount <= decimal.Zero)
            {
                base.Message("到账金额不能为零");
                return false;
            }
            if (string.IsNullOrEmpty(systemId))
            {
                base.Message("请输入网关流水单号");
                return false;
            }
            lock (this._rechargeLockObject)
            {
                if (BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.SystemID == systemId).Count() != 0)
                {
                    base.Message("该流水号在系统中已经存在，请注意是否重复入账");
                    return false;
                }
                RechargeOrder order = this.GetRechargeOrderInfo(id);
                if (order == null)
                {
                    base.Message("编号错误");
                    return false;
                }
                if (order.IsPayment)
                {
                    base.Message("该订单已经支付");
                    return false;
                }
                if (order.CreateAt < DateTime.Now.AddDays(-1))
                {
                    base.Message("该订单已经超过24小时，无法入账");
                    return false;
                }
                if (amount > order.Money)
                {
                    base.Message("超过订单金额");
                    return false;
                }
                PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(order.PayID);
                using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
                {
                    order.Amount = amount;
                    order.PayAt = DateTime.Now;
                    order.SystemID = systemId;
                    order.IsPayment = true;
                    order.Fee = amount * payment.Fee;
                    order.Description = string.IsNullOrEmpty(description) ? order.Description : description;
                    order.Reward = amount * payment.Reward;
                    order.Update(db, t => t.Amount, t => t.PayAt, t => t.SystemID, t => t.IsPayment, t => t.Fee, t => t.Description, t => t.Reward);
                    int sourceId = (int)(order.ID % int.MaxValue);

                    if (!this.AddMoneyLog(db, order.UserID, amount, MoneyLog.MoneyType.Recharge, sourceId, string.Format("{0}充值入账", payment.Name)))
                    {
                        db.Rollback();
                        return false;
                    }

                    if (order.Reward != decimal.Zero)
                    {
                        if (order.Reward > decimal.Zero)
                        {
                            if (!this.AddMoneyLog(db, order.UserID, order.Reward, MoneyLog.MoneyType.RechargeReward, sourceId, string.Format("{0}充值奖励", payment.Name)))
                            {
                                db.Rollback();
                                return false;
                            }
                        }
                        else
                        {
                            if (!this.AddMoneyLog(db, order.UserID, order.Reward, MoneyLog.MoneyType.RechargeFee, sourceId, string.Format("{0}充值手续费", payment.Name)))
                            {
                                db.Rollback();
                                return false;
                            }
                        }
                    }

                    db.Commit();
                }

                this.AddNotify(order.UserID, UserNotify.NotifyType.Recharge, "充值{0}元，已成功到账。", order.Amount.ToString("n"));

                UserAgent.Instance().UpdateTransferOrder(order.PayID, order.ID, order.SystemID, order.Amount, "自动审核");

                // 首充奖励
                SiteAgent.Instance()._firstRecharge(order.UserID, order.Amount);

                // 系统回调
                BetCallback.GetCallback(order.SiteID, BetCallback.CallbackType.Recharge, order.ID);

                return true;
            }
        }


        /// <summary>
        /// 修改用户的提现额度
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="withdraw">新的提现额度</param>
        /// <returns></returns>
        public bool UpdateUserWithdraw(int userId, decimal withdraw)
        {
            if (withdraw < decimal.Zero)
            {
                base.Message("提现额度错误");
                return false;
            }
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                UserMoney userMoney = this.GetTotalMoney(userId);
                if (withdraw > userMoney.Money)
                {
                    base.Message("提现额度大于可用余额");
                    return false;
                }

                if (!this.WithdrawLog(db, userId, withdraw - userMoney.Withdraw, string.Format("管理员{0}操作", AdminInfo.Name)))
                {
                    db.Rollback();
                    return false;
                }

                db.Commit();
            }
            return true;
        }

        /// <summary>
        /// 把资金类型转化成为分类类型的统计
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Dictionary<MoneyLog.MoneyCategoryType, decimal> GetMoneyCategory(Dictionary<MoneyLog.MoneyType, decimal> data)
        {
            Dictionary<MoneyLog.MoneyCategoryType, decimal> list = new Dictionary<MoneyLog.MoneyCategoryType, decimal>();
            foreach (KeyValuePair<MoneyLog.MoneyType, decimal> item in data)
            {
                MoneyLog.MoneyCategoryType type = item.Key.GetCategory();
                if (list.ContainsKey(type))
                {
                    list[type] += item.Value;
                }
                else
                {
                    list.Add(type, item.Value);
                }
            }
            return list;
        }
    }
}
