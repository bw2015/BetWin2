using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Transactions;

using SP.Studio.Model;
using BW.Agent;

using SP.Studio.Core;
using BW.Common.Sites;
using BW.Common.Games;
using BW.Common.Users;
using BW.Common.Reports;
using SP.Studio.Array;
using SP.Studio.Web;


namespace BW.Handler.user
{
    public class money : IHandler
    {
        #region ============  充值   ================

        /// <summary>
        /// 提交充值（返回html）
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void recharge(HttpContext context)
        {
            Guid session = QF("Session", Guid.Empty);
            int userId = 0;
            if (session != Guid.Empty)
            {
                userId = UserAgent.Instance().GetUserID(session);
            }

            if (userId == 0)
            {
                this.CheckUserLogin(context);
                userId = UserInfo.ID;
            }
            decimal money = QF("Money", 0.00M);
            int payId = QF("PayID", 0);
            if (!UserAgent.Instance().Recharge(userId, payId, money, QF("BankCode")))
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }
        }

        /// <summary>
        /// 根据订单号提交充值订单（不需要登录）
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void rechargeorderid(HttpContext context)
        {
            long orderId = QF("OrderID", (long)0);
            if (!UserAgent.Instance().Recharge(orderId))
            {
                context.Response.Write(false, UserAgent.Instance().Message());
            }
        }

        /// <summary>
        /// 查询订单是否支付成功
        /// </summary>
        /// <param name="context"></param>
        private void rechargequery(HttpContext context)
        {
            long orderId = QF("OrderID", (long)0);
            RechargeOrder order = UserAgent.Instance().GetRechargeOrderInfo(orderId);
            if (order == null)
            {
                context.Response.Write(false, "订单号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                IsPayment = order.IsPayment ? 1 : 0
            });
        }

        /// <summary>
        /// 获取充值订单号
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void getrechargeid(HttpContext context)
        {
            decimal money = QF("Money", 0.00M);
            int payId = QF("PayID", 0);
            int userId = 0;

            Guid session = QF("Session", Guid.Empty);
            if (session == Guid.Empty)
            {
                this.CheckUserLogin(context);
                userId = UserInfo.ID;
            }
            else
            {
                userId = UserAgent.Instance().GetUserID(session);
            }

            long orderId = UserAgent.Instance().CreateRechargeOrder(userId, payId, money, QF("BankCode"));
            bool wechat = false;
            if (orderId != 0)
            {
                wechat = SiteAgent.Instance().GetPaymentSettingInfo(payId).PaymentObject.IsWechat();
            }
            this.ShowResult(context, orderId != 0, "充值申请提交成功", new
            {
                OrderID = orderId,
                Bank = QF("BankCode"),
                Wechat = wechat ? 1 : 0
            });
        }

