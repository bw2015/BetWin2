using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;

using BW.Agent;
using BW.Common.Sites;
using BW.Common.Permission;
using BW.GateWay.Payment;
using BW.GateWay.Withdraw;
using BW.GateWay.Planning;

using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Xml;
using BW.Common.Users;
using SP.Studio.Data;

namespace BW.Handler.admin
{
    /// <summary>
    /// 管理员获取站点信息
    /// </summary>
    public class site : IHandler
    {
        /// <summary>
        /// 获取站点信息和当前管理员有权限的菜单
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void get(HttpContext context)
        {
            context.Response.Write(true, SiteInfo.Name, SiteInfo.ToJson(AdminInfo));
        }

        /// <summary>
        /// 获取系统支持的银行列表
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getbank(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(SiteInfo.Setting.WithdrawBankList, t => new
            {
                Name = t,
                Description = t.GetDescription()
            }));
        }

        /// <summary>
        /// 添加系统支持的提现银行
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现银行.Value)]
        private void savebank(HttpContext context)
        {
            string bank = QF("Bank");
            if (!Enum.GetNames(typeof(BankType)).Contains(bank))
            {
                context.Response.Write(false, "提交的银行参数错误");
            }
            List<string> bankList = SiteInfo.Setting.WithdrawBank.Split(',').ToList();
            if (bankList.Contains(bank))
            {
                context.Response.Write(false, "提交的银行已存在列表中");
            }
            bankList.Add(bank);
            SiteInfo.Setting.WithdrawBank = string.Join(",", bankList);
            this.ShowResult(context, SiteAgent.Instance().SaveSiteSettingString(SiteInfo), "保存成功");
        }

        /// <summary>
        /// 删除一个银行
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现银行.Value)]
        private void deletebank(HttpContext context)
        {
            string bank = QF("Bank");
            if (!Enum.GetNames(typeof(BankType)).Contains(bank))
            {
                context.Response.Write(false, "提交的银行参数错误");
            }
            List<string> bankList = SiteInfo.Setting.WithdrawBank.Split(',').ToList();
            if (!bankList.Contains(bank))
            {
                context.Response.Write(false, "列表中不存在该银行");
            }
            bankList.Remove(bank);
            SiteInfo.Setting.WithdrawBank = string.Join(",", bankList);
            this.ShowResult(context, SiteAgent.Instance().SaveSiteSettingString(SiteInfo), "删除成功");
        }

        /// <summary>
        /// 获取系统中所有的枚举类型
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void enumlist(HttpContext context)
        {

            IEnumerable<string> list = this.GetType().Assembly.GetTypes().Where(t => t.IsEnum && (t.IsPublic || t.IsNestedPublic)).Select(t =>
            {
                List<EnumObject> obj = t.ToList();
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("\"{0}\":[", t.FullName)
            .Append(string.Join(",", obj.Select(p => string.Concat("{\"value\":\"", p.Name, "\",\"text\":\"", p.Description, "\"}"))))
            .Append("]");
                return sb.ToString();
            });
            context.Response.Write(true, this.StopwatchMessage(context), string.Concat("{", string.Join(",", list), "}"));
        }

        /// <summary>
        /// 保存站点设置信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.常规设置.Value)]
        private void savesetting(HttpContext context)
        {
            Site site = context.Request.Form.Fill(SiteInfo);
            site.Setting = context.Request.Form.Fill(SiteInfo.Setting, "Setting");
            this.ShowResult(context, SiteAgent.Instance().SaveSiteSetting(site), "站点参数保存成功");
        }

        /// <summary>
        /// 获取单个支付接口的参数
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.充值接口.Value)]
        private void paymentsetting(HttpContext context)
        {
            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(QF("ID", 0)) ?? new PaymentSetting();
            PaymentType type = QF("Type", payment.Type.ToString()).ToEnum<PaymentType>();
            IPayment paymentObject = PaymentFactory.CreatePayment(type, payment.SettingString);

            context.Response.Write(true, this.StopwatchMessage(context), paymentObject.GetType().GetProperties().Where(t => t.HasAttribute<DescriptionAttribute>()).Select(t => new
            {
                Description = t.GetAttribute<DescriptionAttribute>().Description,
                Name = t.Name,
                Value = t.GetValue(paymentObject, null)
            }).ToList().ToJson());
        }

        /// <summary>
        /// 保存支付接口（新增或者修改）
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.充值接口.Value)]
        private void savepaymentinfo(HttpContext context)
        {
            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(QF("ID", 0)) ?? new PaymentSetting();
            payment.IsOpen = false;
            payment = context.Request.Form.Fill(payment);
            IPayment paymentObject = PaymentFactory.CreatePayment(payment.Type, payment.SettingString);
            payment.SettingString = paymentObject == null ? string.Empty : context.Request.Form.Fill(paymentObject, "Setting");

            this.ShowResult(context, SiteAgent.Instance().SavePaymentSetting(payment), "保存成功");
        }

        /// <summary>
        /// 系统的支付参数
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.充值接口.Value)]
        private void getpaymentlist(HttpContext context)
        {
            List<PaymentSetting> list = SiteAgent.Instance().GetPaymentSettingList(true);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Name,
                Type = t.Type.GetDescription(),
                t.Fee,
                t.IsOpen,
                t.Sort
            }));
        }

        /// <summary>
        /// 获取充值接口的设置详情
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.充值接口.Value)]
        private void getpaymentinfo(HttpContext context)
        {
            PaymentSetting setting = SiteAgent.Instance().GetPaymentSettingInfo(QF("ID", 0)) ?? new PaymentSetting();
            context.Response.Write(true, this.StopwatchMessage(context), setting.ToJson());
        }

        /// <summary>
        /// 删除一个充值接口
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.充值接口.Value)]
        private void deletepaymentsetting(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeletePaymentSetting(QF("ID", 0)), "删除成功");
        }

        /// <summary>
        /// 获取付款接口
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现接口.Value)]
        private void getwithdrawlist(HttpContext context)
        {
            List<WithdrawSetting> list = SiteAgent.Instance().GetWithdrawSettingList(QF("IsOpen", 1) == 1);
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.ID,
                t.Name,
                Type = t.Type.GetDescription(),
                t.IsOpen,
                t.Sort
            }));
        }

        /// <summary>
        /// 获取付款接口的参数设定
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现接口.Value)]
        private void withdrawsetting(HttpContext context)
        {
            WithdrawSetting setting = SiteAgent.Instance().GetWithdrawSettingInfo(QF("ID", 0)) ?? new WithdrawSetting();

            WithdrawType type = QF("Type", setting.Type.ToString()).ToEnum<WithdrawType>();
            IWithdraw withdrawObject = WithdrawFactory.CreateWithdraw(type, setting.SettingString);
            if (withdrawObject == null)
            {
                context.Response.Write(false, "无需参数配置");
            }

            context.Response.Write(true, this.StopwatchMessage(context), withdrawObject.GetType().GetProperties().Where(t => t.HasAttribute<DescriptionAttribute>()).Select(t => new
            {
                Description = t.GetAttribute<DescriptionAttribute>().Description,
                Name = t.Name,
                Value = t.GetValue(withdrawObject, null)
            }).ToList().ToJson());
        }

        /// <summary>
        /// 保存出款接口
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现接口.Value)]
        private void savewithdrawsetting(HttpContext context)
        {
            WithdrawSetting setting = SiteAgent.Instance().GetWithdrawSettingInfo(QF("ID", 0)) ?? new WithdrawSetting();
            setting.IsOpen = false;
            setting = context.Request.Form.Fill(setting);

            IWithdraw withdrawObject = WithdrawFactory.CreateWithdraw(setting.Type, setting.SettingString);
            setting.SettingString = withdrawObject == null ? string.Empty : context.Request.Form.Fill(withdrawObject, "Setting");

            this.ShowResult(context, SiteAgent.Instance().SaveWithdrawSetting(setting), "保存成功");
        }

        /// <summary>
        /// 获取提现接口信息
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现接口.Value)]
        private void getwithdrawsettinginfo(HttpContext context)
        {
            WithdrawSetting setting = SiteAgent.Instance().GetWithdrawSettingInfo(QF("ID", 0)) ?? new WithdrawSetting();
            context.Response.Write(true, this.StopwatchMessage(context), setting.ToJson());
        }

        /// <summary>
        /// 删除出款接口
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.财务管理.参数设定.提现接口.Value)]
        private void deletewithdrawsetting(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteWithdrawSetting(QF("ID", 0)), "删除成功");
        }

        /// <summary>
        /// 获取资金类型的设定参数
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.常规设置.Value)]
        private void getmoneytypelist(HttpContext context)
        {
            foreach (EnumObject obj in typeof(BW.Common.Users.MoneyLog.MoneyType).ToList())
            {
                SiteInfo.Setting.SaveMoneyTypeSetting(new Site.MoneyTypeSetting(obj));
            }
            SiteAgent.Instance().SaveSiteSettingString(SiteInfo);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(SiteInfo.Setting.MoneyTypeSetting, t => new
            {
                t.ID,
                t.Key,
                t.Name,
                t.NoTrunover
            }));
        }

        /// <summary>
        /// 保存对资金类型的参数设定
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.常规设置.Value)]
        private void savemoneytypesetting(HttpContext context)
        {
            int id = QF("id", 0);
            string name = QF("name");
            if (!SiteInfo.Setting.SaveMoneyTypeSetting(id, name, QF("value")))
            {
                context.Response.Write(false, string.Format("不存在该资金类型:{0}", id));
            }

            this.ShowResult(context, SiteAgent.Instance().SaveSiteSettingString(SiteInfo), "保存成功");
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void cache(HttpContext context)
        {
            SiteAgent.Instance().RemoveCache();
            context.Response.Write(true, "刷新成功");
        }

        /// <summary>
        /// 获取系统配置
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void getsiteconfig(HttpContext context)
        {
            XElement root = new XElement("root");
            if (!string.IsNullOrEmpty(SiteInfo.ConfigString))
            {
                root = XElement.Parse(SiteInfo.ConfigString);
            }

            List<string> config = new List<string>();
            foreach (XElement item in root.Elements("item"))
            {
                config.Add(string.Format("\"{0}\":\"{1}\"", item.GetAttributeValue("key"), item.GetAttributeValue("value")));
            }

            context.Response.Write(true, this.StopwatchMessage(context), string.Concat("{", string.Join(",", config), "}"));
        }

        /// <summary>
        /// 保存系统配置
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void savesiteconfig(HttpContext context)
        {
            string name = QF("name");
            string value = QF("value");
            if (string.IsNullOrEmpty(name))
            {
                context.Response.Write(false, "参数错误");
            }
            this.ShowResult(context, SiteAgent.Instance().SaveSiteConfig(name, value), "保存成功");
        }

        #region ============   活动管理  ==================

        /// <summary>
        /// 获取系统中的活动列表
        /// </summary>
        /// <param name="context"></param>
        private void planlist(HttpContext context)
        {
            var list = SiteAgent.Instance().GetPlanList();
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Type,
                t.IsOpen,
                t.PlanSetting.Name,
                t.PlanSetting.Description
            }));
        }

        /// <summary>
        /// 活动的详情
        /// </summary>
        /// <param name="context"></param>
        private void planinfo(HttpContext context)
        {
            Planning plan = SiteAgent.Instance().GetPlanInfo(QF("Type").ToEnum<PlanType>());

            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                plan.Type,
                plan.IsOpen,
                setting = new JsonString(plan.PlanSetting.ToString())
            });
        }

        /// <summary>
        /// 保存活动设置
        /// </summary>
        /// <param name="context"></param>
        private void saveplan(HttpContext context)
        {
            Planning plan = SiteAgent.Instance().GetPlanInfo(QF("Type").ToEnum<PlanType>());
            XElement root = new XElement("root");
            foreach (string key in context.Request.Form.AllKeys.Where(t => t.StartsWith("Setting.")))
            {
                XElement item = new XElement(key.Substring("Setting.".Length));
                item.SetValue(QF(key));
                root.Add(item);
            }
            plan.IsOpen = QF("IsOpen", 0) == 1;
            plan.Setting = root.ToString();

            this.ShowResult(context, SiteAgent.Instance().SavePlanInfo(plan), "保存成功");
        }

        /// <summary>
        /// 活动状态查看
        /// </summary>
        /// <param name="context"></param>
        private void planstatus(HttpContext context)
        {
            List<PlanStatus> list = BDC.PlanStatus.Where(t => t.SiteID == SiteInfo.ID && t.CreateAt > DateTime.Now.Date.AddDays(-3)).OrderByDescending(t => t.CreateAt).ToList();

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                Type = t.Type.GetDescription(),
                t.Total,
                t.Count,
                t.CreateAt,
                t.Time,
                t.Progress
            }));
        }

        #endregion

        #region =========== 系统任务  ===============

        /// <summary>
        /// 系统任务查看
        /// </summary>
        /// <param name="context"></param>
        private void taskstatus(HttpContext context)
        {
            List<TaskStatus> list = BDC.TaskStatus.Where(t => t.SiteID == SiteInfo.ID && t.StartAt > DateTime.Now.Date.AddDays(-3)).OrderByDescending(t => t.StartAt).ToList();

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                Type = t.Type.GetDescription(),
                t.Total,
                t.Count,
                t.StartAt,
                t.Time,
                t.Progress
            }));
        }

        #endregion

        #region =========== 域名管理 ===========

        /// <summary>
        /// 系统域名列表
        /// </summary>
        /// <param name="context"></param>
        private void domainlist(HttpContext context)
        {
            var list = BDC.SiteDomain.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.Sort).ToList();
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => new
            {
                t.Domain,
                t.IsCDN,
                t.IsSpeed,
                t.Sort
            }));
        }

        /// <summary>
        /// 删除一个域名绑定
        /// </summary>
        /// <param name="context"></param>
        private void domaindelete(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteDomain(QF("Domain")));
        }

        /// <summary>
        /// 添加域名
        /// </summary>
        /// <param name="context"></param>
        private void savedomain(HttpContext context)
        {
            string domain = QF("Domain");
            this.ShowResult(context, SiteAgent.Instance().SaveDomain(domain), "保存成功");
        }

        /// <summary>
        /// 更改域名设置
        /// </summary>
        /// <param name="context"></param>
        private void updatedomain(HttpContext context)
        {
            SiteDomain domain = BDC.SiteDomain.Where(t => t.SiteID == SiteInfo.ID && t.Domain == QF("domain")).FirstOrDefault();
            if (domain == null)
            {
                context.Response.Write(false, "域名错误");
            }
            switch (QF("name"))
            {
                case "IsCDN":
                    domain.IsCDN = QF("value", 0) == 1;
                    domain.Update(null, t => t.IsCDN);
                    break;
                case "IsSpeed":
                    domain.IsSpeed = QF("value", 0) == 1;
                    domain.Update(null, t => t.IsSpeed);
                    break;
                case "Sort":
                    domain.Sort = QF("value", (short)0);
                    domain.Update(null, t => t.Sort);
                    break;
            }
            context.Response.Write(true, "保存成功");
        }

        #endregion

        #region ========== 系统提示  =============

        /// <summary>
        /// 系统提示
        /// </summary>
        /// <param name="context"></param>
        [Admin]
        private void tip(HttpContext context)
        {
            int withdraw = 0;
            if (AdminInfo.HasPermission(AdminPermission.财务管理.提现管理.提现记录.Value))
            {
                withdraw = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == WithdrawOrder.WithdrawStatus.None && t.Appointment < DateTime.Now).Count();
            }
            int checkTransfer = 0;
            if (AdminInfo.HasPermission(AdminPermission.财务管理.充值管理.转账审核.Value))
            {
                checkTransfer = BDC.TransferOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == TransferOrder.TransferStatus.None).Count();
            }
            int systemBill = 0;
            int expireBill = 0;
            if (AdminInfo.HasPermission(AdminPermission.系统首页.贝盈首页.财务结算.Value))
            {
                systemBill = BDC.SystemBill.Where(t => t.SiteID == SiteInfo.ID && t.Status == Common.Systems.SystemBill.BillStatus.Normal).Count();
            }
            expireBill = BDC.SystemBill.Where(t => t.SiteID == SiteInfo.ID && t.Status == Common.Systems.SystemBill.BillStatus.Normal && t.EndAt < DateTime.Now.Date).Count();


            List<RechargeOrder> recharge = BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.IsPayment && t.PayAt > DateTime.Now.AddMinutes(-10)).OrderByDescending(t => t.PayAt).ToList();
            List<ChatLog> chat = BDC.ChatLog.Where(t => t.SiteID == SiteInfo.ID && !t.IsRead && t.UserID == AdminInfo.IMID && t.CreateAt < DateTime.Now.AddSeconds(-3)).ToList();
            chat.ForEach(t =>
            {
                UserAgent.Instance().MessageRead(t.ID);
            });
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                Withdraw = new JsonString(withdraw),
                Transfer = new JsonString(checkTransfer),
                Bill = new JsonString(systemBill),
                ExpireBill = new JsonString(expireBill),
                Recharge = new JsonString(recharge.ConvertAll(t => new
                {
                    t.ID,
                    t.Money,
                    UserName = UserAgent.Instance().GetUserName(t.UserID)
                }).ToJson()),
                Message = new JsonString(string.Concat("[", string.Join(",", chat.Select(t => t.ToString())), "]"))
            });
        }

        #endregion

        /// <summary>
        /// 短信发送记录查看
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.系统管理.系统设置.短信记录.Value)]
        private void smslog(HttpContext context)
        {
            IQueryable<SMSLog> list = BDC.SMSLog.Where(t => t.SiteID == SiteInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.Mobile,
                t.CreateAt,
                Status = t.Status.GetDescription(),
                t.Content,
                t.Result
            }));
        }

        /// <summary>
        /// 留言反馈列表
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.留言反馈.Value)]
        private void feedlist(HttpContext context)
        {
            IQueryable<Feedback> list = BDC.Feedback.Where(t => t.SiteID == SiteInfo.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.ID), t => new
            {
                t.ID,
                t.UserID,
                t.Type,
                UserName = UserAgent.Instance().GetUserName(t.UserID),
                t.QQ,
                t.Email,
                t.Skype,
                t.Other,
                t.Content,
                t.IP,
                t.CreateAt,
                IPAddress = UserAgent.Instance().GetIPAddress(t.IP)
            }));
        }

        /// <summary>
        /// 删除留言
        /// </summary>
        /// <param name="context"></param>
        [Admin(AdminPermission.客服管理.客服设置.留言反馈.Value)]
        private void deletefeed(HttpContext context)
        {
            this.ShowResult(context, SiteAgent.Instance().DeleteFeedback(QF("ID", 0)));
        }

        private void test(HttpContext context)
        {
            TimeSpan time = TimeSpan.Parse("23:00");

            context.Response.Write(time.ToString());
        }
    }
}
