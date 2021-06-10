using BW.Agent;
using BW.Common.Admins;
using BW.Common.Sites;
using BW.Common.Users;
using SP.Studio.Array;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace BW.Handler.admin
{
    /// <summary>
    /// 资金相关
    /// </summary>
    public class money : IHandler
    {
        /// <summary>
        /// 支付接口的参数设定资料
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.充值接口.Value)]
        private void paymentsetting(HttpContext context)
        {

        }

        /// <summary>
        /// 充值记录
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.充值记录.Value)]
        private void rechargelist(HttpContext context)
        {
            var list = BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID);
            list = list.Where(t => t.IsPayment == (QF("Status", 1) == 1));
            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (!string.IsNullOrEmpty(QF("Agent")))
            {
                int agentId = UserAgent.Instance().GetUserID(QF("Agent"));
                list = list.Where(t => t.UserID == agentId || BDC.UserDepth.Where(p => p.SiteID == SiteInfo.ID && p.UserID == agentId).Select(p => p.ChildID).Contains(t.UserID));
            }
            if (QF("ID", (long)0) != 0) list = list.Where(t => t.ID == QF("ID", (long)0));
            if (!string.IsNullOrEmpty(QF("SystemID"))) list = list.Where(t => t.SystemID == QF("SystemID"));
            if (QF("PayID", 0) != 0) list = list.Where(t => t.PayID == QF("PayID", 0));

            if (WebAgent.IsType<DateTime>(QF("StartAt")))
            {
                list = QF("IsPayment", 0) == 1 ? list.Where(t => t.PayAt > DateTime.Parse(QF("StartAt"))) : list.Where(t => t.CreateAt > DateTime.Parse(QF("StartAt")));
            }
            if (WebAgent.IsType<DateTime>(QF("EndAt")))
            {
                list = QF("IsPayment", 0) == 1 ? list.Where(t => t.PayAt < DateTime.Parse(QF("EndAt")).AddDays(1)) : list.Where(t => t.CreateAt < DateTime.Parse(QF("EndAt")).AddDays(1));
            }
            Dictionary<int, string> payment = SiteAgent.Instance().GetPaymentSettingList(true).ToDictionary(t => t.ID, t => t.Name);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.PayAt).ThenByDescending(t => t.CreateAt), t => new
            {
                t.ID,
                t.SystemID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Money,
                t.Amount,
                t.Fee,
                t.PayID,
                Payment = payment.ContainsKey(t.PayID) ? payment[t.PayID] : "N/A",
                t.IsPayment,
                t.CreateAt,
                t.PayAt,
                t.Description,
                t.Reward
            },
            new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money)),
                Amount = this.Show(list.Sum(t => (decimal?)t.Amount)),
                Fee = this.Show(list.Sum(t => (decimal?)t.Fee)),
                Reward = this.Show(list.Sum(t => (decimal?)t.Reward))
            }));
        }

        /// <summary>
        /// 充值详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.充值记录.Value)]
        private void rechargeinfo(HttpContext context)
        {
            long id = QF("id", (long)0);
            RechargeOrder order = UserAgent.Instance().GetRechargeOrderInfo(id);
            if (order == null)
            {
                context.Response.Write(false, "编号错误");
            }

            List<MoneyLog> log = UserAgent.Instance().GetMoneyLogList(order.UserID, MoneyLog.MoneyType.Recharge, (int)(id % int.MaxValue));

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
                order.Amount,
                Log = new JsonString(log.ToJson(t => t.ID, t => t.Money, t => t.Balance, t => t.CreateAt, t => t.Description))
            });
        }

        /// <summary>
        /// 管理员手工确认到账
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.充值记录.确认到账)]
        private void rechargeconfirm(HttpContext context)
        {
            long id = QF("id", (long)0);
            this.ShowResult(context, UserAgent.Instance().ConfirmRechargeOrderInfo(id, QF("Amount", decimal.Zero), QF("SystemID")), "入账成功");
        }

        /// <summary>
        /// 管理员代客充值
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.代客充值.Value)]
        private void payment(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            if (userId == 0)
            {
                context.Response.Write(false, "用户名错误");
            }
            decimal money = QF("Money", decimal.Zero);
            if (money <= 0)
            {
                context.Response.Write(false, "金额错误");
            }
            int payId = QF("PayID", 0);
            if (payId == 0)
            {
                context.Response.Write(false, "充值渠道错误");
            }

            this.ShowResult(context, AdminAgent.Instance().AddRechargeOrder(userId, money, payId, QF("Description")), "提交成功");

        }

        /// <summary>
        /// 奖励类型
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void rewardtype(HttpContext context)
        {
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            int value = QF("Type") == "debit" ? 1 : 0;
            foreach (MoneyLog.MoneyType type in Enum.GetValues(typeof(MoneyLog.MoneyType)))
            {
                if ((byte)type % 2 == value) continue;
                string category = type.GetCategory().GetDescription();
                if (!dic.ContainsKey(category)) dic.Add(category, new List<string>());

                dic[category].Add(string.Format("\"{0}\":\"{1}\"", type.ToString(), type.GetDescription()));
            }

            string json = string.Join(",", dic.Select(t => string.Format("\"{0}\":{{{1}}}", t.Key, string.Join(",", t.Value))));

            context.Response.Write(true, "帐变的奖励类型", string.Concat("{", json, "}"));
        }

        /// <summary>
        /// 发放奖励
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.发放奖励.Value)]
        private void reward(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            if (userId == 0)
            {
                context.Response.Write(false, "用户名错误");
            }
            decimal money = QF("Money", decimal.Zero);
            if (money <= 0)
            {
                context.Response.Write(false, "金额错误");
            }

            MoneyLog.MoneyType type = QF("Type").ToEnum<MoneyLog.MoneyType>();
            if ((int)type % 2 != 1)
            {
                context.Response.Write(false, "类型错误");
            }
            int sourceId = WebAgent.GetRandom();
            string description = QF("Description");
            bool success = UserAgent.Instance().AddMoneyLog(userId, money, type, sourceId, description);
            if (success)
            {
                AdminInfo.Log(AdminLog.LogType.Money, "发放奖励，发放用户：{0}，金额：{1}，奖励类型：{2}，备注信息：{3}，编号：{4}",
                    QF("UserName"), money.ToString("n"), type.GetDescription(), description, sourceId);
            }

            this.ShowResult(context, success, "奖励发放成功");
        }

        /// <summary>
        /// 会员扣款
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.提现管理.会员扣款.Value)]
        private void debit(HttpContext context)
        {
            int userId = UserAgent.Instance().GetUserID(QF("UserName"));
            if (userId == 0)
            {
                context.Response.Write(false, "用户名错误");
            }
            decimal money = QF("Money", decimal.Zero);
            if (money <= 0)
            {
                context.Response.Write(false, "金额错误");
            }
            MoneyLog.MoneyType type = QF("Type").ToEnum<MoneyLog.MoneyType>();
            if ((int)type % 2 != 0)
            {
                context.Response.Write(false, "类型错误");
            }
            int sourceId = WebAgent.GetRandom();
            string description = QF("Description");
            bool success = UserAgent.Instance().AddMoneyLog(userId, money * -1, type, sourceId, description);
            if (success)
            {
                AdminInfo.Log(AdminLog.LogType.Money, "会员扣款，扣款用户：{0}，金额：{1}，扣款类型：{2}，备注信息：{3}，编号：{4}",
                    QF("UserName"), money.ToString("n"), type.GetDescription(), description, sourceId);
            }

            this.ShowResult(context, success, "扣款成功");
        }

        /// <summary>
        /// 获取提现列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.提现管理.提现记录.Value)]
        private void withdrawlist(HttpContext context)
        {
            Dictionary<int, string> dic = SiteAgent.Instance().GetWithdrawSettingList(true).ToDictionary(t => t.ID, t => t.Name);
            IQueryable<WithdrawOrder> list = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID);
            if (QF("ID", 0) != 0)
            {
                list = list.Where(t => t.ID == QF("ID", 0));
            }
            if (!string.IsNullOrEmpty(QF("User")))
            {
                int userId = UserAgent.Instance().GetUserID(QF("User"));
                if (userId != 0)
                {
                    list = list.Where(t => t.UserID == userId);
                }
                else
                {
                    list = list.Where(t => t.AccountName == QF("User"));
                }
            }
            if (!string.IsNullOrEmpty(QF("Agent")))
            {
                int agentId = UserAgent.Instance().GetUserID(QF("Agent"));
                list = list.Where(t => t.UserID == agentId || BDC.UserDepth.Where(p => p.SiteID == SiteInfo.ID && p.UserID == agentId).Select(p => p.ChildID).Contains(t.UserID));
            }
            if (WebAgent.IsType<DateTime>(QF("StartAt"))) list = list.Where(t => t.CreateAt > QF("StartAt", DateTime.Now));
            if (WebAgent.IsType<DateTime>(QF("EndAt"))) list = list.Where(t => t.CreateAt < QF("EndAt", DateTime.Now).AddDays(1));


            if (WebAgent.IsType<DateTime>(QF("StartAt1"))) list = list.Where(t => t.Appointment > QF("StartAt1", DateTime.Now));
            if (WebAgent.IsType<DateTime>(QF("EndAt1"))) list = list.Where(t => t.Appointment < QF("EndAt1", DateTime.Now).AddDays(1));

            if (!string.IsNullOrEmpty(QF("Status"))) list = list.Where(t => t.Status == QF("Status").ToEnum<WithdrawOrder.WithdrawStatus>());
            if (!string.IsNullOrEmpty(QF("WithdrawSettingID")))
            {
                list = list.Where(t => t.WithdrawSettingID == QF("WithdrawSettingID", 0));
            }
            if (QF("IsManual", 0) == 1) list = list.Where(t => t.IsManual);
            if (!string.IsNullOrEmpty(QF("Source"))) list = list.Where(t => t.Source == QF("Source"));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Money,
                t.TotalMoney,
                t.Fee,
                t.CreateAt,
                Appointment = t.GetAppointment(),
                Status = t.Status.GetDescription(),
                WithdrawSetting = dic.ContainsKey(t.WithdrawSettingID) ? dic[t.WithdrawSettingID] : "N/A",
                t.AccountName,
                t.AccountNumber,
                Bank = string.IsNullOrEmpty(t.BankName) ? t.Bank.GetDescription() : t.BankName,
                t.Description
            }, new
            {
                TotalMoney = this.Show(list.Sum(t => (decimal?)(t.Money + t.Fee))),
                Fee = this.Show(list.Sum(t => (decimal?)t.Fee)),
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 批量处理提现申请
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.提现管理.提现记录.处理提现)]
        private void withdrawbatch(HttpContext context)
        {
            var list = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == WithdrawOrder.WithdrawStatus.None && t.WithdrawSettingID == 0 && !t.IsManual && t.Money <= QF("Money", decimal.Zero));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Money,
                t.CreateAt,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.Fee,
                Status = t.Status.GetDescription()
            }, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money))
            }));
        }

        /// <summary>
        /// 获取提现详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.提现管理.提现记录.Value)]
        private void withdrawinfo(HttpContext context)
        {
            WithdrawOrder order = UserAgent.Instance().GetWithdrawOrderInfo(QF("ID", 0));
            if (order == null)
            {
                context.Response.Write(false, "提现订单编号错误");
            }
            List<WithdrawOrderLog> logList = UserAgent.Instance().GetWithdrawLogList(order.ID);

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                order.ID,
                order.UserID,
                UserName = UserAgent.Instance().GetUserName(order.UserID),
                order.Money,
                order.Fee,
                order.TotalMoney,
                order.CreateAt,
                order.Status,
                order.Description,
                order.WithdrawSettingID,
                Bank = string.IsNullOrEmpty(order.BankName) ? order.Bank.GetDescription() : order.BankName,
                order.AccountName,
                order.AccountNumber,
                order.Source,
                order.Appointment,
                recordCount = logList.Count,
                list = new JsonString(logList.ConvertAll(t => new
                {
                    Admin = AdminAgent.Instance().GetAdminName(t.AdminID),
                    t.CreateAt,
                    Status = t.Status.GetDescription(),
                    t.Description
                }).ToJson())
            });
        }

        /// <summary>
        /// 处理提现
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.提现管理.提现记录.处理提现)]
        private void withdrawcheck(HttpContext context)
        {
            string action = QF("action");
            this.ShowResult(context, UserAgent.Instance().CheckWithdrawOrder(QF("ID", 0), QF("WithdrawSettingID", 0), QF("Description"), action), "处理成功");
        }

        /// <summary>
        /// 批量处理（不改变状态，仅写入出款接口）
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.提现管理.提现记录.处理提现)]
        private void withdrawlistcheck(HttpContext context)
        {
            int id = QF("ID", 0);
            int WithdrawSettingID = QF("WithdrawSettingID", 0);
            string description = QF("Description");
            this.ShowResult(context, UserAgent.Instance().CheckWithdrawOrderList(id, WithdrawSettingID, description), "已提交至银行");
        }

        /// <summary>
        /// 提现来源
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void withdrawsource(HttpContext context)
        {
            string[] list = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID).GroupBy(t => t.Source).Select(t => t.Key).ToArray();

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.Where(t => !string.IsNullOrEmpty(t)), t => new
            {
                text = t,
                value = t
            }));
        }

        /// <summary>
        /// 充值接口对账功能
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.充值记录.Value)]
        private void rechargecheck(HttpContext context)
        {
            int payId = QF("PayID", 0);
            string content = Regex.Replace(QF("Content"), "\"|'|￥", string.Empty);
            bool isPayment = QF("IsPayment", 0) == 1;
            DateTime startAt = WebAgent.IsType<DateTime>(QF("StartAt")) ? DateTime.Parse(QF("StartAt")) : new DateTime(2000, 1, 1);
            DateTime endAt = QF("EndAt", DateTime.Now);
            if (string.IsNullOrEmpty(content))
            {
                context.Response.Write(false, "没有提交对单内容");
            }
            IQueryable<RechargeOrder> query = BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.PayID == payId && t.IsPayment);
            if (isPayment)
            {
                query = query.Where(t => t.PayAt >= startAt && t.PayAt <= endAt);
            }
            else
            {
                query = query.Where(t => t.CreateAt >= startAt && t.CreateAt <= endAt);
            }

            Dictionary<string, RechargeOrder> list = query.ToDictionary(t => t.ID.ToString(), t => t);

            //20160919011344458	300
            Regex regex = new Regex(@"(?<ID>\d{17,20})[\s\t]{0,}(?<Money>[\d\.]+)");
            List<RechargeCheck> result = new List<RechargeCheck>();
            Dictionary<string, decimal> remote = new Dictionary<string, decimal>();
            foreach (string line in content.Split('\n'))
            {
                if (!regex.IsMatch(line)) continue;

                string id = regex.Match(line).Groups["ID"].Value;
                decimal money = decimal.Parse(regex.Match(line).Groups["Money"].Value);

                if (remote.ContainsKey(id))
                {
                    context.Response.Write(false, "接口数据订单号发生重复");
                    return;
                }

                remote.Add(id, money);

                if (list.ContainsKey(id) && list[id].Amount == money)
                {
                    result.Add(new RechargeCheck(id, list[id].Amount, money, RechargeCheckStatus.Success));
                    continue;
                }
                if (!list.ContainsKey(id))
                {
                    result.Add(new RechargeCheck(id, decimal.Zero, money, RechargeCheckStatus.NoLocal));
                    continue;
                }
                if (list[id].Amount != money)
                {
                    result.Add(new RechargeCheck(id, list[id].Amount, money, RechargeCheckStatus.Money));
                    continue;
                }
            }
            foreach (string id in list.Keys)
            {
                if (!remote.ContainsKey(id))
                {
                    result.Add(new RechargeCheck(id, list[id].Amount, decimal.Zero, RechargeCheckStatus.NoGateway));
                }
            }

            Dictionary<RechargeCheckStatus, int> status = result.GroupBy(t => t.Status).ToDictionary(t => t.Key, t => t.Count());

            if (status.Count == 0)
            {
                context.Response.Write(false, "没有对账数据");
            }

            string message = string.Join("&nbsp;&nbsp;", status.Select(t => string.Format("{0}：{1}笔", t.Key.GetDescription(), t.Value)));

            context.Response.Write(true, message, this.ShowResult(result, t => new
            {
                t.ID,
                t.Money,
                t.Amount,
                t.Status,
                StatusName = t.Status.GetDescription()
            }));
        }

        /// <summary>
        /// 审核线下转账订单
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.转账审核.Value)]
        private void transferorderlist(HttpContext context)
        {
            Dictionary<int, string> paymentlist = SiteAgent.Instance().GetPaymentSettingList(true).ToDictionary(t => t.ID, t => t.Name);

            IQueryable<TransferOrder> list = BDC.TransferOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == QF("Status").ToEnum<TransferOrder.TransferStatus>());

            if (!string.IsNullOrEmpty(QF("User"))) list = list.Where(t => t.UserID == UserAgent.Instance().GetUserID(QF("User")));
            if (QF("PayID", 0) != 0) list = list.Where(t => t.PayID == QF("PayID", 0));
            if (!string.IsNullOrEmpty(QF("StartAt"))) list = list.Where(t => t.PaymentAt > QF("StartAt", DateTime.Now));
            if (!string.IsNullOrEmpty(QF("EndAt"))) list = list.Where(t => t.PaymentAt < QF("EndAt", DateTime.Now));

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserID,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                Payment = paymentlist.Get(t.PayID, "N/A"),
                t.Money,
                t.Amount,
                t.PaymentAt,
                Status = t.Status.GetDescription()
            }, new
            {
                Money = this.Show(list.Sum(t => (decimal?)t.Money)),
                Amount = this.Show(list.Sum(t => (decimal?)t.Amount))
            }));
        }

        /// <summary>
        /// 获取转账订单详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.转账审核.Value)]
        private void transferorderinfo(HttpContext context)
        {
            TransferOrder order = UserAgent.Instance().GetTransferOrderInfo(QF("ID", 0));
            if (order == null)
            {
                context.Response.Write(false, "编号错误");
            }

            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(order.PayID);
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                order.ID,
                order.UserID,
                UserName = UserAgent.Instance().GetUserName(order.UserID),
                Payment = payment.Name + (payment.IsOpen ? "" : "(停止)"),
                order.CreateAt,
                order.PaymentAt,
                order.Money,
                order.Name,
                order.Description,
                order.Amount,
                order.Status,
                StatusName = order.Status.GetDescription(),
                SystemID = order.SerialID
            });
        }

        /// <summary>
        /// 审核转账申请
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.充值管理.转账审核.Value)]
        private void checktransferorder(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().CheckTransferOrder(QF("ID", 0), QF("Amount", decimal.Zero), QF("Action").ToEnum<TransferOrder.TransferStatus>(), QF("SystemID")), "审核成功");
        }
    }
}