        /// <summary>
        /// 充值记录
        /// </summary>
        /// <param name="context"></param>
        private void rechargelog(HttpContext context)
        {
            Dictionary<int, string> payment = SiteAgent.Instance().GetPaymentSettingList(true).ToDictionary(t => t.ID, t => t.Name);

            IQueryable<RechargeOrder> list = BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > QF("StartAt", SiteInfo.StartDate) && t.CreateAt < QF("EndAt", DateTime.Now).AddDays(1));
            if (QF("IsPayment", 0) == 1) list = list.Where(t => t.IsPayment);
            switch (QF("View", 0))
            {
                case 1:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 2:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 3:
                    list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")) && BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID);
                    break;
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Payment = payment.ContainsKey(t.PayID) ? payment[t.PayID] : "N/A",
                Money = t.Money,
                t.IsPayment,
                t.PayAt,
                t.CreateAt
            }, new
            {
                Total = this.Show(list.Sum(t => (decimal?)t.Amount))
            }));
        }


        /// <summary>
        /// 提交充值到帐信息
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void transferorder(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().SaveTransferOrder(QF("PayID", (long)0), QF("Money", decimal.Zero), QF("Name"), QF("PaymentTime", DateTime.Now), QF("Description")), "充值信息提交成功");
        }

        /// <summary>
        /// 充值详情
        /// </summary>
        /// <param name="context"></param>
        private void getrechargeinfo(HttpContext context)
        {
            long id = QF("id", (long)0);
            RechargeOrder order = UserAgent.Instance().GetRechargeOrderInfo(id);
            if (order == null)
            {
                context.Response.Write(false, "编号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                order.ID,
                IsPayment = order.IsPayment ? "true" : "false",
                UserName = UserAgent.Instance().GetUserName(order.UserID),
                order.UserID,
                Payment = SiteAgent.Instance().GetPaymentSettingInfo(order.PayID).Name,
                order.CreateAt,
                order.PayAt,
                order.SystemID,
                order.Money,
                order.Amount
            });
        }

        #endregion

        /// <summary>
        /// 提现信息
        /// </summary>
        /// <param name="context"></param>
        private void withdrawinfo(HttpContext context)
        {
            this.CheckUserLogin(context);

            if (string.IsNullOrEmpty(UserInfo.PayPassword))
            {
                context.Response.Write(false, "暂未设置资金密码", new
                {
                    Type = ErrorType.PayPassword
                });
            }
            List<BankAccount> bankList = UserAgent.Instance().GetBankAccountList(UserInfo.ID);
            if (bankList.Count == 0)
            {
                context.Response.Write(false, "未绑定银行卡号", new
                {
                    Type = ErrorType.BankAccount
                });
            }

            if (bankList.Count(t => t.IsWithdraw) == 0)
            {
                context.Response.Write(false, "当前提现账号不可用");
            }

            decimal money = UserAgent.Instance().GetUserMoney(UserInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Money = money,
                Withdraw = SiteInfo.Setting.Turnover == decimal.Zero ? money : Math.Min(money, UserInfo.Withdraw),
                MaxWithdraw = SiteInfo.Setting.WithdrawMax,
                MinWithdraw = SiteInfo.Setting.WithdrawMin,
                BankAccount = new JsonString("[" + string.Join(",", bankList.Select(t => t.ToJson())) + "]")
            });
        }

        /// <summary>
        /// 提交提现
        /// </summary>
        /// <param name="context"></param>
        private void withdraw(HttpContext context)
        {
            this.CheckUserLogin(context);
            decimal money = QF("Money", 0.00M);
            int bankId = QF("BankID", 0);
            string payPassword = QF("PayPassword");

            this.ShowResult(context, UserAgent.Instance().Withdraw(UserInfo.ID, money, bankId, payPassword),
                "提现成功");
        }

        /// <summary>
        /// 提现记录
        /// </summary>
        /// <param name="context"></param>
        private void withdrawlist(HttpContext context)
        {
            IQueryable<WithdrawOrder> list = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.Money,
                t.CreateAt,
                Status = t.Status.GetDescription(),
                t.Description,
                Account = t.ToString()
            }));
        }


        /// <summary>
        /// 提现日志
        /// </summary>
        /// <param name="context"></param>
        private void withdrawlog(HttpContext context)
        {
            IQueryable<WithdrawOrder> list = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > QF("StartAt", SiteInfo.StartDate) && t.CreateAt < QF("EndAt", DateTime.Now).AddDays(1));
            switch (QF("View", 0))
            {
                case 1:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 2:
                    list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                case 3:
                    list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")) && BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                    break;
                default:
                    list = list.Where(t => t.UserID == UserInfo.ID);
                    break;
            }
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Money,
                t.Fee,
                TotalMoney = t.Money + t.Fee,
                t.CreateAt,
                Status = t.Status.GetDescription(),
                t.Appointment,
                t.Description
            }, new
            {
                Total = this.Show(list.Sum(t => (decimal?)t.Money + t.Fee)),
                Fee = this.Show(list.Sum(t => (decimal?)t.Fee))
            }));
        }

        /// <summary>
        /// 提现详情
        /// </summary>
        /// <param name="context"></param>
        private void getwithdrawinfo(HttpContext context)
        {
            WithdrawOrder order = UserAgent.Instance().GetWithdrawOrderInfo(QF("ID", 0));
            if (order == null || order.UserID != UserInfo.ID)
            {
                context.Response.Write(false, "编号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                order.ID,
                UserName = UserAgent.Instance().GetUserName(order.UserID),
                order.Money,
                order.Fee,
                TotalMoney = order.Money + order.Fee,
                order.CreateAt,
                Status = order.Status.GetDescription(),
                order.Appointment,
                order.Description
            });
        }

        /// <summary>
        /// 帐变记录
        /// </summary>
        /// <param name="context"></param>
        private void moneylog(HttpContext context)
        {
            int tableId = UserInfo.ID.GetTableID();
            using (TransactionScope tran = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadUncommitted
            }))
            {
                IQueryable<MoneyLog> list = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > QF("StartAt", SiteInfo.StartDate));

                if (!string.IsNullOrEmpty(QF("Type")))
                {
                    MoneyLog.MoneyType[] type = QF("Type").Split(',').Select(t => t.ToEnum<MoneyLog.MoneyType>()).Where(t => t != MoneyLog.MoneyType.None).ToArray();
                    if (type.Length == 0)
                    {
                        list = list.Where(t => t.ID == 0);
                    }
                    else if (type.Length == 1)
                    {
                        list = list.Where(t => t.Type == type[0]);
                    }
                    else
                    {
                        list = list.Where(t => type.Contains(t.Type));
                    }

                }

                switch (QF("View", 0))
                {
                    case 1: // 直属下级
                        list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.Depth == 1).Select(p => p.ChildID).Contains(t.UserID));
                        break;
                    case 2: // 全部下级
                        list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID));
                        break;
                    case 3: // 指定用户
                        int childId = UserAgent.Instance().GetUserID(QF("User"));
                        list = list.Where(t => BDC.UserDepth.Where(p => p.UserID == UserInfo.ID && p.ChildID == childId).Select(p => p.ChildID).Contains(t.UserID) && t.TableID == childId.GetTableID());
                        break;
                    default:
                        list = list.Where(t => t.UserID == UserInfo.ID && t.TableID == tableId);
                        break;
                }

                list = list.Where(t => t.CreateAt < QF("EndAt", DateTime.Now.Date).AddDays(1));

                context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
                {
                    t.ID,
                    UserName = UserAgent.Instance().GetUserName(t.UserID),
                    Type = t.Type.GetDescription(),
                    t.Money,
                    t.Balance,
                    t.CreateAt,
                    t.Description
                }));
            }
        }

        /// <summary>
        /// 获取单条帐变的详情
        /// </summary>
        /// <param name="context"></param>
        private void moneyloginfo(HttpContext context)
        {
            int id = QF("ID", 0);
            MoneyLog log = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).FirstOrDefault();
            if (log == null || (log.UserID != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, log.ID)))
            {
                context.Response.Write(false, "编号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                log.ID,
                UserName = UserAgent.Instance().GetUserName(log.UserID),
                Type = log.Type.GetDescription(),
                log.Money,
                log.Balance,
                log.CreateAt,
                log.Description
            });

        }

        /// <summary>
        /// 资金分类
        /// </summary>
        /// <param name="context"></param>
        private void moneycategory(HttpContext context)
        {
            context.Response.Write(true, "资金类型", this.ShowResult(typeof(MoneyLog.MoneyCategoryType).ToList(), t => new
            {
                text = t.Description,
                value = t.Name
            }));
        }

        /// <summary>
        /// 资金类型
        /// </summary>
        /// <param name="context"></param>
        private void moneytype(HttpContext context)
        {
            
            MoneyLog.MoneyType[] type = BDC.UserDateMoney.Where(t => t.SiteID == SiteInfo.ID && 
                (t.UserID == UserInfo.ID || BDC.UserDepth.Where(p => p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.UserID))).GroupBy(t => t.Type).Select(t => t.Key).ToArray();


            //List<MoneyLog.MoneyType> list = new List<MoneyLog.MoneyType>();
            //foreach (MoneyLog.MoneyType obj in Enum.GetValues(typeof(MoneyLog.MoneyType)))
            //{
            //    if (obj == MoneyLog.MoneyType.None || !type.Contains(obj)) continue;
            //    list.Add(obj);
            //}

            Dictionary<MoneyLog.MoneyCategoryType, IEnumerable<MoneyLog.MoneyType>> dic =
                type.GroupBy(t => t.GetCategory()).ToDictionary(
                    t => t.Key,
                    t => type.Where(p => p.GetCategory() == t.Key)
                    );

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append(string.Join(",", dic.Select(t => string.Concat("\"", t.Key.GetDescription(), "\":{",
                string.Join(",", t.Value.Select(p => string.Format("\"{0}\":\"{1}\"", p, p.GetDescription())))
                , "}"))));
            sb.Append("}");

            context.Response.Write(true, "资金类型", sb.ToString());
        }

        /// <summary>
        /// 锁定明细
        /// </summary>
        /// <param name="context"></param>
        private void lockmoney(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(UserAgent.Instance().GetMoneyLockList(UserInfo.ID), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.Money,
                t.LockAt,
                t.Description
            }));
        }



        /// <summary>
        /// 盈亏统计（今日、昨日）
        /// </summary>
        /// <param name="context"></param>
        private void winstatistic(HttpContext context)
        {
            Dictionary<MoneyLog.MoneyType, decimal> today =
                BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.CreateAt > DateTime.Now.Date).GroupBy(t => t.Type).ToDictionary(t => t.Key, t => t.Sum(p => p.Money));

            Dictionary<MoneyLog.MoneyType, decimal> yesterday =
                BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.CreateAt > DateTime.Now.Date.AddDays(-1) && t.CreateAt < DateTime.Now.Date).GroupBy(t => t.Type).ToDictionary(t => t.Key, t => t.Sum(p => p.Money));

            MoneyLog.MoneyCategoryType[] category = new MoneyLog.MoneyCategoryType[] { MoneyLog.MoneyCategoryType.Other, MoneyLog.MoneyCategoryType.Transfer, MoneyLog.MoneyCategoryType.Recharge, MoneyLog.MoneyCategoryType.Withdraw };
            decimal todayMoney = this.Show(UserAgent.Instance().GetMoneyCategory(today).Where(t => !category.Contains(t.Key)).Sum(t => (decimal?)t.Value))
                + this.Show(BDC.GameLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.CreateAt > DateTime.Now.Date).Sum(t => (decimal?)t.Money));

            decimal yesterdayMoney = this.Show(UserAgent.Instance().GetMoneyCategory(yesterday).Where(t => !category.Contains(t.Key)).Sum(t => (decimal?)t.Value))
                + this.Show(BDC.GameLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.CreateAt > DateTime.Now.Date.AddDays(-1) && t.CreateAt < DateTime.Now.Date).Sum(t => (decimal?)t.Money));


            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Today = todayMoney,
                Yesterday = yesterdayMoney,
                Recharge = this.Show(BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.IsPayment).Sum(t => (decimal?)t.Amount)),
                Withdraw = this.Show(BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.Status == WithdrawOrder.WithdrawStatus.Finish).Sum(t => (decimal?)t.Money))
            });
        }

        /// <summary>
        /// 会员个人报表
        /// </summary>
        /// <param name="context"></param>
        private void userstatistic(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            int parentId = QF("ParentID", 0);
            int userId = QF("ID", UserInfo.ID);
            if (!UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
            {
                context.Response.Write(false, "用户ID错误");
            }

            Dictionary<MoneyLog.MoneyType, decimal> moneylog = BDC.UserDateMoney.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Date >= startAt && t.Date <= endAt).GroupBy(t => t.Type).ToDictionary(t => t.Key, t => t.Sum(p => p.Money));
            Dictionary<BW.Common.Games.GameType, decimal> gamelog = BDC.UserDateGame.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Date >= startAt && t.Date <= endAt).GroupBy(t => t.Type).ToDictionary(t => t.Key, t => t.Sum(p => p.Money));

            UserReport report = new UserReport(userId, moneylog, gamelog);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Category = new JsonString(typeof(MoneyLog.MoneyCategoryType).ToList().ToDictionary(t => t.Name, t => t.Description).ToJson()),
                Game = new JsonString(GameAgent.Instance().GetGameSetting().Where(t => t.IsOpen).ToDictionary(t => t.Type, t => t.Type.GetDescription()).ToJson()),
                MoneyLog = new JsonString(report.data.ToJson()),
                GameLog = new JsonString(report.game.ToJson()),
                Total = report.Money
            });
        }

        /// <summary>
        /// 团队报表
        /// </summary>
        /// <param name="context"></param>
        private void teamstatistic(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            string user = QF("User");
            int parentId = UserInfo.ID;
            bool self = true;
            if (!string.IsNullOrEmpty(user))
            {
                parentId = UserAgent.Instance().GetUserID(user);
                self = false;
            }
            string parentUser = string.Empty;

            if (parentId != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, parentId))
            {
                context.Response.Write(false, "用户ID错误");
            }

            int agentId = parentId == UserInfo.ID ? UserInfo.ID : UserAgent.Instance().GetAgentID(parentId);
            if (!UserAgent.Instance().IsUserChild(UserInfo.ID, agentId))
            {
                agentId = UserInfo.ID;
            }
            parentUser = agentId == UserInfo.ID ? string.Empty : UserAgent.Instance().GetUserName(agentId);

            List<UserReport> list = UserAgent.Instance().GetTeamStatistic(parentId, startAt, endAt, self);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Money,
                Category = new JsonString(t.data.ToJson()),
                Game = new JsonString(t.game.ToJson())
            }, new
            {
                Category = new JsonString(typeof(MoneyLog.MoneyCategoryType).ToList().ToDictionary(t => t.Name, t => t.Description).ToJson()),
                Game = new JsonString(GameAgent.Instance().GetGameSetting().Where(t => t.IsOpen).ToDictionary(t => t.Type, t => t.Type.GetDescription()).ToJson()),
                Total = this.Show(list.Sum(t => (decimal?)t.Money)),
                Parent = parentUser
            }));
        }

        /// <summary>
        /// 团队的总报表
        /// </summary>
        /// <param name="context"></param>
        private void teamreport(HttpContext context)
        {
            string user = QF("User");
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            int userId = UserInfo.ID;
            if (!string.IsNullOrEmpty(user))
            {
                userId = UserAgent.Instance().GetUserID(user);
                if (userId == 0 || !UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
                {
                    context.Response.Write(false, "用户名错误");
                }
            }
            Dictionary<MoneyLog.MoneyType, decimal> _money = UserAgent.Instance().GetTeamReport(userId, startAt, endAt, true);
            Dictionary<GameType, decimal> _game = UserAgent.Instance().GetTeamGameReportByType(userId, startAt, endAt);
            UserReport report = new UserReport(userId, _money, _game);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Money = new JsonString(report.data.ToJson()),
                Game = new JsonString(report.game.ToJson()),
                Category = new JsonString(typeof(MoneyLog.MoneyCategoryType).ToList().ToDictionary(t => t.Name, t => t.Description).ToJson()),
                GameType = new JsonString(GameAgent.Instance().GetGameSetting().Where(t => t.IsOpen).ToDictionary(t => t.Type, t => t.Type.GetDescription()).ToJson()),
                Total = report.Money,
                User = UserAgent.Instance().GetUserName(userId),
                StartAt = startAt,
                EndAt = endAt.AddDays(-1)
            });
        }

        /// <summary>
        /// 团队的游戏报表
        /// </summary>
        /// <param name="context"></param>
        private void teamgamestatistic(HttpContext context)
        {
            int userId = QF("ID", UserInfo.ID);
            if (userId != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
            {
                context.Response.Write(false, "用户编号错误");
            }
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            GameReport report = UserAgent.Instance().GetTeamGameReport(userId, startAt, endAt);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                report.UserID,
                UserName = UserAgent.Instance().GetUserName(report.UserID),
                report.VideoBet,
                report.VideoMoney,
                report.SlotBet,
                report.SlotMoney,
                report.SportBet,
                report.SportMoney
            });
        }

        /// <summary>
        /// 获取用户团队的业绩报表
        /// </summary>
        /// <param name="context"></param>
        private void getuserteamreport(HttpContext context)
        {
            int userId = QF("ID", UserInfo.ID);
            if (userId != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
            {
                context.Response.Write(false, "用户编号错误");
            }
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            Dictionary<MoneyLog.MoneyType, decimal> dic = UserAgent.Instance().GetTeamReport(userId, startAt, endAt, true);

            UserReport report = new UserReport(userId, dic, null);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(report.data, t => new
            {
                Type = t.Key.GetDescription(),
                Money = t.Value
            }, new
            {
                Total = report.Money
            }));
        }

        /// <summary>
        /// 用户的游戏报表
        /// </summary>
        /// <param name="context"></param>
        private void getusergamereport(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            Dictionary<MoneyLog.MoneyType, decimal> lottery = BDC.UserDateMoney.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID
            && t.Date >= startAt && t.Date < endAt && new MoneyLog.MoneyType[] { MoneyLog.MoneyType.Bet, MoneyLog.MoneyType.Reward }.Contains(t.Type))
            .GroupBy(t => t.Type).Select(t => new { Type = t.Key, Money = t.Sum(p => p.Money) }).ToDictionary(t => t.Type, t => t.Money);

            var video = BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.EndAt > startAt && t.EndAt < startAt).GroupBy(t => 0).Select(t => new
            {
                BetAmount = t.Sum(p => p.BetAmount),
                Money = t.Sum(p => p.Money)
            }).FirstOrDefault();

            var slot = BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.PlayAt > startAt && t.PlayAt < startAt).GroupBy(t => 0).Select(t => new
            {
                BetAmount = t.Sum(p => p.BetAmount),
                Money = t.Sum(p => p.Money)
            }).FirstOrDefault();

            var sport = BDC.SportLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.PlayAt > startAt && t.PlayAt < startAt).GroupBy(t => 0).Select(t => new
            {
                BetAmount = t.Sum(p => p.BetMoney),
                Money = t.Sum(p => p.Money)
            }).FirstOrDefault();

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Bet = lottery.Get(MoneyLog.MoneyType.Bet, decimal.Zero),
                Reward = lottery.Get(MoneyLog.MoneyType.Reward, decimal.Zero) - lottery.Get(MoneyLog.MoneyType.Bet, decimal.Zero),
                VideoBet = video == null ? decimal.Zero : video.BetAmount,
                VideoMoney = video == null ? decimal.Zero : video.Money,
                SlotBet = slot == null ? decimal.Zero : slot.BetAmount,
                SlotMoney = slot == null ? decimal.Zero : slot.Money,
                SportBet = sport == null ? decimal.Zero : sport.BetAmount,
                SportMoney = sport == null ? decimal.Zero : sport.Money
            });
        }

        /// <summary>
        /// 获取用户的第三方游戏输赢报表
        /// </summary>
        /// <param name="context"></param>
        private void getusergametypereport(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            var list = BDC.UserDateGame.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.Date >= startAt && t.Date < endAt).GroupBy(t => t.Type).Select(t => new
            {
                Type = t.Key,
                Bet = t.Sum(p => p.Amount),
                Money = t.Sum(p => p.Money)
            });

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                Type = t.Type.GetDescription(),
                t.Bet,
                t.Money
            }));


        }
    }
}
