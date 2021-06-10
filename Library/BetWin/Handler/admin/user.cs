using BW.Agent;
using BW.Common.Admins;
using BW.Common.Games;
using BW.Common.Reports;
using BW.Common.Sites;
using BW.Common.Users;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Model;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace BW.Handler.admin
{
    /// <summary>
    /// 管理员对会员/代理的管理
    /// </summary>
    public class user : IHandler
    {
        /// <summary>
        /// 添加总代
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.新增总代.Value)]
        private void agentadd(HttpContext context)
        {
            string username = QF("UserName");
            string password = QF("Password");
            int rebate = QF("Rebate", 0);

            this.ShowResult(context, UserAgent.Instance().AddUser(new User()
            {
                UserName = username,
                Password = password,
                Rebate = rebate,
                Type = User.UserType.Agent
            }), "总代创建成功");
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.用户列表.Value)]
        private void list(HttpContext context)
        {
            var query = BDC.User.Where(t => t.SiteID == SiteInfo.ID);
            if (QF("UserID", 0) != 0) query = query.Where(t => t.ID == QF("UserID", 0));
            if (!string.IsNullOrEmpty(QF("User"))) query = query.Where(t => t.UserName.Contains(QF("User")) || t.NickName.Contains(QF("User")));
            if (!string.IsNullOrEmpty(QF("AccountName"))) query = query.Where(t => t.AccountName.Contains(QF("AccountName")) || t.Mobile == QF("AccountName") || t.Email == QF("AccountName"));
            if (!string.IsNullOrEmpty(QF("Agent"))) query = query.Where(t => t.AgentID == UserAgent.Instance().GetUserID(QF("Agent")));
            if (!string.IsNullOrEmpty(QF("RegIP"))) query = query.Where(t => t.RegIP == QF("RegIP"));
            if (QF("MinMoney", -1.00M) >= 0) query = query.Where(t => t.Money >= QF("MinMoney", 0.00M));
            if (QF("MaxMoney", -1.00M) >= 0) query = query.Where(t => t.Money <= QF("MaxMoney", 0.00M));
            if (!string.IsNullOrEmpty(QF("Status"))) query = query.Where(t => t.Status == QF("Status").ToEnum<User.UserStatus>());
            if (QF("Online", 0) == 1) query = query.Where(t => t.IsOnline);
            if (!string.IsNullOrEmpty(QF("Test"))) query = query.Where(t => t.IsTest == (QF("Test", 0) == 1));
            if (!string.IsNullOrEmpty(QF("GroupID"))) query = query.Where(t => t.GroupID == QF("GroupID", 0));
            int recordCount = query.Count();

            IOrderedQueryable<User> userlist;
            switch (QF("sort").ToLower())
            {
                case "money desc":
                    userlist = query.OrderByDescending(t => t.Money);
                    break;
                case "money asc":
                    userlist = query.OrderBy(t => t.Money);
                    break;
                default:
                    userlist = query.OrderByDescending(t => t.ID);
                    break;
            }

            context.Response.Write(true, "用户列表", this.ShowResult(userlist, t => new
            {
                t.ID,
                t.UserName,
                t.Type,
                TypeName = t.Type.GetDescription(),
                t.AccountName,
                t.Rebate,
                t.CreateAt,
                t.Status,
                t.Money,
                t.LockMoney,
                Group = SiteInfo.UserGroup[t.GroupID].Name,
                ChildCount = UserAgent.Instance().GetChildCount(t.ID),
                //Agent = t.AgentID == 0 ? "N/A" : string.Join("&gt;&gt;",
                //    UserAgent.Instance().GetUserParentList(t.ID).Take(3).Select(p => string.Format("<a href=\"javascript:\" class=\"diag-user\" data-userid=\"{0}\">{1}</a>", p, UserAgent.Instance().GetUserName(p)))),
                StatusName = t.Status.GetDescription(),
                Online = t.IsOnline,
                t.IsTest
            }, new
            {
                Money = this.Show(userlist.Sum(t => (decimal?)t.Money)),
                LockMoney = this.Show(userlist.Sum(t => (decimal?)t.LockMoney))
            }));
        }

        /// <summary>
        /// 得到在线人数
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.用户列表.Value)]
        private void online(HttpContext context)
        {
            context.Response.Write(true, "在线会员", new
            {
                Count = UserAgent.Instance().GetUserOnlineCount()
            });
        }

        /// <summary>
        /// 踢出用户
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.踢下线)]
        private void offline(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().SetOffline(QF("ID", 0)), "踢出成功");
        }

        /// <summary>
        /// 查看用户基本信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.用户列表.Value)]
        private void get(HttpContext context)
        {
            int userId = QF("UserID", 0);
            if (userId == 0) context.Response.Write(false, "未指定要查询的用户");
            User user = UserAgent.Instance().GetUserInfo(userId);
            if (user == null) context.Response.Write(false, "用户编号错误");

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                user.ID,
                user.UserName,
                UserType = user.Type.GetDescription(),
                StatusName = user.Status.GetDescription(),
                Lock = new JsonString(
                    typeof(User.LockStatus).ToList().Select(t => new
                    {
                        t.ID,
                        t.Name,
                        t.Description,
                        IsChecked = ((int)user.Lock & t.ID) == t.ID ? 1 : 0
                    }).ToJson()
                    ),
                Function = new JsonString(
                    typeof(User.FunctionType).ToList().Select(t => new
                    {
                        t.ID,
                        t.Name,
                        t.Description,
                        IsChecked = ((int)user.Function & t.ID) == t.ID ? 1 : 0
                    }).ToJson()
                    ),
                user.TotalMoney,
                user.LockMoney,
                user.Withdraw,
                user.Wallet,
                user.Money,
                user.NickName,
                user.AccountName,
                user.Mobile,
                user.Email,
                user.QQ,
                Agent = user.AgentID == 0 ? "N/A" : string.Join("&gt;&gt;",
                    UserAgent.Instance().GetUserParentList(user.ID).Select(p => string.Format("{0}", UserAgent.Instance().GetUserName(p)))),
                user.CreateAt,
                user.RegIP,
                RegIPAddress = UserAgent.Instance().GetIPAddress(user.RegIP),
                user.LoginAt,
                user.LoginIP,
                LoginIPAddress = UserAgent.Instance().GetIPAddress(user.LoginIP),
                Password = user.Password == SP.Studio.Security.MD5.toMD5(SiteInfo.Setting.DefaultPassword ?? string.Empty) ? "默认密码" : "已设置",
                PayPassword = !string.IsNullOrEmpty(user.PayPassword) ? "已设置" : "未设置",
                Bank = UserAgent.Instance().GetBankAccountList(user.ID).Count > 0 ? "已设置" : "未设置",
                SecretKey = user.SecretKey == Guid.Empty ? "未设置" : "已设置",
                //BetAmount = this.Show(BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == user.ID && t.Type == MoneyLog.MoneyType.Bet).Sum(t => (decimal?)t.Money)),
                //RechargeAmount = this.Show(BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == user.ID && t.Type == MoneyLog.MoneyType.Recharge).Sum(t => (decimal?)t.Money)),
                //WithdeawAmount = this.Show(BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == user.ID && t.Type == MoneyLog.MoneyType.Withdraw).Sum(t => (decimal?)t.Money)),
                remark = UserAgent.Instance().GetRemarkInfo(user.ID),
                user.Rebate,
                user.IsTest,
                user.GroupID
            });
        }

        /// <summary>
        /// 根据用户名获取用户的基本信息
        /// </summary>
        /// <param name="context"></param>
        private void getinfo(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            if (userId == 0)
            {
                context.Response.Write(false, string.Format("{0}不存在", QF("UserName")));
            }
            User user = UserAgent.Instance().GetUserInfo(userId);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                user.ID,
                user.UserName,
                user.Money,
                Status = user.Status.GetDescription()
            });
        }

        /// <summary>
        /// 重置用户资料
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.重置登录密码, AdminPermission.会员管理.重置资金密码, AdminPermission.会员管理.重置提现账户)]
        private void userinforeset(HttpContext context)
        {
            User user = UserAgent.Instance().GetUserInfo(QF("UserID", 0));
            if (user == null)
            {
                context.Response.Write(false, "编号错误");
            }
            bool success = false;
            switch (QF("Name"))
            {
                case "AccountName":
                    this.CheckAdminLogin(context, AdminPermission.会员管理.重置提现账户);
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.AccountName, QF("value"));
                    break;
                case "PayPassword":
                    this.CheckAdminLogin(context, AdminPermission.会员管理.重置资金密码);
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.PayPassword);
                    break;
                case "Password":
                    this.CheckAdminLogin(context, AdminPermission.会员管理.重置登录密码);
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.Password);
                    break;
                case "Mobile":
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.Mobile, QF("value"));
                    break;
                case "Email":
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.Email);
                    break;
                case "QQ":
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.QQ);
                    break;
                case "SecretKey":
                    success = UserAgent.Instance().ResetUserInfo(user, t => t.SecretKey);
                    break;
            }
            context.Response.Write(success, success ? "重置成功" : UserAgent.Instance().Message());
        }

        /// <summary>
        /// 获取用户的登录IP信息
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getip(HttpContext context)
        {
            string ip = BDC.User.Where(t => t.SiteID == SiteInfo.ID && t.ID == QF("id", 0)).Select(t => t.LoginIP).FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                context.Response.Write(false, "ID错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                IP = ip,
                IPAddress = UserAgent.Instance().GetIPAddress(ip)
            });
        }

        /// <summary>
        /// 获取备注信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.会员备注)]
        private void remark(HttpContext context)
        {

            int userId = QF("UserID", 0);
            context.Response.Write(true, "", this.ShowResult(
                UserAgent.Instance().GetUserRemarkList(userId), t => new
                {
                    Admin = AdminAgent.Instance().GetAdminName(t.AdminID),
                    t.CreateAt,
                    t.Content
                }));

        }

        /// <summary>
        /// 添加一条备注信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.会员备注)]
        private void remarkadd(HttpContext context)
        {
            int userId = QF("UserID", 0);
            string content = QF("Content");
            this.ShowResult(context, UserAgent.Instance().SaveRemarkInfo(userId, content), "保存成功");
        }

        /// <summary>
        /// 资金类型
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void moneylogtype(HttpContext context)
        {
            string data = string.Join(",", typeof(MoneyLog.MoneyCategoryType).ToList().Select(t =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("\"{0}\":{{", t.Description);
                sb.Append(string.Join(",", typeof(MoneyLog.MoneyType).ToList().Where(p =>
                {
                    MoneyLog.MoneyCategoryAttribute attribute = ((MoneyLog.MoneyType)p.ID).GetAttribute<MoneyLog.MoneyCategoryAttribute>();
                    if (attribute == null) return false;
                    return (int)attribute.Type == t.ID;
                }).Select(p => string.Format("\"{0}\":\"{1}\"", p.Name, p.Description))));

                sb.Append("}");
                return sb.ToString();
            }));

            context.Response.Write(true, this.StopwatchMessage(context), "{" + data + "}");
        }

        /// <summary>
        /// 会员的帐变记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.会员帐变)]
        private void moneylog(HttpContext context)
        {
            int userId = QF("UserID", 0);
            var list = BDC.MoneyLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId && t.TableID == userId.GetTableID());
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));
            if (!string.IsNullOrEmpty(QF("Key"))) list = list.Where(t => t.Description.Contains(QF("Key")));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<MoneyLog.MoneyType>());

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.Money,
                t.Balance,
                t.CreateAt,
                t.Description
            }));
        }


        /// <summary>
        /// 查看用户的被锁定资金明细
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.会员帐变)]
        private void moneylock(HttpContext context)
        {
            var list = BDC.MoneyLock.Where(t => t.SiteID == SiteInfo.ID && t.UserID == QF("UserID", 0) && t.UnLockAt.Year < 2000);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.LockAt), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.Money,
                t.LockAt,
                t.Description,
                t.SourceID
            }, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }


        /// <summary>
        /// 对会员锁定金额进行解锁操作
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.解锁资金)]
        private void moneyunlock(HttpContext context)
        {
            int id = QF("ID", 0);
            this.ShowResult(context, UserAgent.Instance().UnlockMoney(id), "解锁成功");
        }

        /// <summary>
        /// 操作记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.操作记录)]
        private void log(HttpContext context)
        {
            var list = BDC.UserLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == QF("UserID", 0));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));
            if (!string.IsNullOrEmpty(QF("Key"))) list = list.Where(t => t.Content.Contains(QF("Key")));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                t.CreateAt,
                t.Content,
                t.IP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.IP)
            }));
        }

        /// <summary>
        /// 用户的银行卡列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.银行卡信息)]
        private void bankaccount(HttpContext context)
        {
            var list = BDC.BankAccount.Where(t => t.SiteID == SiteInfo.ID && t.UserID == QF("UserID", 0));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                Bank = t.Type.GetDescription(),
                BankName = t.Bank,
                AccountName = UserAgent.Instance().GetUserAccountName(t.UserID),
                t.Account,
                t.CreateAt
            }));
        }

        /// <summary>
        /// 删除用户绑定的银行卡
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.银行卡信息)]
        private void deletebank(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().DeleteBankAccount(QF("id", 0)), "删除成功");
        }

        /// <summary>
        /// 添加配额配置
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.配额管理.Value)]
        private void addquota(HttpContext context)
        {
            QuotaSetting setting = context.Request.Form.Fill<QuotaSetting>();

            this.ShowResult(context, SiteAgent.Instance().SaveQuotaSetting(setting), "配额添加成功");
        }

        /// <summary>
        /// 配额列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.配额管理.Value)]
        private void quotalist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(SiteAgent.Instance().GetQuotaSettingList(), t => new
            {
                t.MinRebate,
                t.MaxRebate,
                t.Number,
                t.ID
            }));
        }

        /// <summary>
        /// 删除配额设置
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.配额管理.Value)]
        private void deletequota(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteQuota(QF("ID", 0)), "删除成功");
        }

        /// <summary>
        /// 站内信列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.站内信)]
        private void messagelist(HttpContext context)
        {

            IQueryable<UserMessage> list = BDC.UserMessage.Where(t => t.SiteID == SiteInfo.ID && t.UserID == QF("UserID", 0));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.Title,
                t.CreateAt,
                t.ReadAt
            }));
        }

        /// <summary>
        /// 发送站内信
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.站内信)]
        private void messagesend(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().SendMessage(QF("UserID", 0), QF("Title"), QF("Content")), "发送成功");
        }

        /// <summary>
        /// 更新用户的可提现额度
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.用户列表.Value)]
        private void updatewithdraw(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateUserWithdraw(QF("UserID", 0), QF("Value", decimal.MinusOne)), "设置成功");
        }

        /// <summary>
        /// 会员的锁定状态值更新
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.Value)]
        private void updatelockstatus(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateUserLockStatus(QF("ID", 0), QF("name").ToEnum<User.LockStatus>(), QF("Value", 0) == 1), "锁定状态修改成功");
        }

        /// <summary>
        /// 会员的功能开放
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.Value)]
        private void updatefunctionstatus(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateUserFunctionStatus(QF("ID", 0), QF("name").ToEnum<User.FunctionType>(), QF("Value", 0) == 1), "功能设置保存成功");
        }

        /// <summary>
        /// 会员日志查看
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.风控管理.用户日志.Value)]
        private void loglist(HttpContext context)
        {
            IQueryable<UserLog> list = BDC.UserLog.Where(t => t.SiteID == SiteInfo.ID);
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).Date.AddDays(1));
            if (!string.IsNullOrEmpty(QF("IP"))) list = list.Where(t => t.IP == QF("IP"));
            if (!string.IsNullOrEmpty(QF("Key"))) list = list.Where(t => t.Content.Contains(QF("Key")));
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.IP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.IP),
                t.CreateAt,
                t.Content
            }));
        }

        /// <summary>
        /// IP风控管理
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.风控管理.IP管理.Value)]
        private void iprisk(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("User"));
            IQueryable<UserLog> list = BDC.UserLog.Where(t => t.SiteID == SiteInfo.ID && t.UserID == userId);
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).Date.AddDays(1));

            IQueryable<int> users = null;

            switch (QF("Type"))
            {
                case "IP":
                    users = BDC.UserLog.Where(t => t.SiteID == SiteInfo.ID && t.AdminID == 0 && list.GroupBy(p => p.IP).Select(p => p.Key).Contains(t.IP)).GroupBy(t => t.UserID).Select(t => t.Key);
                    break;
                case "Broswer":
                    users = BDC.UserLog.Where(t => t.SiteID == SiteInfo.ID && t.AdminID == 0 && list.GroupBy(p => p.BowserID).Select(p => p.Key).Contains(t.BowserID)).GroupBy(t => t.UserID).Select(t => t.Key);
                    break;
            }


            IQueryable<User> userList = BDC.User.Where(t => t.SiteID == SiteInfo.ID).Join(users, t => t.ID, t => t, (user, log) => user);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(userList.OrderBy(t => t.ID), t => new
            {
                t.ID,
                t.UserName,
                t.Money,
                t.CreateAt,
                t.LoginAt,
                t.LoginIP,
                IPAddress = UserAgent.Instance().GetIPAddress(t.LoginIP),
                Agent = t.AgentID == 0 ? "N/A" : string.Join("&gt;&gt;",
                    UserAgent.Instance().GetUserParentList(t.ID).Select(p => string.Format("<a href=\"javascript:\" class=\"diag-user\" data-userid=\"{0}\">{1}</a>", p, UserAgent.Instance().GetUserName(p)))),
            }, new
            {
                Money = this.Show(userList.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 清楚用户缓存
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void removecache(HttpContext context)
        {
            UserAgent.Instance().RemoveUserCache(QF("UserID", 0));
            context.Response.Write(true, "缓存刷新成功");
        }

        /// <summary>
        /// 标记用户是否是测试帐号
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.修改)]
        private void updateusertest(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateUserTest(QF("UserID", 0), QF("Value", 0) == 1));
        }

        /// <summary>
        /// 修改用户的返点
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.修改)]
        private void userinforebate(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateUserRebate(QF("UserID", 0), QF("Value", 0)));
        }

        [Admin(AdminPermission.会员管理.账号管理.修改)]
        private void updateagent(HttpContext context)
        {
            int userId = QF("UserID", 0);
            int agentId = -1;
            string agent = QF("Agent");
            if (agent == "总代")
            {
                agentId = 0;
            }
            else
            {
                agentId = UserAgent.Instance().GetUserID(agent);
                if (agentId == 0) agentId = -1;
            }
            if (agentId == -1)
            {
                context.Response.Write(false, "代理账号输入错误");
            }

            this.ShowResult(context, UserAgent.Instance().UpdateUserAgent(userId, agentId), "修改成功");
        }

        /// <summary>
        /// 修改单独用户的配额数量
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.修改代理配额)]
        private void getquotalist(HttpContext context)
        {
            int userId = QF("UserID", 0);
            List<QuotaSetting> list = UserAgent.Instance().GetUserQuotaList(userId);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                UserID = userId,
                t.MinRebate,
                t.MaxRebate,
                t.Number,
                t.Count,
                t.Overage
            }));
        }

        /// <summary>
        /// 修改单个代理的配额数量
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.修改代理配额)]
        private void updatequota(HttpContext context)
        {
            switch (QF("name"))
            {
                case "Number":
                    this.ShowResult(context, UserAgent.Instance().UpdateUserQuotaNumber(QF("UserID", 0), QF("id", 0), QF("value", -1)), "配额数量修改成功");
                    break;
                default:
                    context.Response.Write(false, "没有指定要执行的动作");
                    break;
            }
        }

        #region =========== 分组管理 ==============

        [Admin(AdminPermission.会员管理.账号管理.分组管理.Value)]
        private void grouplist(HttpContext context)
        {
            Dictionary<int, string> condition = BDC.GroupCondition.ToDictionary(t => t.ID, t => t.Name);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(UserAgent.Instance().GetGroupList(), t => new
            {
                t.ID,
                t.Name,
                t.Description,
                t.IsDefault,
                t.Sort,
                Condition = condition.Get(t.ConditionID, "N/A")
            }));
        }

        /// <summary>
        /// 修改用户所在的分组
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.账号管理.修改)]
        private void updategroup(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().UpdateUserGroup(QF("ID", 0), QF("GroupID", 0)));
        }

        /// <summary>
        /// 获取分组信息
        /// </summary>
        /// <param name="context"></param>
        private void groupinfo(HttpContext context)
        {
            int id = QF("ID", 0);
            UserGroup group = UserAgent.Instance().GetGroupInfo(id);
            if (group == null)
            {
                context.Response.Write(false, "分组编号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                group.ID,
                group.Name,
                group.Description,
                group.IsDefault,
                Setting = new JsonString(group.Setting.ToJson()),
                group.Sort,
                group.ConditionID
            });
        }

        /// <summary>
        /// 保存分组信息
        /// </summary>
        /// <param name="context"></param>
        private void savegroupinfo(HttpContext context)
        {
            int id = QF("ID", 0);
            UserGroup group = UserAgent.Instance().GetGroupInfo(id);
            group = context.Request.Form.Fill(group);
            group.Setting = context.Request.Form.Fill(group.Setting, "Setting");

            this.ShowResult(context, UserAgent.Instance().SaveUserGroupInfo(group));
        }

        /// <summary>
        /// 分组条件列表
        /// </summary>
        /// <param name="context"></param>
        private void conditionlist(HttpContext context)
        {
            IQueryable<GroupCondition> list = BDC.GroupCondition;
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Name
            }));
        }

        #endregion

        #region =========== 契约管理 ===========

        /// <summary>
        /// 契约管理
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.契约管理.Value)]
        private void contractlist(HttpContext context)
        {
            IQueryable<Contract> list = BDC.Contract.Where(t => t.SiteID == SiteInfo.ID);
            if (QF("ID", 0) != 0) list = list.Where(t => t.ID == QF("ID", 0));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<Contract.ContractType>());
            if (!string.IsNullOrEmpty(QF("User1"))) list = list.Where(t => t.User1 == UserAgent.Instance().GetUserID(QF("User1")));
            if (!string.IsNullOrEmpty(QF("User2"))) list = list.Where(t => t.User2 == UserAgent.Instance().GetUserID(QF("User2")));
            if (!string.IsNullOrEmpty(QF("Status"))) list = list.Where(t => t.Status == QF("Status").ToEnum<Contract.ContractStatus>());
            if (!string.IsNullOrEmpty(QF("StartAt"))) list = list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            if (!string.IsNullOrEmpty(QF("EndAt"))) list = list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                t.User1,
                UserName1 = UserAgent.Instance().GetUserName(t.User1),
                t.User2,
                UserName2 = UserAgent.Instance().GetUserName(t.User2),
                Status = t.Status.GetDescription(),
                t.CreateAt
            }));
        }

        /// <summary>
        /// 重新运行契约工资
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.契约管理.Value)]
        private void contractrun(HttpContext context)
        {
            Contract.ContractType type = QF("Type").ToEnum<Contract.ContractType>();
            Planning plan;
            switch (type)
            {
                case Contract.ContractType.WagesAgent:
                    plan = BDC.Planning.Where(t => t.IsOpen && t.SiteID == SiteInfo.ID && t.Type == GateWay.Planning.PlanType.WagesAgent).FirstOrDefault();
                    if (plan == null)
                    {
                        context.Response.Write(false, "没有开启工资");
                    }
                    this.ShowResult(context, SiteAgent.Instance()._lotteryWages(SiteInfo.ID, plan.PlanSetting, true), "日工资活动执行成功");
                    break;
                case Contract.ContractType.LossWages:
                    plan = BDC.Planning.Where(t => t.IsOpen && t.SiteID == SiteInfo.ID && t.Type == GateWay.Planning.PlanType.LossWages).FirstOrDefault();
                    if (plan == null)
                    {
                        context.Response.Write(false, "没有开启挂单工资");
                    }
                    this.ShowResult(context, SiteAgent.Instance()._lotteryLossWages(SiteInfo.ID, plan.PlanSetting, true), "挂单工资活动执行成功");
                    break;
            }

            context.Response.Write(false, "没有指定契约类型");
        }

        /// <summary>
        /// 契约的详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.契约管理.Value)]
        private void getcontractinfo(HttpContext context)
        {
            Contract contract = UserAgent.Instance().GetContractInfo(QF("ID", 0));
            if (contract == null)
            {
                context.Response.Write(false, "订单号错误");
            }

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                contract.ID,
                Type = contract.Type.GetDescription(),
                contract.CreateAt,
                Status = contract.Status.GetDescription(),
                User1 = UserAgent.Instance().GetUserName(contract.User1),
                User2 = UserAgent.Instance().GetUserName(contract.User2),
                Setting = new JsonString("[", string.Join(",", contract.Setting.Select(t => t.ToString())), "]")
            });
        }

        /// <summary>
        /// 删除契约
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.契约管理.Value)]
        private void deletecontractinfo(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().DeleteContract(QF("ID", 0)), "契约删除成功");
        }

        /// <summary>
        /// 契约转账列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.契约转账.Value)]
        private void getcontractloglist(HttpContext context)
        {
            IQueryable<ContractLog> list = BDC.ContractLog.Where(t => t.SiteID == SiteInfo.ID);
            if (QF("ID", 0) != 0) list = list.Where(t => t.ID == QF("ID", 0));
            if (!string.IsNullOrEmpty(QF("User1"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User1")));
            if (!string.IsNullOrEmpty(QF("User2"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User2")));
            if (!string.IsNullOrEmpty(QF("StartAt"))) list = list.Where(t => t.CreateAt > QF("StartAt", DateTime.MinValue));
            if (!string.IsNullOrEmpty(QF("EndAt"))) list = list.Where(t => t.CreateAt < QF("EndAt", DateTime.Now).AddDays(1));
            if (!string.IsNullOrEmpty(QF("Type"))) list = list.Where(t => t.Type == QF("Type").ToEnum<Contract.ContractType>());
            if (!string.IsNullOrEmpty(QF("Status"))) list = list.Where(t => t.Status == QF("Status").ToEnum<ContractLog.TransferStatus>());
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                Type = t.Type.GetDescription(),
                User1 = t.UserID,
                UserName1 = UserAgent.Instance().GetUserName(t.UserID),
                t.User2,
                UserName2 = UserAgent.Instance().GetUserName(t.User2),
                t.CreateAt,
                t.Money,
                Status = t.Status.GetDescription(),
                t.Description
            }));
        }

        /// <summary>
        /// 支付契约
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.契约转账.Value)]
        private void paymentcontractlog(HttpContext context)
        {
            int id = QF("ID", 0);

            this.ShowResult(context, UserAgent.Instance().ExecContractLog(id));
        }
        #endregion

        #region ============== 第三方游戏账户管理  ==============

        /// <summary>
        /// 第三方游戏账户列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.第三方游戏)]
        private void gameaccountlist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(UserAgent.Instance().GetGameAccount(QF("UserID", 0)), t => new
            {
                t.UserID,
                t.PlayerName,
                t.Password,
                t.Type,
                Game = t.Type.GetDescription(),
                t.Money,
                t.UpdateAt,
                t.Withdraw,
                t.WithdrawAt
            }));
        }

        /// <summary>
        /// 修改游戏账户的可转出额度
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.第三方游戏)]
        private void updategameaccount(HttpContext context)
        {
            GameAccount account = UserAgent.Instance().GetGameAccountInfo(QF("UserID", 0), QF("Type").ToEnum<GameType>());
            if (account == null)
            {
                context.Response.Write(false, "账户不存在");
            }

            switch (QF("name"))
            {
                case "Withdraw":
                    account.Withdraw = QF("value", decimal.MinusOne);
                    if (account.Withdraw < decimal.Zero)
                    {
                        context.Response.Write(false, "输入错误");
                    }
                    if (account.Withdraw > account.Money)
                    {
                        context.Response.Write(false, "可转出额度不能大于当前余额");
                    }
                    account.WithdrawAt = DateTime.Now;
                    account.Update(null, t => t.Withdraw, t => t.WithdrawAt);
                    GameAgent.Instance().AddGameWithdraw(account.UserID, account.Type, account.Withdraw, account.WithdrawAt, string.Format("管理员{0}修改额度", AdminInfo.AdminName));
                    AdminInfo.Log(AdminLog.LogType.User, "更改用户{0}在{1}账户的可转出额度为{2}", UserAgent.Instance().GetUserName(account.UserID), account.Type.GetDescription(), account.Withdraw.ToString("c"));

                    break;
            }

            context.Response.Write(true, "修改成功");
        }

        #endregion

        #region =========== 分红管理 =============

        /// <summary>
        /// 查询分红记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.分红管理.Value)]
        private void bounslist(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            Planning plan = SiteAgent.Instance().GetPlanInfo(GateWay.Planning.PlanType.Bonus);
            if (plan == null || !plan.IsOpen)
            {
                context.Response.Write(false, "系统未开放分红");
            }

            List<PlanBouns> list = SiteAgent.Instance().GetPlanBouns(startAt, endAt);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                UserID = t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Member,
                t.Money,
                t.Sales,
                Bouns = t.GetBouns(plan.PlanSetting.Value)
            }, new
            {
                StartAt = startAt.ToShortDateString(),
                EndAt = endAt.AddDays(-1).ToShortDateString(),
                Money = list.Count == 0 ? decimal.Zero : list.Sum(p => p.Money)
            }));
        }

        /// <summary>
        /// 分红的执行
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.会员管理.代理管理.分红管理.Value)]
        private void bounsrun(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);

            this.ShowResult(context, SiteAgent.Instance().BounsRun(SiteInfo.ID, startAt, endAt), "总代分红发放成功");
        }

        /// <summary>
        /// 单级分红的列表
        /// </summary>
        /// <param name="context"></param>
        private void singlebounslist(HttpContext context)
        {
            DateTime startAt = QF("StartAt", DateTime.Now.Date);
            DateTime endAt = QF("EndAt", DateTime.Now.Date).AddDays(1);
            List<PlanSingleBouns> list = SiteAgent.Instance().GetPlanSingleBouns(startAt, endAt);
            if (list == null)
            {
                context.Response.Write(false, "系统未开放单级分红");
            }

            Planning plan = SiteAgent.Instance().GetPlanInfo(GateWay.Planning.PlanType.SingleBonus);
            if (plan == null || !plan.IsOpen)
            {
                context.Response.Write(false, "系统未开放单级分红");
            }


            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Money,
                t.Sales,
                t.MemberCount,
                Bouns = t.GetBouns(plan.PlanSetting.Value)
            }));

        }
        #endregion

    }
}
