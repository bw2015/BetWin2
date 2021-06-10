using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using SP.Studio.Core;
using SP.Studio.Model;
using BW.Framework;
using SP.Studio.Array;

using BW.Agent;
using BW.Common.Users;
using BW.Common.Sites;
using BW.GateWay.Planning;

using BW.Common.Reports;

namespace BW.Handler.user
{
    /// <summary>
    /// 团队管理
    /// </summary>
    public class team : IHandler
    {
        /// <summary>
        /// 团队成员列表
        /// </summary>
        /// <param name="context"></param>
        private void list(HttpContext context)
        {
            this.CheckUserLogin(context);
            int agentId = UserInfo.ID;
            var list = BDC.User.Where(t => t.SiteID == SiteInfo.ID);

            if (!string.IsNullOrEmpty(QF("User")))
            {
                int userId = UserAgent.Instance().GetUserID(QF("User"));
                if (!UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
                {
                    context.Response.Write(false, "您搜索的用户不存在或者不是您的下级");
                }
                list = list.Where(t => t.ID == userId);
            }
            else
            {
                if (!string.IsNullOrEmpty(QF("Agent")))
                {
                    agentId = UserAgent.Instance().GetUserID(QF("Agent"));
                }
                if (agentId != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, agentId))
                {
                    context.Response.Write(false, "当前查看的用户不是您的下级");
                }

                list = list.Where(t => t.AgentID == agentId);
            }

            Dictionary<int, decimal> betYesterday = null;
            if (QF("Bet.Yesterday", 0) == 1)
            {
                betYesterday = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.Type == MoneyLog.MoneyType.Bet && t.CreateAt > DateTime.Now.Date.AddDays(-1) && t.CreateAt < DateTime.Now.Date).
                    Join(list, t => t.UserID, t => t.ID, (log, user) => log).GroupBy(t => t.UserID).Select(t => new
                    {
                        t.Key,
                        Money = t.Sum(p => p.Money)
                    }).ToDictionary(t => t.Key, t => Math.Abs(t.Money));
            }

