using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Resources;
using System.Xml.Linq;
using System.Web;

using SP.Studio.Data;
using SP.Studio.Xml;
using SP.Studio.Core;
using SP.Studio.Web;
using SP.Studio.Security;

using BW.Framework;
using BW.GateWay.Games;
using BW.Common.Games;
using BW.Common.Users;
using BW.Common.Reports;

namespace BW.Agent
{
    /// <summary>
    /// 第三方游戏的代理类
    /// </summary>
    public partial class GameAgent : AgentBase<GameAgent>
    {
        /// <summary>
        /// 获取系统中的游戏（没有配置也返回，状态为未开启）
        /// </summary>
        /// <returns></returns>
        public List<GameSetting> GetGameSetting()
        {
            return this.GetGameSetting(SiteInfo.ID);
        }

        /// <summary>
        /// 指定的站点游戏接口配置（适用于非web程序）
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<GameSetting> GetGameSetting(int siteId)
        {
            List<GameSetting> list = BDC.GameSetting.Where(t => t.SiteID == siteId).ToList();
            foreach (GameType type in Enum.GetValues(typeof(GameType)))
            {
                if (!list.Exists(t => t.Type == type))
                {
                    list.Add(new GameSetting()
                    {
                        SiteID = siteId,
                        Type = type
                    });
                }
            }
            return list;
        }

        /// <summary>
        /// 获取系统中该类型游戏的自定义参数配置
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<GameSetting> GetGameSetting(GameType type)
        {
            return BDC.GameSetting.Where(t => t.Type == type).ToList();
        }

        /// <summary>
        /// 获取游戏接口的设置
        /// 如果SiteID不为0则适用于非web程序
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public GameSetting GetGameSettingInfo(GameType type, int siteId = 0)
        {
            if (siteId == 0) siteId = SiteInfo.ID;

            return BDC.GameSetting.Where(t => t.SiteID == siteId && t.Type == type).FirstOrDefault() ?? new GameSetting() { SiteID = siteId, Type = type, IsOpen = false };
        }


        /// <summary>
        /// 保存游戏接口的设置进入数据库
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool SaveGameSettingInfo(GameSetting setting)
        {
            setting.SiteID = SiteInfo.ID;
            if (setting.Exists())
            {
                return setting.Update() == 1;
            }
            else
            {
                return setting.Add();
            }
        }

