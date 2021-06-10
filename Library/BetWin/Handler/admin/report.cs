using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Web;
using System.Data;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Reports;
using BW.Common.Games;
using BW.Common.Lottery;

using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Web;
using SP.Studio.Model;
using SP.Studio.Data;


namespace BW.Handler.admin
{
    public class report : IHandler
    {
        /// <summary>
        /// 获取资金流水日志
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.会员数据.会员帐变.Value)]
        private void moneylog(HttpContext context)
        {
            IQueryable<MoneyLog> list = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID);

            if (!string.IsNullOrEmpty(QF("UserName"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("UserName")));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<MoneyLog.MoneyType>());
            if (!string.IsNullOrEmpty(QF("IP"))) list = list.Where(t => t.IP == QF("IP"));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));
            if (QF("MinMoney", decimal.Zero) != decimal.Zero) list = list.Where(t => Math.Abs(t.Money) >= QF("MinMoney", decimal.Zero));
            if (QF("MaxMoney", decimal.Zero) != decimal.Zero) list = list.Where(t => Math.Abs(t.Money) <= QF("MaxMoney", decimal.Zero));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Type = t.Type.GetDescription(),
                t.Money,
                t.Balance,
                t.CreateAt,
                t.IP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.IP),
                t.Description
            }, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 查询历史帐变记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.会员数据.会员帐变.Value)]
        private void moneyhistory(HttpContext context)
        {
            IQueryable<MoneyHistory> list = BDC.MoneyHistory.Where(t => t.SiteID == SiteInfo.ID);

            DateTime updateAt = SystemAgent.Instance().GetMoneyUpdateAt();

            if (!string.IsNullOrEmpty(QF("UserName"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("UserName")));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<MoneyLog.MoneyType>());
            if (!string.IsNullOrEmpty(QF("IP"))) list = list.Where(t => t.IP == QF("IP"));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));
            if (QF("MinMoney", decimal.Zero) != decimal.Zero) list = list.Where(t => Math.Abs(t.Money) >= QF("MinMoney", decimal.Zero));
            if (QF("MaxMoney", decimal.Zero) != decimal.Zero) list = list.Where(t => Math.Abs(t.Money) <= QF("MaxMoney", decimal.Zero));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Type = t.Type.GetDescription(),
                t.Money,
                t.Balance,
                t.CreateAt,
                t.IP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.IP),
                t.Description
            }, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money)),
                UpdateAt = updateAt
            }));
        }

        /// <summary>
        /// 团队的充提统计报表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.平台数据.充提统计.Value)]
        private void payment(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("User"));
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            Dictionary<string, object> dic = new Dictionary<string, object>();

            //UserID,UserName,Recharge,Withdraw
            DataSet ds = SystemAgent.Instance().GetProcReport("rpt_TeamPayment", "SiteID", SiteInfo.ID, "UserID", userId, "StartAt", startAt, "EndAt", endAt);

            IEnumerable<DataRow> list = ds.Tables[0].Rows.ToList();
            decimal withdraw = this.Show(list.Sum(t => (decimal?)t["Withdraw"]));
            decimal recharge = this.Show(list.Sum(t => (decimal?)t["Recharge"]));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, dr => new
            {
                UserID = (int)dr["UserID"],
                UserName = (string)dr["UserName"],
                Recharge = (decimal)dr["Recharge"],
                Withdraw = (decimal)dr["Withdraw"],
                Money = (decimal)dr["Recharge"] - (decimal)dr["Withdraw"]

            }, new
            {
                Recharge = recharge,
                Withdraw = withdraw,
                Money = recharge - withdraw
            }));
        }

        /// <summary>
        /// 获取存储过程的参数
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getparameter(HttpContext context)
        {
            string name = QF("procname");
            string database = QF("database");

            List<EnumObject> list = SystemAgent.Instance().GetReportInfo(name, database);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Name,
                Default = t.Description,
                Type = t.Picture
            }, new
            {
                Name = name
            }));
        }

        /// <summary>
        /// 获取报表存储过程的数据
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getdata(HttpContext context)
        {
            string name = QF("procname");
            string database = QF("database");
            List<EnumObject> list = SystemAgent.Instance().GetReportInfo(name, database);
            IEnumerable<object[]> data = SystemAgent.Instance().GetReportData(name,
                database, SystemAgent.Instance().GetReportParameber(list));

            context.Response.Write(true, this.StopwatchMessage(context), string.Concat("[",
               string.Join(",", data.Select(t => string.Format("[{0}]",
                   string.Join(",", t.Select(p => "\"" + p.ToString() + "\"")))
                    )
                ), "]")
            );
        }

        /// <summary>
        /// 平台的充提统计
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.平台数据.充提统计.Value)]
        private void sitepayment(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            Dictionary<DateTime, decimal> recharge = BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.PayAt >= startAt && t.PayAt < endAt && t.IsPayment)
                .Join(BDC.User.Where(t => t.SiteID == SiteInfo.ID && !t.IsTest), t => t.UserID, t => t.ID, (money, user) => money).GroupBy(t => new
                {
                    t.PayAt.Year,
                    t.PayAt.Month,
                    t.PayAt.Day
                }).ToDictionary(t => new DateTime(t.Key.Year, t.Key.Month, t.Key.Day), t => t.Sum(p => p.Amount));

            Dictionary<DateTime, decimal> withdraw = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt >= startAt && t.CreateAt < endAt && t.Status == WithdrawOrder.WithdrawStatus.Finish)
                .Join(BDC.User.Where(t => t.SiteID == SiteInfo.ID && !t.IsTest), t => t.UserID, t => t.ID, (money, user) => money).GroupBy(t => new
                {
                    t.CreateAt.Year,
                    t.CreateAt.Month,
                    t.CreateAt.Day
                }).ToDictionary(t => new DateTime(t.Key.Year, t.Key.Month, t.Key.Day), t => t.Sum(p => p.Money));

            List<Tuple<DateTime, decimal, decimal>> list = new List<Tuple<DateTime, decimal, decimal>>();

            while (startAt < endAt)
            {
                list.Add(new Tuple<DateTime, decimal, decimal>(startAt, recharge.Get(startAt, decimal.Zero), withdraw.Get(startAt, decimal.Zero)));
                startAt = startAt.AddDays(1);
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                Date = t.Item1.ToLongDateString(),
                Recharge = t.Item2,
                Withdraw = t.Item3,
                Money = t.Item2 - t.Item3
            }, new
            {
                Recharge = this.Show(list.Sum(t => (decimal?)t.Item2)),
                Withdraw = this.Show(list.Sum(t => (decimal?)t.Item3)),
                Money = this.Show(list.Sum(t => (decimal?)(t.Item2 - t.Item3)))
            }));
        }

        /// <summary>
        /// 平台盈亏统计
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.平台数据.盈亏统计.Value)]
        private void sitemoney(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            var list = BDC.UserDateMoney.Where(t => t.SiteID == SiteInfo.ID && t.Date >= startAt && t.Date < endAt).Join(BDC.User.Where(t => t.SiteID == SiteInfo.ID && !t.IsTest), t => t.UserID, t => t.ID, (moneylog, user) => moneylog).GroupBy(t => new
            {
                t.Type,
                t.Date
            }).Select(t => new
            {
                Date = t.Key.Date,
                t.Key.Type,
                Money = t.Sum(p => p.Money)
            }).ToArray();

            Dictionary<DateTime, UserReport> report = new Dictionary<DateTime, UserReport>();

            Dictionary<MoneyLog.MoneyCategoryType, decimal> total = list.Select(t => new { Type = t.Type.GetCategory(), t.Money }).GroupBy(t => t.Type).Select(t => new
            {
                Type = t.Key,
                Money = t.Sum(p => p.Money)
            }).ToDictionary(t => t.Type, t => t.Money);

            while (startAt < endAt)
            {
                Dictionary<MoneyLog.MoneyType, decimal> data = list.Where(t => t.Date == startAt).ToDictionary(t => t.Type, t => t.Money);
                report.Add(startAt, new UserReport(0, data, null));
                startAt = startAt.AddDays(1);

            }


            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(report, t => new
            {
                Date = t.Key.ToLongDateString(),
                Data = new JsonString(t.Value.data.ToJson()),
                Money = t.Value.Money * -1
            }, new
            {
                Data = new JsonString(total.ToJson()),
                Money = this.Show(report.Sum(t => (decimal?)(t.Value.Money))) * -1
            }));
        }

        /// <summary>
        /// 用户的盈亏统计
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.查看会员报表)]
        private void userpayment(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            int userId = QF("UserID", 0);

            var list = BDC.UserDateMoney.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Date >= startAt && t.Date < endAt).GroupBy(t => new
            {
                t.Type,
                t.Date
            }).Select(t => new
            {
                Date = t.Key.Date,
                t.Key.Type,
                Money = t.Sum(p => p.Money)
            }).ToList();

            Dictionary<DateTime, UserReport> report = new Dictionary<DateTime, UserReport>();
            while (startAt < endAt)
            {
                Dictionary<MoneyLog.MoneyType, decimal> data = list.Where(t => t.Date == startAt).ToDictionary(t => t.Type, t => t.Money);
                report.Add(startAt, new UserReport(0, data, null));
                startAt = startAt.AddDays(1);
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(report, t => new
            {
                Date = t.Key.ToLongDateString(),
                Data = new JsonString(t.Value.data.ToJson()),
                Money = t.Value.Money
            }, new
            {
                Money = report.Sum(t => (decimal?)(t.Value.Money))
            }));
        }


        /// <summary>
        /// 个人盈亏报表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.会员数据.个人盈亏.Value)]
        private void usermoney(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            var list = BDC.UserDateMoney.Where(t => t.SiteID == SiteInfo.ID && t.Date >= startAt && t.Date < endAt && !this.TestUserList.Contains(t.UserID)).GroupBy(t => new
            {
                t.UserID,
                t.Type
            }).Select(t => new
            {
                t.Key.UserID,
                t.Key.Type,
                Money = t.Sum(p => p.Money)
            }).ToArray();

            List<UserReport> report = list.GroupBy(t => t.UserID).Select(t =>
            {
                Dictionary<MoneyLog.MoneyType, decimal> data = list.Where(p => p.UserID == t.Key).ToDictionary(p => p.Type, p => p.Money);
                return new UserReport(t.Key, data, null);
            }).OrderBy(t => t.Money).ToList();

            int index = 1;
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(report, t => new
            {
                Index = index++,
                UserID = t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Data = new JsonString(t.data.ToJson()),
                Money = t.Money
            }, new
            {
                Money = this.Show(report.Sum(t => (decimal?)(t.Money)))
            }));
        }

        /// <summary>
        /// 团队盈亏
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.会员数据.团队盈亏.Value)]
        private void teammoney(HttpContext context)
        {
            int parentId = UserAgent.Instance().GetUserID(QF("Agent"));
            bool isSelf = QF("IsSelf", 0) == 1;
            IQueryable<User> userlist = BDC.User.Where(t => t.SiteID == SiteInfo.ID);
            if (string.IsNullOrEmpty(QF("UserName")))
            {
                userlist = userlist.Where(t => t.AgentID == parentId);
            }
            else
            {
                userlist = userlist.Where(t => t.UserName == QF("UserName"));
            }
            int[] users = userlist.Select(t => t.ID).ToArray();
            StringBuilder sb = new StringBuilder();

            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);


            Dictionary<int, UserReport> list = new Dictionary<int, UserReport>();
            foreach (var item in UserAgent.Instance().GetTeamReport(users, startAt, endAt, isSelf))
            {
                list.Add(item.Key, new UserReport(item.Key, item.Value, null));
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                UserID = t.Key,
                UserName = UserAgent.Instance().GetUserName(t.Key),
                Data = new JsonString("{", string.Join(",", t.Value.data.Select(p => string.Format("\"{0}\":{1}", p.Key, p.Value))), "}"),
                Money = t.Value.Money
            }, new
            {
                Money = list.Count == 0 ? decimal.Zero : list.Sum(p => p.Value.Money)
            }));
        }


        /// <summary>
        /// 彩票报表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.游戏报表.彩票报表.Value)]
        private void lottery(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.AddDays(-1));
            DateTime endAt = QF("EndAt", DateTime.Now);

            
            var list = BDC.LotteryOrderReward.Where(t => t.SiteID == SiteInfo.ID && t.Time >= startAt && t.Time <= endAt).GroupBy(t => t.Type).Select(t => new
            {
                Type = t.Key,
                Count = t.Count(),
                BetMoney = t.Sum(p => p.Money),
                Reward = t.Sum(p => p.Reward),
                Money = t.Sum(p => p.Money - p.Reward)
            });

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Type,
                Game = LotteryAgent.Instance().GetLotteryName(t.Type),
                t.Count,
                t.BetMoney,
                t.Reward,
                t.Money,
                Return = t.BetMoney == decimal.Zero ? decimal.Zero : t.Reward / t.BetMoney
            }));
        }

        /// <summary>
        /// 玩法报表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.游戏报表.彩票报表.Value)]
        private void lotteryplayer(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.AddDays(-1));
            DateTime endAt = QF("ENdAt", DateTime.Now);
            LotteryType type = QF("Game", QF("Type")).ToEnum<LotteryType>();

            IQueryable<int> testUser = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.IsTest).Select(t => t.ID);
            var list = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > startAt && t.CreateAt < endAt && t.IsLottery && !testUser.Contains(t.UserID) && t.Type == type).GroupBy(t => t.PlayerID).Select(t => new
            {
                PlayerID = t.Key,
                Count = t.Count(),
                BetMoney = t.Sum(p => p.Money),
                Reward = t.Sum(p => p.Reward),
                Money = t.Sum(p => p.Money - p.Reward)
            });

            Dictionary<int, string> player = LotteryAgent.Instance().GetPlayerList(type).Where(t => t.ID != 0).ToDictionary(t => t.ID, t => t.Name);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderBy(t => t.Money).ToList(), t => new
            {
                t.PlayerID,
                Player = player.Get(t.PlayerID, t.PlayerID.ToString()),
                t.Count,
                t.BetMoney,
                t.Reward,
                t.Money,
                Return = t.BetMoney == decimal.Zero ? decimal.Zero : t.Reward / t.BetMoney
            }));
        }

        /// <summary>
        /// 彩种的用户报表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.游戏报表.彩票报表.Value)]
        private void lotteryuser(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.AddDays(-1));
            DateTime endAt = QF("ENdAt", DateTime.Now);
            LotteryType type = QF("Game", QF("Type")).ToEnum<LotteryType>();
            IQueryable<int> testUser = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.IsTest).Select(t => t.ID);
            var list = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > startAt && t.CreateAt < endAt && t.IsLottery && !testUser.Contains(t.UserID) && t.Type == type).GroupBy(t => t.UserID).Select(t => new
            {
                UserID = t.Key,
                Count = t.Count(),
                BetMoney = t.Sum(p => p.Money),
                Reward = t.Sum(p => p.Reward),
                Money = t.Sum(p => p.Money - p.Reward)
            });

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderBy(t => t.Money).ToList(), t => new
            {
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Count,
                t.BetMoney,
                t.Reward,
                t.Money,
                Return = t.BetMoney == decimal.Zero ? decimal.Zero : t.Reward / t.BetMoney
            }));
        }

        /// <summary>
        /// 第三方游戏的统计报表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.数据统计.游戏报表.第三方游戏.Value)]
        private void gamelist(HttpContext context)
        {
            int agentId = UserAgent.Instance().GetUserID(QF("Agent"));
            int uid = UserAgent.Instance().GetUserID(QF("User"));

            List<UserGameReport> list = GameAgent.Instance().GetUserGameReport(uid, agentId, QF("StartAt", DateTime.Now.Date), QF("EndAt", DateTime.Now.Date).AddDays(1), QF("IsSelf", 0) == 1);
            var dic = list.ToDictionary(t => t.UserID + "-" + t.Type, t => new { t.Money, t.Amount });
            GameType[] type = list.GroupBy(t => t.Type).Select(t => t.Key).ToArray();

            var resultList = list.GroupBy(t => t.UserID).Select(t => t.Key).ToDictionary(t => t, t =>
            {
                var result = list.Where(p => p.UserID == t && type.Contains(p.Type));
                return new
                {
                    Money = result.Sum(p => p.Money),
                    Amount = result.Sum(p => p.Amount)
                };
            });

            string data = string.Join(",", type.Select(t => string.Format("\"{0}\":\"{1}\"", t, t.GetDescription())));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.GroupBy(t => t.UserID).Select(t => t.Key), userId =>
            {
                List<string> result = new List<string>();
                foreach (GameType t in type)
                {
                    string key = userId + "-" + t;
                    if (dic.ContainsKey(key))
                    {
                        result.Add(string.Format("\"{0}\":{{\"Money\":{1},\"Amount\":{2}}}", t, dic[key].Money, dic[key].Amount));
                    }
                    else
                    {
                        result.Add(string.Format("\"{0}\":{{\"Money\":0,\"Amount\":0}}", t));
                    }
                }
                return new
                {
                    UserID = userId,
                    UserName = UserAgent.Instance().GetUserName(userId),
                    Game = new JsonString("{", string.Join(",", result), "}")
                };
            }, new JsonString("{" + data + "}")));
        }
    }
}