            JsonString parent = new JsonString("[",
                    string.Join(",", UserAgent.Instance().GetUserParentList(agentId, UserInfo.ID).Select(t => string.Format("\"{0}\"", UserAgent.Instance().GetUserName(t)))),
                    "]");

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserName,
                Type = t.Type.GetDescription(),
                t.Rebate,
                t.Money,
                t.CreateAt,
                t.IsOnline,
                t.FaceShow,
                // 是否是直属下级
                IsAgent = (t.AgentID == agentId ? 1 : 0),
                Bet = new JsonString(new
                {
                    Yesterday = betYesterday == null ? decimal.Zero : betYesterday.Get(t.ID, decimal.Zero)
                }.ToJson())
            }, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money)),
                Agent = UserAgent.Instance().GetUserName(UserAgent.Instance().GetAgentID(agentId)),
                Parent = parent,
                Bet = new JsonString(new
                {
                    Yesterday = betYesterday == null ? decimal.Zero : this.Show(betYesterday.Sum(t => (decimal?)t.Value))
                }.ToJson())
            }));
        }

        /// <summary>
        /// 查看下级的用户资料
        /// </summary>
        /// <param name="context"></param>
        private void userinfo(HttpContext context)
        {
            int userId = QF("ID", 0);
            if (userId == 0)
            {
                context.Response.Write(false, "ID错误");
            }
            if (!UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
            {
                context.Response.Write(false, "当前查看的用户不是您的下级");
            }
            User user = UserAgent.Instance().GetUserInfo(userId);
            string agent = "null";
            if (QF("Agent", 0) == 1)
            {
                List<string> parent = new List<string>();
                foreach (int parentId in UserAgent.Instance().GetUserParentList(userId))
                {
                    parent.Add("\"" + UserAgent.Instance().GetUserName(parentId) + "\"");
                    if (parentId == UserInfo.ID) break;
                }

                agent = string.Format("[{0}]", string.Join(",", parent));
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                user.ID,
                user.UserName,
                user.Rebate,
                user.Money,
                user.CreateAt,
                user.LoginAt,
                user.FaceShow,
                user.Type,
                IsAgent = user.AgentID == UserInfo.ID,
                ParentRabate = UserInfo.Rebate,
                ParentMoney = UserInfo.Money,
                Agent = new JsonString(agent)
            });
        }

        /// <summary>
        /// 获取团队中的在线用户
        /// </summary>
        /// <param name="context"></param>
        private void onlinelist(HttpContext context)
        {
            this.CheckUserLogin(context);
            var list = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.IsOnline && BDC.UserDepth.Where(p => p.SiteID == SiteInfo.ID && p.UserID == UserInfo.ID).Select(p => p.ChildID).Contains(t.ID));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.Select(t => new { t.UserName, t.Money, t.CreateAt }).OrderBy(t => t.CreateAt), t => t, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 用户的类型，同时判断是不是直属上级
        /// </summary>
        /// <param name="context"></param>
        private void updateusertype(HttpContext context)
        {
            this.CheckUserLogin(context);
            User user = UserAgent.Instance().GetUserInfo(QF("id", 0));
            if (user == null || user.AgentID != UserInfo.ID)
            {
                context.Response.Write(false, "用户ID错误或者不是您的直属下级");
            }

            if (user.Type == User.UserType.Agent)
            {
                context.Response.Write(false, "该用户已经是代理了");
            }

            user.Type = User.UserType.Agent;
            this.ShowResult(context, UserAgent.Instance().UpdateUserInfo(user, t => t.Type), "升级成功");
        }

        /// <summary>
        /// 用户的配额
        /// </summary>
        /// <param name="context"></param>
        private void quota(HttpContext context)
        {
            this.CheckUserLogin(context);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(UserAgent.Instance().GetUserQuotaList(UserInfo.ID).Where(t => t.MinRebate <= UserInfo.Rebate), t => new
            {
                t.MinRebate,
                MaxRebate = Math.Min(UserInfo.Rebate, t.MaxRebate),
                t.Number,
                t.Count,
                Over = Math.Max(0, t.Number - t.Count)
            }));
        }

        /// <summary>
        /// 邀请链接列表
        /// </summary>
        /// <param name="context"></param>
        private void invitelist(HttpContext context)
        {
            this.CheckUserLogin(context);
            List<UserInvite> list = UserAgent.Instance().GetInviteList(UserInfo.ID);
            List<QuotaSetting> quota = UserAgent.Instance().GetUserQuotaList(UserInfo.ID);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Rebate,
                t.Member,
                Type = t.Type.GetDescription(),
                Quota = quota.Exists(p => p.MinRebate <= t.Rebate && p.MaxRebate >= t.Rebate) ? quota.Find(p => p.MinRebate <= t.Rebate && p.MaxRebate >= t.Rebate).Number.ToString() : "无限制"
            }));
        }

        /// <summary>
        /// 添加下级代理的初始条件
        /// </summary>
        /// <param name="context"></param>
        private void invite(HttpContext context)
        {
            if (UserInfo.Type != User.UserType.Agent)
            {
                context.Response.Write(false, "您不是代理");
            }

            List<int> rebate = new List<int>();
            int start = SiteInfo.Setting.IsSameRebate ? UserInfo.Rebate : Math.Max(SiteInfo.Setting.MinRebate, UserInfo.Rebate - 2);
            for (int i = start; i >= SiteInfo.Setting.MinRebate; i -= 2)
            {
                rebate.Add(i);
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                UserRebate = UserInfo.Rebate,
                Type = new JsonString(typeof(User.UserType).ToList().ToDictionary(t => t.Name, t => t.Description).ToJson()),
                Rebate = new JsonString(string.Format("[{0}]", string.Join(",", rebate)))
            });
        }

        /// <summary>
        /// 新建连接
        /// </summary>
        /// <param name="context"></param>
        private void saveinvite(HttpContext context)
        {
            UserInvite invite = context.Request.Form.Fill<UserInvite>();
            this.ShowResult(context, UserAgent.Instance().SaveUserInvite(invite), "保存成功");
        }

        /// <summary>
        /// 删除一个邀请链接
        /// </summary>
        /// <param name="context"></param>
        private void deleteinvite(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().DeleteInvite(QF("id")), "删除成功");
        }

        /// <summary>
        /// 邀请链接列表
        /// </summary>
        /// <param name="context"></param>
        private void inviteurl(HttpContext context)
        {
            string id = QF("id");
            UserInvite invite = UserAgent.Instance().GetUserInviteInfo(id);
            if (invite == null)
            {
                context.Response.Write(false, "邀请码错误");
            }
            IEnumerable<string> list = SystemAgent.Instance().GetInviteDomain().Select(t => string.Format("{0}{1}", t.Domain, id.Trim())).OrderBy(t => Guid.NewGuid()).Take(5);
            List<QuotaSetting> quota = UserAgent.Instance().GetUserQuotaList(UserInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new { Url = t }, new
            {
                invite.ID,
                invite.Rebate,
                invite.Member,
                Type = invite.Type.GetDescription(),
                Quota = quota.Exists(p => p.MinRebate <= invite.Rebate && p.MaxRebate >= invite.Rebate) ? quota.Find(p => p.MinRebate <= invite.Rebate && p.MaxRebate >= invite.Rebate).Number.ToString() : "无限制"
            }));

        }

        /// <summary>
        /// 今日的个人数据统计
        /// </summary>
        /// <param name="context"></param>
        private void today(HttpContext context)
        {
            this.CheckUserLogin(context);

            Dictionary<MoneyLog.MoneyCategoryType, decimal> list = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID && t.CreateAt > DateTime.Now.Date).GroupBy(t => t.Type).Select(t => new
            {
                Type = t.Key,
                Money = t.Sum(p => p.Money)
            }).ToList().Select(t => new
            {
                Type = t.Type.GetCategory(),
                Money = t.Money
            }).GroupBy(t => t.Type).Select(t => new
            {
                t.Key,
                Money = t.Sum(p => p.Money)
            }).ToDictionary(t => t.Key, t => Math.Abs(t.Money));

            IQueryable<int> childList = BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID).Select(t => t.ChildID);
            IQueryable<User> teamUser = BDC.User.Where(t => t.SiteID == SiteInfo.ID && childList.Contains(t.ID));

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Return = list.ContainsKey(MoneyLog.MoneyCategoryType.Return) ? list[MoneyLog.MoneyCategoryType.Return] : 0M,
                Agent = list.ContainsKey(MoneyLog.MoneyCategoryType.Agent) ? list[MoneyLog.MoneyCategoryType.Agent] : 0M,
                Member = childList.Count(),
                Balance = this.Show(teamUser.Sum(t => (decimal?)t.Money)),
                Online = teamUser.Where(t => t.IsOnline).Count()
            });
        }

        /// <summary>
        /// 统计日期范围内的团队绩效
        /// </summary>
        /// <param name="context"></param>
        private void datestatistics(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            // 所有的下级
            IQueryable<int> childList = BDC.UserDepth.Where(t => t.SiteID == SiteInfo.ID && t.UserID == UserInfo.ID).Select(t => t.ChildID);

            Dictionary<MoneyLog.MoneyType, decimal> list = UserAgent.Instance().GetTeamReport(UserInfo.ID, startAt, endAt, true);

            BW.Common.Reports.UserReport report = new Common.Reports.UserReport(UserInfo.ID, list, null);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Recharge = report.data.Get(MoneyLog.MoneyCategoryType.Recharge, decimal.Zero),
                Withdraw = Math.Abs(report.data.Get(MoneyLog.MoneyCategoryType.Withdraw, decimal.Zero)),
                Bet = Math.Abs(report.data.Get(MoneyLog.MoneyCategoryType.Bet, decimal.Zero)),
                Reward = report.data.Get(MoneyLog.MoneyCategoryType.Reward, decimal.Zero),
                Return = report.data.Get(MoneyLog.MoneyCategoryType.Return, decimal.Zero),
                Agent = report.data.Get(MoneyLog.MoneyCategoryType.Agent, decimal.Zero),
                Member = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > startAt && t.CreateAt < endAt && childList.Contains(t.ID)).Count(),
                Money = report.data.Get(MoneyLog.MoneyCategoryType.Activity, decimal.Zero)
            });
        }

        /// <summary>
        /// 上级请求修改下级的返点，获取下级用户的返点信息
        /// </summary>
        /// <param name="context"></param>
        private void rebateinfo(HttpContext context)
        {
            User user = UserAgent.Instance().GetUserInfo(QF("ID", 0));
            if (user == null)
            {
                context.Response.Write(false, "编号错误");
            }
            if (user.AgentID != UserInfo.ID)
            {
                context.Response.Write(false, "该用户不是您的直属下级");
            }

            List<int> rebate = new List<int>();
            if (SiteInfo.Setting.IsSameRebate) rebate.Add(UserInfo.Rebate);

            for (int i = UserInfo.Rebate - 2; i >= user.Rebate; i -= 2)
            {
                rebate.Add(i);
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                UserID = user.ID,
                UserName = user.UserName,
                Rebate = user.Rebate,
                RebateList = new JsonString("[" + string.Join(",", rebate) + "]")
            });
        }

        /// <summary>
        /// 上级给下级升级点位
        /// </summary>
        /// <param name="context"></param>
        private void saverebate(HttpContext context)
        {
            User user = UserAgent.Instance().GetUserInfo(QF("ID", 0));
            if (user == null)
            {
                context.Response.Write(false, "编号错误");
            }
            if (user.AgentID != UserInfo.ID)
            {
                context.Response.Write(false, "该用户不是您的直属下级");
            }
            this.ShowResult(context, UserAgent.Instance().UpdateUserRebate(user.ID, QF("Rebate", 0)));
        }

        /// <summary>
        /// 转账信息
        /// </summary>
        /// <param name="context"></param>
        private void transferinfo(HttpContext context)
        {
            User user = UserAgent.Instance().GetUserInfo(QF("ID", 0));
            if (user == null)
            {
                context.Response.Write(false, "编号错误");
            }
            bool check = false;
            if (user.AgentID != UserInfo.ID && user.ID != UserInfo.AgentID)
            {
                context.Response.Write(false, "该用户不是您的直属上下级");
            }
            else if (user.AgentID == UserInfo.ID)
            {
                if (!UserInfo.Function.HasFlag(User.FunctionType.TransferDown))
                {
                    context.Response.Write(false, "您没有向下级转账的权限");
                }
                check = true;
            }
            else if (user.ID == UserInfo.AgentID)
            {
                if (!UserInfo.Function.HasFlag(User.FunctionType.TransferUp))
                {
                    context.Response.Write(false, "您没有向上级转账的权限");
                }
                check = true;
            }

            if (check)
            {
                context.Response.Write(true, this.StopwatchMessage(context), new
                {
                    user.ID,
                    UserName = user.UserName,
                    Money = UserInfo.Money
                });
            }
            else
            {
                context.Response.Write(false, "发生未知错误");
            }
        }

        /// <summary>
        /// 提交上下级转账
        /// </summary>
        /// <param name="context"></param>
        private void savetransferinfo(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().SaveTransferInfo(UserInfo.ID, QF("ID", 0), QF("Money", decimal.Zero), QF("PayPassword")), "转账成功");
        }

        /// <summary>
        /// 获取整个团队的日期盈亏报表
        /// </summary>
        /// <param name="context"></param>
        private void getteamcalendar(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.AddDays(-7).Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date);
            int userId = QF("UserID", UserInfo.ID);
            if (userId != UserInfo.ID && !UserAgent.Instance().IsUserChild(UserInfo.ID, userId))
            {
                context.Response.Write(false, "团队编号错误");
            }
            List<TeamDateMoney> list = BDC.TeamDateMoney.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.Date >= startAt && t.Date <= endAt).ToList();
            Dictionary<DateTime, UserReport> report = new Dictionary<DateTime, UserReport>();

            DateTime _startAt = startAt;
            while (_startAt <= endAt)
            {
                report.Add(_startAt, new UserReport(userId, list.Where(t => t.Date == _startAt).ToDictionary(t => t.Type, t => t.Money), null));
                _startAt = _startAt.AddDays(1);
            }

            _startAt = startAt.AddDays(-1);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(report, t => new
            {
                Date = t.Key,
                Data = new JsonString(t.Value.data.ToJson()),
                Money = t.Value.Money
            }));
        }

        #region ======== 契约管理 =========

        /// <summary>
        /// 契约管理
        /// </summary>
        /// <param name="context"></param>
        private void contactlist(HttpContext context)
        {
            IQueryable<Contract> list = BDC.Contract.Where(t => t.SiteID == SiteInfo.ID);
            switch (QF("type"))
            {
                case "child":
                    list = list.Where(t => t.User1 == UserInfo.ID);
                    break;
                case "parent":
                    list = list.Where(t => t.User2 == UserInfo.ID);
                    break;
                default:
                    context.Response.Write(false, "没有指定查找类型");
                    break;
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                UserID = t.User2,
                UserName = UserAgent.Instance().GetUserName(t.User2),
                t.CreateAt,
                Status = t.Status.GetDescription()
            }));
        }

        /// <summary>
        /// 新建契约的时候获取初始化信息
        /// </summary>
        /// <param name="context"></param>
        private void getcontractinfo(HttpContext context)
        {
            Contract.ContractType type = QF("Type").ToEnum<Contract.ContractType>();
            Planning plan = SiteAgent.Instance().GetPlanInfo(type.ToEnum<PlanType>());
            if (plan == null || !plan.IsOpen)
            {
                context.Response.Write(false, "当前系统未开放该类型契约");
            }

            Contract contract = UserAgent.Instance().GetContractInfo(type, UserInfo.ID, UserInfo.AgentID);
            if (contract == null)
            {
                context.Response.Write(false, "您的上级未签订该类型契约");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Type = contract.Type,
                ContractType = contract.Type.GetDescription(),
                Setting = new JsonString("[", string.Join(",", contract.Setting.Select(t => t.ToString())), "]")
            });
        }

        /// <summary>
        /// 获取契约内容（根据ID获取契约内容）
        /// </summary>
        /// <param name="context"></param>
        private void getcontractdetail(HttpContext context)
        {
            int id = QF("ID", 0);
            Contract contract = UserAgent.Instance().GetContractInfo(id);
            if (contract == null || !new int[] { contract.User1, contract.User2 }.Contains(UserInfo.ID))
            {
                context.Response.Write(false, "契约编号错误");
            }

            string[] action = contract.GetAction(UserInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                contract.ID,
                Type = contract.Type,
                ContractType = contract.Type.GetDescription(),
                contract.CreateAt,
                Status = contract.Status.GetDescription(),
                User1 = contract.User1 == UserInfo.ID ? UserInfo.UserName : "我的上级",
                User2 = UserAgent.Instance().GetUserName(contract.User2),
                Setting = new JsonString("[", string.Join(",", contract.Setting.Select(t => t.ToString())), "]"),
                Action = new JsonString("[", string.Join(",", action.Select(t => "\"" + t + "\"")), "]")
            });
        }

        /// <summary>
        /// 签订契约
        /// </summary>
        /// <param name="context"></param>
        private void addcontract(HttpContext context)
        {
            Contract.ContractType type = QF("Type").ToEnum<Contract.ContractType>();
            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            if (userId == 0)
            {
                context.Response.Write(false, "下级用户名错误");
            }

            bool percent = QF("Percent", 0) == 1;
            Dictionary<string, decimal> data = new Dictionary<string, decimal>();
            foreach (string key in context.Request.Form.AllKeys.Where(t => t.StartsWith("Setting.")))
            {
                string name = key.Substring(key.IndexOf('.') + 1);
                data.Add(name, QF(key, decimal.MinusOne) / (percent ? 100 : 1));
            }

            this.ShowResult(context, UserAgent.Instance().AddContract(UserInfo.ID, userId, type, QF("PayPassword"), data), "契约签订成功");
        }

        /// <summary>
        /// 对契约进行更新操作
        /// </summary>
        /// <param name="context"></param>
        private void updatecontract(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateContractInfo(QF("ID", 0), UserInfo.ID, QF("PayPassword"), QF("Action")), "保存成功");
        }

        /// <summary>
        /// 获取未发放的契约列表
        /// </summary>
        /// <param name="context"></param>
        private void getcontractpayment(HttpContext context)
        {
            IQueryable<ContractLog> list = BDC.ContractLog.Where(t => t.SiteID == SiteInfo.ID && (t.UserID == UserInfo.ID || t.User2 == UserInfo.ID) && t.Status == ContractLog.TransferStatus.None);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.CreateAt,
                t.Money,
                t.Description,
                User = t.UserID == UserInfo.ID ? UserAgent.Instance().GetUserName(t.User2) : "上级",
                // 自己是否是上级
                IsParent = t.UserID == UserInfo.ID ? 1 : 0
            }));
        }

        /// <summary>
        /// 提交契约的处理
        /// </summary>
        /// <param name="context"></param>
        private void submitcontractlog(HttpContext context)
        {
            int id = QF("ID", 0);
            if (id == 0)
            {
                context.Response.Write(false, "编号错误");
            }

            ContractLog log = UserAgent.Instance().GetContractLogInfo(id);
            if (log == null || log.Status != ContractLog.TransferStatus.None)
            {
                context.Response.Write(false, "状态错误");
            }

            if (log.UserID == UserInfo.ID)
            {
                this.ShowResult(context, UserAgent.Instance().ExecContractLog(id), "发放成功");
            }
            else if (log.User2 == UserInfo.ID)
            {
                UserAgent.Instance().SaveChatLog(new ChatLog()
                {
                    Content = string.Format("请发放【{0}】，金额：{1}元，备注信息：{2}", log.Type.GetDescription(), log.Money.ToString("c"), log.Description),
                    CreateAt = DateTime.Now,
                    SendAvatar = UserInfo.FaceShow,
                    SendID = "USER-" + UserInfo.ID,
                    SendName = UserInfo.Name,
                    UserID = "USER-" + log.UserID,
                    SiteID = SiteInfo.ID,
                    Key = UserAgent.Instance().GetTalkKey("USER-" + UserInfo.ID, "USER-" + log.UserID)
                });
                context.Response.Write(true, "已经成功通知上级");
            }
            else
            {
                context.Response.Write(false, "编号错误");
            }
        }

        #endregion
    }
}