        /// <summary>
        /// 导入真人视频日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool ImportLog(VideoLog log)
        {
            //if (log.SiteID == 0) return false;
            GameSetting setting = log.UserID == 0 ? null : GameAgent.Instance().GetGameSettingInfo(log.Type, log.SiteID);
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (log.Exists(db, t => t.SiteID, t => t.BillNo))
                {
                    base.Message("记录已经存在");
                    return false;
                }
                if (!log.Add(db))
                {
                    db.Rollback();
                    return false;
                }
                if (setting != null && log.UserID != 0)
                {
                    this.UpdateAccountWithdraw(db, log.Type, log.UserID, setting.Turnover, log.BetAmount, log.Money, log.Balance, log.EndAt);
                }

                this.SaveGameReport(db, log.SiteID, log.UserID, log.Type, log.StartAt, log.Money, log.BetAmount, GameCategory.Live);

                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 导入电子游戏记录
        /// </summary>
        /// <param name="log"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool ImportLog(SlotLog log)
        {
            //if (log.SiteID == 0) return false;
            GameSetting setting = log.UserID == 0 ? null : GameAgent.Instance().GetGameSettingInfo(log.Type, log.SiteID);

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (log.Exists(db, t => t.SiteID, t => t.BillNo))
                {
                    base.Message("记录已经存在");
                    return false;
                }
                if (!log.Add(db))
                {
                    db.Rollback();
                    return false;
                }
                if (setting != null && log.UserID != 0)
                {
                    this.UpdateAccountWithdraw(db, log.Type, log.UserID, setting.Turnover, log.BetAmount, log.Money, log.Balance, log.PlayAt);
                }

                this.SaveGameReport(db, log.SiteID, log.UserID, log.Type, log.PlayAt, log.Money, log.BetAmount, GameCategory.Slot);

                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 导入体育记录
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool ImportLog(SportLog log)
        {
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                SportLog loginfo = new SportLog() { Type = log.Type, WagersID = log.WagersID }.Info(db, t => t.Type, t => t.WagersID);
                if (loginfo == null)
                {
                    if (!log.Add(db))
                    {
                        db.Rollback();
                        return false;
                    }
                }
                else if (loginfo.Status == log.Status)
                {
                    db.Rollback();
                    return false;
                }
                else
                {
                    loginfo.ResultAt = DateTime.Now;
                    loginfo.Result = log.Result;
                    loginfo.Status = log.Status;
                    loginfo.BetAmount = log.BetAmount;
                    loginfo.BetMoney = log.BetMoney;
                    loginfo.Money = log.Money;
                    loginfo.ExtendXML = log.ExtendXML;

                    loginfo.Update(db, t => t.ResultAt, t => t.Result, t => t.Status, t => t.BetAmount, t => t.BetMoney, t => t.Money, t => t.ExtendXML);

                    this.SaveGameReport(db, log.SiteID, log.UserID, log.Type, log.ResultAt, log.Money, log.BetAmount, GameCategory.Sport);
                }

                GameSetting setting = log.UserID == 0 ? null : GameAgent.Instance().GetGameSettingInfo(log.Type, log.SiteID);
                if (setting != null && log.UserID != 0 && !string.IsNullOrEmpty(setting.SettingString))
                {
                    decimal balance = setting.Setting.GetBalance(log.UserID);
                    this.UpdateAccountWithdraw(db, log.Type, log.UserID, setting.Turnover, log.BetMoney, log.Money, balance, log.PlayAt);
                }
                db.Commit();
                return true;
            }
        }

        /// <summary>
        /// 保存游戏的第三方报表
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="date"></param>
        /// <param name="money"></param>
        /// <param name="amount"></param>
        /// <param name="category">游戏分类</param>
        private void SaveGameReport(DbExecutor db, int siteId, int userId, GameType type, DateTime date, decimal money, decimal amount, GameCategory category)
        {
            db.ExecuteNonQuery(CommandType.StoredProcedure, "data_SaveUserGameReport",
                NewParam("@SiteID", siteId),
                NewParam("@UserID", userId),
                NewParam("@Type", type),
                NewParam("@Date", date),
                NewParam("@Money", money),
                NewParam("@Amount", amount),
                NewParam("@GameType", category));
        }

        /// <summary>
        /// 获取开始导入日志的开始时间（在日志服务中调用）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DateTime GetLogStartTime(GameType type, string gameType = "")
        {
            using (DbExecutor db = NewExecutor())
            {
                object startTime = db.ExecuteScalar(CommandType.StoredProcedure, "game_GetStartTime",
                    NewParam("@Type", type),
                    NewParam("@GameType", gameType));
                if (startTime == DBNull.Value) return DateTime.Now.AddDays(-1);
                if ((DateTime)startTime < DateTime.Now.AddDays(-5)) startTime = DateTime.Now.AddDays(-5);
                return (DateTime)startTime;
            }
        }

        /// <summary>
        /// 更新会员游戏账户的提现额度（仅记录）
        /// 算法：Math.Min(当前余额 ， 原提现额 + 当前投注 / 流水倍数 + 奖金)
        /// </summary>
        /// <param name="db">数据库对象</param>
        /// <param name="type">游戏类型</param>
        /// <param name="userId">用户</param>
        /// <param name="turnover">当前游戏设定的流水限制</param>
        /// <param name="betMoney">投注金额</param>
        /// <param name="money">奖金（为负数不累计）</param>
        /// <param name="balance">当前余额</param>
        /// <param name="playAt">游戏时间</param>
        private void UpdateAccountWithdraw(DbExecutor db, GameType type, int userId, decimal turnover, decimal betMoney, decimal money, decimal balance, DateTime playAt)
        {

            GameAccount account = new GameAccount() { UserID = userId, Type = type }.Info(db);
            if (account == null) return;
            string desc = string.Empty;

            //原提现额 + 当前投注 / 流水倍数
            if (turnover != decimal.Zero)
            {
                account.Withdraw += betMoney / turnover;
                desc += string.Format("投注金额：{0}，流水限制：{1}", betMoney.ToString("n"), turnover.ToString("P"));
            }
            // + 奖金（为负数不累计）
            if (money > decimal.Zero)
            {
                account.Withdraw += money;
                desc += string.Format(" 奖金：+{0}", money.ToString("n"));
            }

            desc += string.Format(" 当前余额：{0}，提现额：{1}", balance.ToString("n"), account.Withdraw.ToString("n"));
            // 只有日志中可以获取余额的情况下才与余额做比对
            // 不与余额做比对
            //if (balance > decimal.Zero)
            //{
            //    account.Withdraw = Math.Min(balance, account.Withdraw);
            //    desc += string.Format(" 取小值：{0}", account.Withdraw.ToString("n"));
            //}

            account.WithdrawAt = playAt;
            account.Update(db, t => t.Withdraw, t => t.WithdrawAt);

            this.AddGameWithdraw(db, userId, type, account.Withdraw, playAt, desc);
        }


        /// <summary>
        /// 添加游戏账户的提现额度变化日志
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="withdraw"></param>
        /// <param name="description"></param>
        private bool AddGameWithdraw(DbExecutor db, int userId, GameType type, decimal withdraw, DateTime createAt, string description)
        {
            return new GameWithdraw()
            {
                SiteID = UserAgent.Instance().GetSiteID(userId, db),
                CreateAt = createAt,
                UserID = userId,
                Type = type,
                Withdraw = withdraw,
                Description = description
            }.Add(db);
        }


        /// <summary>
        /// 添加转出额度变化日志
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="type"></param>
        /// <param name="withdraw"></param>
        /// <param name="createAt"></param>
        /// <param name="description"></param>
        public void AddGameWithdraw(int userId, GameType type, decimal withdraw, DateTime createAt, string description)
        {
            using (DbExecutor db = NewExecutor())
            {
                this.AddGameWithdraw(db, userId, type, withdraw, createAt, description);
            }
        }


        /// <summary>
        /// 游戏字典解析的缓存KEY
        /// </summary>
        private Dictionary<string, string> _gameValue = new Dictionary<string, string>();
        /// <summary>
        /// 获取注释值
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(GameType type, string category, string key)
        {
            string cacheKey = string.Concat(type, "-", category, "-", key);
            if (_gameValue.ContainsKey(cacheKey)) return _gameValue[cacheKey];
            lock (_gameValue)
            {
                if (_gameValue.ContainsKey(cacheKey)) return _gameValue[cacheKey];

                ResourceManager rm = new ResourceManager(typeof(Resources.Res));
                string xml = (string)rm.GetObject(type.ToString() + "_Log");
                if (string.IsNullOrEmpty(xml)) return key;
                XElement root = XElement.Parse(xml);
                if (!string.IsNullOrEmpty(category)) root = root.Element(category);
                if (root == null)
                {
                    _gameValue.Add(cacheKey, key);
                    return key;
                }

                XElement item = root.Elements("item").Where(t => t.GetAttributeValue("name") == key).FirstOrDefault();
                if (item == null)
                {
                    _gameValue.Add(cacheKey, key);
                    return key;
                }
                string value = item.GetAttributeValue("value");
                _gameValue.Add(cacheKey, value);
                return value;
            }
        }

        /// <summary>
        /// 获取游戏列表（如果返回null表示该接口不支持单独游戏入口）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerable<SlotGame> GetGameList(GameType type)
        {
            ResourceManager rm = new ResourceManager(typeof(Resources.Res));
            string xml = (string)rm.GetObject(type.ToString());
            if (string.IsNullOrEmpty(xml))
            {
                yield break;
            }
            XElement root = XElement.Parse(xml);
            List<SlotGame> list = new List<SlotGame>();
            foreach (XElement item in root.Elements("item"))
            {
                yield return new SlotGame(type, item);
            }
        }

        /// <summary>
        /// 获取游戏玩法对象
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public GamePlayer GetGamePlayerInfo(int playerId)
        {
            if (playerId == 0) return null;
            return BDC.GamePlayer.Where(t => t.SiteID == SiteInfo.ID && t.ID == playerId).FirstOrDefault();
        }

        /// <summary>
        /// 获取系统配置的所有老虎机游戏列表
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SlotGame> GetSlotGameList()
        {
            foreach (GameType type in Enum.GetValues(typeof(GameType)))
            {
                foreach (SlotGame game in this.GetGameList(type))
                {
                    yield return game;
                }
            }
        }

        /// <summary>
        /// 获取游戏列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<GamePlayer> GetGamePlayerList(GameType type)
        {
            IEnumerable<SlotGame> list = SysSetting.GetSetting().SlogGameList.Where(t => t.Type == type);

            Dictionary<string, GamePlayer> gamePlayer = BDC.GamePlayer.Where(t => t.SiteID == SiteInfo.ID && t.Type == type).ToDictionary(t => t.Code, t => t);

            int id = int.MinValue;
            foreach (SlotGame game in list)
            {
                GamePlayer player = null;
                if (gamePlayer.ContainsKey(game.ID))
                {
                    player = gamePlayer[game.ID];
                    player.GameInfo = game;
                }
                else
                {
                    player = new GamePlayer(game, id++);
                }
                yield return player;
            }
        }


        /// <summary>
        /// 获取游戏ID对应的中文游戏名
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetGameName(GameType type, string id)
        {
            IEnumerable<SlotGame> list = this.GetGameList(type);
            if (list == null || list.Count() == 0) return id;
            return list.FirstOrDefault(t => t.ID == id).Name ?? id;
        }

        /// <summary>
        /// 发起一笔转账（自动锁定金额）
        /// 此时并未操作真正转账
        /// </summary>
        /// <returns></returns>
        private int CreateTransfer(int userId, decimal money, GameType game, TransferLog.ActionType action)
        {
            if (money < 10M)
            {
                base.Message("转账最低金额为10元");
                return 0;
            }
            if (UserAgent.Instance().CheckUserLockStatus(userId, User.LockStatus.Transfer))
            {
                base.Message("账户被禁止转账");
                return 0;
            }
            if (UserAgent.Instance().CheckUserLockStatus(userId, User.LockStatus.Contract))
            {
                base.Message("您有契约转账尚未完成");
                return 0;
            }
            GameSetting setting = this.GetGameSettingInfo(game);
            if (setting.Rate == decimal.Zero)
            {
                base.Message("平台暂未开通结算功能");
                return 0;
            }

            DateTime? lastTransferTime = BDC.TransferLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Type == game).Max(t => (DateTime?)t.CreateAt);
            if (lastTransferTime != null && lastTransferTime.Value.AddSeconds(30) > DateTime.Now)
            {
                base.Message("上一笔转账正在处理中，请{0}秒之后再进行操作", (int)((TimeSpan)(lastTransferTime.Value.AddSeconds(30) - DateTime.Now)).TotalSeconds);
                return 0;
            }

            // 将要扣除的额度
            decimal userMoney = money * setting.Rate * (action == TransferLog.ActionType.IN ? -1 : 1);

            TransferLog log = new TransferLog()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Action = action,
                CreateAt = DateTime.Now,
                Money = money,
                Status = TransferLog.TransferStatus.None,
                Type = game
            };

            int sourceId = 0;
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                try
                {
                    if (!log.Add(true, db))
                    {
                        db.Rollback();
                        return 0;
                    }
                    sourceId = log.ID;

                    switch (action)
                    {
                        case TransferLog.ActionType.IN:
                            // 如果是转入则判断平台游戏账户额度是否足够
                            if (setting.Money + Math.Abs(money) * setting.Rate * -1 < decimal.Zero)
                            {
                                base.Message("平台额度不足，请与客服联系");
                                return 0;
                            }

                            // 从主账户转入游戏账户
                            if (UserAgent.Instance().GetUserMoney(userId, db) < money)
                            {
                                base.Message("可用余额不足");
                                return 0;
                            }

                            //#1.1 锁定用户金额
                            if (!UserAgent.Instance().LockMoney(db, userId, money, MoneyLock.LockType.Transfer, sourceId, string.Format("转账至{0}账户", game.GetDescription())))
                            {
                                db.Rollback();
                                return 0;
                            }
                            break;
                        case TransferLog.ActionType.OUT:

                            break;
                    }

                    db.Commit();
                }
                catch (Exception ex)
                {
                    base.Message(ex.Message);
                    db.Rollback();
                    return 0;
                }
            }
            return sourceId;
        }

        /// <summary>
        /// 更新转账日志记录的状态
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="status"></param>
        private bool UpdateTransferStatus(DbExecutor db, int transferId, TransferLog.TransferStatus status, string description)
        {
            if (status == TransferLog.TransferStatus.Success)
            {
                TransferLog log = new TransferLog() { ID = transferId }.Info(db);
                decimal money = Math.Abs(log.Money);
                if (log.Action == TransferLog.ActionType.IN) money *= -1;
                this.AddMoneyLog(db, log.Type, money, transferId,
                    string.Format("用户{0}{1}{2}元", UserAgent.Instance().GetUserName(db, log.UserID), log.Action.GetDescription(), log.Money.ToString("n")));
            }
            return db.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE {0} SET Status = @Status,TransferDesc = @Description,CheckAt = GETDATE() WHERE SiteID = @SiteID AND TransferID = @ID AND Status = @None", typeof(TransferLog).GetTableName()),
                NewParam("@SiteID", SiteInfo.ID),
                NewParam("@ID", transferId),
                NewParam("@Status", status),
                NewParam("@None", TransferLog.TransferStatus.None),
                NewParam("@Description", description)) > 0;
        }

        private static object _lockTransfer = new object();
        /// <summary>
        /// 与第三方游戏之间的账户转账
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="money"></param>
        /// <param name="game">要转入或者转出的游戏账户</param>
        /// <param name="action">IN 为转入| OUT为转出</param>
        /// <returns></returns>
        public bool Transfer(int userId, decimal money, GameType game, string action)
        {
            if (action != "IN" && action != "OUT")
            {
                base.Message("未选择正确的操作方式");
                return false;
            }

            GameSetting setting = this.GetGameSettingInfo(game);
            if (!setting.IsOpen || !setting.IsSystemOpen)
            {
                base.Message("{0}接口已经关闭", game.GetDescription());
                return false;
            }

            GameAccount account = UserAgent.Instance().GetGameAccountInfo(userId, game);
            if (account == null)
            {
                base.Message("您在{0}暂未开户", game.GetDescription());
                return false;
            }

            lock (userId.ToString())
            {
                int sourceId = this.CreateTransfer(userId, money, game, action.ToEnum<TransferLog.ActionType>());
                if (sourceId == 0)
                {
                    return false;
                }

                decimal amount;

                UserMoney userMoney = UserAgent.Instance().GetTotalMoney(userId);

                bool success = true;
                switch (action.ToEnum<TransferLog.ActionType>())
                {
                    case TransferLog.ActionType.IN:
                        #region 从主账户转入游戏账户

                        //#1.2 调用接口进行转入充值操作                                                                        
                        if (!setting.Setting.Deposit(userId, money, sourceId.ToString(), out amount))
                        {
                            base.Message("转入失败。资金将在5分钟内解锁");
                            return false;
                        }

                        //#1.3 转入成功，扣除用户资金
                        using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                        {
                            //#1.3.1 获取用户的提现额度
                            decimal withdraw = Math.Min(userMoney.Withdraw, money);
                            UserAgent.Instance().WithdrawLog(db, userId, withdraw * -1, string.Format("转入{0}账户，流水号{1}", game.GetDescription(), sourceId));

                            //#1.3.2 更新游戏账户的余额+可转出金额
                            account.Money = amount;
                            account.UpdateAt = DateTime.Now;
                            account.Withdraw += withdraw;
                            account.Update(db, t => t.Money, t => t.UpdateAt, t => t.Withdraw);

                            //#1.3.3 更新游戏账户的提现额度
                            this.AddGameWithdraw(db, userId, game, account.Withdraw, DateTime.Now, string.Format("从余额转入{0}元，附带提现额度{1}元", money.ToString("n"), withdraw.ToString("n")));

                            //#1.3.4 解锁资金
                            if (!UserAgent.Instance().UnlockMoney(db, userId, MoneyLock.LockType.Transfer, sourceId, "转账成功"))
                            {
                                db.Rollback();
                                return false;
                            }

                            //#1.3.4 扣除转账金额
                            if (!UserAgent.Instance().AddMoneyLog(db, userId, money * -1, MoneyLog.MoneyType.TransferToGame, sourceId, string.Format("转账至{0}账户", game.GetDescription())))
                            {
                                db.Rollback();
                                return false;
                            }

                            //#1.3.5 更新转账日志状态
                            this.UpdateTransferStatus(db, sourceId, TransferLog.TransferStatus.Success, "转账成功");

                            db.Commit();
                        }

                        #endregion
                        break;
                    case TransferLog.ActionType.OUT:
                        #region 从游戏账户转至主账户

                        if (setting.Turnover != decimal.Zero && money > account.Withdraw)
                        {
                            base.Message("转出额度不足，可能原因：<br />1、第三方游戏平台获取游戏记录时间为5～30分钟。<br />2、您的消费流水未达到转出金额的{0}", setting.Turnover.ToString("P"));
                            return false;
                        }

                        //#2.1 调用接口进行提现操作
                        if (!setting.Setting.Withdraw(userId, money, sourceId.ToString(), out amount))
                        {
                            // 发生出错情况
                            if (amount == decimal.MinusOne)
                            {
                                base.Message("转出失败，系统将在5分钟检测该笔记录状态，如果成功将会自动转到您的余额上");
                                return false;
                            }
                            success = false;
                        }

                        //#2.2 加入用户主账户
                        using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
                        {
                            if (!success)
                            {
                                this.UpdateTransferStatus(db, sourceId, TransferLog.TransferStatus.Faild, "转出失败");
                                db.Commit();
                                base.Message("转出失败");
                                return false;
                            }

                            //#2.2.1 更新游戏账户余额和提现额度
                            account.Money = amount;
                            account.UpdateAt = DateTime.Now;
                            account.Withdraw -= money;
                            account.Update(db, t => t.Money, t => t.UpdateAt, t => t.Withdraw);

                            //2.2.2 更新用户主账户余额
                            if (success && !UserAgent.Instance().AddMoneyLog(db, userId, money, MoneyLog.MoneyType.TransferToSite, sourceId, string.Format("从{0}转出", game.GetDescription())))
                            {
                                db.Rollback();
                                success = false;
                            }

                            //2.2.3 更新用户的可提现额度
                            if (success && !UserAgent.Instance().WithdrawLog(db, userId, money, string.Format("从{0}转出", game.GetDescription())))
                            {
                                db.Rollback();
                                success = false;
                            }

                            if (success)
                            {
                                //#1.3.5 更新转账日志状态
                                this.UpdateTransferStatus(db, sourceId, TransferLog.TransferStatus.Success, "转账成功");
                                db.Commit();
                            }
                        }

                        #endregion
                        break;
                }

                if (!success)
                {
                    SystemAgent.Instance().AddSystemLog(SiteInfo.ID, string.Format("[额度转账]用户{0}进行{1}操作在本地处理时候失败，信息：{2}", userId, action, this.Message()));
                }
                else
                {

                }

                return success;
            }
        }

        /// <summary>
        /// 预备转换筹码
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="money">转入金额</param>
        /// <param name="sourceId">如果转入成功的预备编号</param>
        /// <returns></returns>
        public bool TransferCasino(int userId, decimal money, string payPassword, out int sourceId)
        {
            sourceId = 0;
            if (!UserAgent.Instance().CheckPayPassword(userId, payPassword))
            {
                return false;
            }
            GameType gameType = GameType.Casino;
            GameSetting setting = this.GetGameSettingInfo(gameType);
            if (!setting.IsOpen || !setting.IsSystemOpen)
            {
                base.Message("{0}接口已经关闭", gameType.GetDescription());
                return false;
            }
            decimal userMoney = UserAgent.Instance().GetUserMoney(userId);
            if (userMoney < money)
            {
                base.Message("余额不足");
                return false;
            }

            TransferLog log = new TransferLog()
            {
                SiteID = SiteInfo.ID,
                UserID = userId,
                Action = TransferLog.ActionType.IN,
                CreateAt = DateTime.Now,
                Money = money,
                Status = TransferLog.TransferStatus.None,
                Type = gameType
            };

            if (log.Add(true))
            {
                sourceId = log.ID;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 管理员扫码进行筹码兑换
        /// </summary>
        /// <param name="code"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public bool CheckTransferCasino(string code, out TransferLog log)
        {
            log = null;
            Regex regex = new Regex(@"(?<ID>\d+)\-(?<Key>\w{40})$");
            if (!regex.IsMatch(code)) return false;
            int id = int.Parse(regex.Match(code).Groups["ID"].Value);
            string key = regex.Match(code).Groups["Key"].Value;
            if (MD5.Encrypto(id.ToString()) != key)
            {
                return false;
            }
            log = this.GetTransferInfo(id);
            if (log == null) return false;
            if (log.Type != GameType.Casino)
            {
                base.Message("兑换类型错误");
                return false;
            }
            switch (log.Status)
            {
                case TransferLog.TransferStatus.Faild:
                    base.Message("状态错误");
                    break;
                case TransferLog.TransferStatus.Error:
                    base.Message("订单错误");
                    break;
                case TransferLog.TransferStatus.Success:
                    base.Message("该订单已于{0}兑换成功", log.CheckAt);
                    break;
                case TransferLog.TransferStatus.None:
                    if (log.CreateAt < DateTime.Now.AddMinutes(-5))
                    {
                        base.Message("订单已超时，请重新生成");
                        return false;
                    }
                    break;
            }
            if (log.Status != TransferLog.TransferStatus.None)
            {
                return false;
            }
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                if (!UserAgent.Instance().AddMoneyLog(db, log.UserID, log.Money * -1, MoneyLog.MoneyType.TransferToGame, log.ID, "兑换筹码"))
                {
                    db.Rollback();
                    return false;
                }

                log.Status = TransferLog.TransferStatus.Success;
                log.CheckAt = DateTime.Now;
                log.Update(db, t => t.Status, t => t.CheckAt);

                db.Commit();
            }

            AdminInfo.Log(Common.Admins.AdminLog.LogType.Money, "审核筹码兑换，编号：{0},金额：{1}元", log.ID, log.Money.ToString("c"));

            return true;
        }

        /// <summary>
        /// 获取转账详情
        /// </summary>
        /// <param name="transferId"></param>
        /// <returns></returns>
        public TransferLog GetTransferInfo(int transferId, int siteId = 0)
        {
            if (siteId == 0) siteId = SiteInfo.ID;

            TransferLog log = BDC.TransferLog.Where(t => t.SiteID == siteId && t.ID == transferId).FirstOrDefault();
            return log;
        }

        /// <summary>
        /// 检查转账信息并且处理（适用于非web程序）
        /// </summary>
        /// <param name="transferId"></param>
        public bool CheckTransfer(int transferId, int siteId)
        {
            TransferLog log = this.GetTransferInfo(transferId, siteId);
            if (log == null || log.Status != TransferLog.TransferStatus.None)
            {
                base.Message("当前状态为{0}", log == null ? "不存在" : log.Status.GetDescription());
                return false;
            }
            IGame.TransferStatus status = log.Check();
            if (status == IGame.TransferStatus.None)
            {
                base.Message("远程接口返回{0}", status.GetDescription());
                return false;
            }

            MoneyLock moneyLock = UserAgent.Instance().GetMoneyLockInfo(log.UserID, log.ID, MoneyLock.LockType.Transfer, log.SiteID);
            MoneyLog moneyLog = UserAgent.Instance().GetMoneyLogInfo(log.UserID, log.Action == TransferLog.ActionType.IN ? MoneyLog.MoneyType.TransferToGame : MoneyLog.MoneyType.TransferToSite, log.ID, log.SiteID);

            using (DbExecutor db = NewExecutor(IsolationLevel.ReadCommitted))
            {
                switch (log.Action)
                {
                    case TransferLog.ActionType.IN:
                        #region 资金转入操作
                        if (moneyLock == null)
                        {
                            log.Status = TransferLog.TransferStatus.Error;
                            log.Description = "锁定资金记录不存在";
                        }
                        else if (moneyLock.UnLockAt.Year > 2000)
                        {
                            log.Status = TransferLog.TransferStatus.Error;
                            log.Description = "资金已被解锁";
                        }
                        else
                        {
                            if (moneyLock.Money != log.Money)
                            {
                                this.Message("锁定金额不等于转账金额");
                                return false;
                            }
                            //#1.1 解锁资金
                            if (!UserAgent.Instance().UnlockMoney(db, log.UserID, MoneyLock.LockType.Transfer, log.ID, string.Format("转账状态：{0}", status.GetDescription())))
                            {
                                this.Message("锁定资金记录不存在");
                                db.Rollback();
                                return false;
                            }

                            switch (status)
                            {
                                case IGame.TransferStatus.Faild:
                                case IGame.TransferStatus.Other:

                                    log.Status = TransferLog.TransferStatus.Faild;
                                    log.Description = string.Format("状态查询：{0}", status.GetDescription());
                                    break;
                                case IGame.TransferStatus.Success:
                                    if (moneyLog != null)
                                    {
                                        log.Status = TransferLog.TransferStatus.Error;
                                        log.Description = string.Format("转账资金记录已存在，编号：{0}", moneyLog.ID);
                                    }
                                    else
                                    {
                                        // #1.2 扣除金额
                                        if (!UserAgent.Instance().AddMoneyLog(db, log.UserID, log.Money * -1, MoneyLog.MoneyType.TransferToGame, log.ID, string.Format("转账到{0}账户", log.Type.GetDescription())))
                                        {
                                            db.Rollback();
                                            return false;
                                        }
                                        log.Status = TransferLog.TransferStatus.Success;
                                        log.Description = string.Format("对账成功");
                                    }
                                    break;
                            }
                        }
                        #endregion
                        break;
                    case TransferLog.ActionType.OUT:
                        #region 资金转出操作
                        if (moneyLog != null)
                        {
                            log.Status = TransferLog.TransferStatus.Error;
                            log.Description = "资金记录已经存在";
                        }
                        else
                        {
                            switch (status)
                            {
                                case IGame.TransferStatus.Faild:
                                case IGame.TransferStatus.Other:
                                    log.Status = TransferLog.TransferStatus.Faild;
                                    log.Description = string.Format("转账状态：{0}", status.GetDescription());
                                    break;
                                case IGame.TransferStatus.Success:
                                    log.Status = TransferLog.TransferStatus.Success;
                                    log.Description = string.Format("转账状态：{0}", status.GetDescription());
                                    if (!UserAgent.Instance().AddMoneyLog(db, log.UserID, log.Money, MoneyLog.MoneyType.TransferToSite, log.ID, string.Format("从{0}账户转出", log.Type.GetDescription())))
                                    {
                                        db.Rollback();
                                        return false;
                                    }
                                    break;
                            }
                        }
                        #endregion
                        break;
                }

                if (!this.UpdateTransferStatus(db, log.ID, log.Status, log.Description))
                {
                    base.Message("修改转账状态失败");
                    db.Rollback();
                    return false;
                }
                db.Commit();
            }

            if (log.Status == TransferLog.TransferStatus.Success)
            {
                UserAgent.Instance().UpdateGameAccountMoney(log.UserID, log.Type);
            }

            return true;
        }

        /// <summary>
        /// 根据用户ID创建一个新的玩家名字
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="length">指定的长度</param>
        /// <returns></returns>
        public string CreatePlayerName(int userId, int length = 10)
        {
            string user = UserAgent.Instance().GetUserName(userId);
            if (user.Length > 9) user = user.Substring(0, 9);
            if (!WebAgent.IsUserNameByEnglish(user)) user = userId.ToString().PadLeft(6, '0');
            return user + Guid.NewGuid().ToString("N").Substring(0, length - user.Length);
        }

        /// <summary>
        /// 增减上分额度（允许余额出现负数）
        /// </summary>
        /// <param name="db"></param>
        /// <param name="type"></param>
        /// <param name="money">换算之前的额度（实际转入转出金额）</param>
        /// <param name="sourceId">转账编号（如果是上分的话则为0）</param>
        /// <param name="description">为空生成默认备注</param>
        /// <returns></returns>
        public bool AddMoneyLog(DbExecutor db, GameType type, decimal money, int sourceId, string description = null)
        {
            GameSetting setting = new GameSetting() { SiteID = SiteInfo.ID, Type = type }.Info(db);
            if (setting == null || setting.Rate == decimal.Zero)
            {
                base.Message("未开启游戏");
                return false;
            }

            money = setting.Rate * money;

            setting.Money += money;
            setting.Update(db, t => t.Money);

            return new GameMoneyLog()
            {
                SiteID = SiteInfo.ID,
                Type = type,
                Balance = setting.Money,
                Money = money,
                CreateAt = DateTime.Now,
                Description = description,
                SourceID = sourceId
            }.Add(db);
        }

        /// <summary>
        /// 获取没有开奖的体育注单
        /// </summary>
        /// <returns></returns>
        public List<SportLog> GetSportLogByNone()
        {
            return BDC.SportLog.Where(t => t.Status == LogStatus.None && t.PlayAt > DateTime.Now.AddDays(-7)).ToList();
        }

        /// <summary>
        /// 获取第三方游戏的报表
        /// </summary>
        /// <param name="teamId">团队编号</param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <param name="isSelf"></param>
        /// <returns></returns>
        public List<UserGameReport> GetUserGameReport(int userId, int teamId, DateTime startAt, DateTime endAt, bool isSelf)
        {
            int[] users = userId != 0 ?
                new int[] { userId } :
                BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.AgentID == teamId).Select(t => t.ID).ToArray();
            List<UserGameReport> list = new List<UserGameReport>();
            if (users.Length == 0) return list;
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_GameReport",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", string.Join(",", users)),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt),
                    NewParam("@IsSelf", isSelf));
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new UserGameReport(dr));
                }
                return list;
            }
        }

        /// <summary>
        /// 获取用户余额
        /// </summary>
        /// <param name="type">需要用到的排序字段</param>
        /// <returns></returns>
        public List<UserGameCredit> GetCreditList(GameType type, int pageIndex, int pageSize, out int recordCount)
        {
            if (!Enum.IsDefined(typeof(GameType), type)) type = (GameType)1;
            using (DbExecutor db = NewExecutor())
            {
                string countSql = "SELECT COUNT(*) FROM (SELECT UserID FROM usr_Game WHERE SiteID = @SiteID GROUP BY UserID) a";
                recordCount = (int)db.ExecuteScalar(CommandType.Text, countSql,
                    NewParam("@SiteID", SiteInfo.ID));

                string sql = string.Format(@"WITH arg1 AS(SELECT * , ROW_NUMBER() OVER(ORDER BY [{0}] DESC) as ROWS FROM UserGameCredit WHERE SiteID = @SiteID)
SELECT * FROM arg1 WHERE ROWS BETWEEN @Start AND @End", (int)type);

                DataSet ds = db.GetDataSet(CommandType.Text, sql,
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@Start", (pageIndex - 1) * pageSize + 1),
                    NewParam("@End", pageIndex * pageSize));

                List<UserGameCredit> list = new List<UserGameCredit>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new UserGameCredit(dr));
                }

                return list;
            }
        }
    }
}
