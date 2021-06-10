using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Data;

using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Xml;

using BW.Common;
using BW.Common.Sites;
using BW.Common.Admins;
using BW.Common.Users;
using BW.Common.Lottery;
using BW.GateWay.Payment;
using System.Xml.Linq;

using BW.Framework;
using SP.Studio.Web;

namespace BW.Agent
{
    /// <summary>
    /// 站点代理
    /// </summary>
    public partial class SiteAgent : AgentBase<SiteAgent>
    {
        public SiteAgent() : base() { }

        /// <summary>
        /// 获取系统所有的授权域名
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetSiteDomain()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (SiteDomain t in BDC.SiteDomain)
            {
                dic.Add(t.Domain.ToLower(), t.SiteID);
            }
            return dic;
        }

        /// <summary>
        /// 获取指定权重的域名列表
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public List<string> GetSiteDomain(int sort)
        {
            return BDC.SiteDomain.Where(t => t.SiteID == SiteInfo.ID && t.Sort == sort).Select(t => t.Domain).ToList();
        }

        /// <summary>
        /// 根据邀请码获取域名列表（按照权重排序）
        /// </summary>
        /// <param name="invite"></param>
        /// <returns></returns>
        public List<string> GetSiteDomain(string invite)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "GetInviteDomain",
                    NewParam("@InviteID", invite));

                List<string> list = new List<string>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add((string)dr[0]);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取站点信息（无缓存）
        /// </summary>
        /// <param name="siteId">站点ID</param>
        /// <returns></returns>
        public Site GetSiteInfo(int siteId)
        {
            return BDC.Site.Where(t => t.ID == siteId).FirstOrDefault();
        }

        /// <summary>
        /// 获取站点的参数设定（web程序下调用缓存，兼容非web程序）
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public Site.SiteSetting GetSiteSetting(int siteId)
        {
            if (SiteInfo != null) return SiteInfo.Setting;
            return new Site.SiteSetting(BDC.Site.Where(t => t.ID == siteId).Select(t => t.SettingString).FirstOrDefault());
        }

        /// <summary>
        /// 使用外部输入的数据库对象获取参数设定（无缓存，兼容非web程序）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Site.SiteSetting GetSiteSetting(int siteId, DbExecutor db)
        {
            string setting = (string)db.ExecuteScalar(CommandType.Text, "SELECT Setting FROM Site WHERE SiteID = @SiteID",
               NewParam("@SiteID", siteId));
            return new Site.SiteSetting(setting);
        }

        /// <summary>
        /// 从当前域名中获取站点信息（有缓存）
        /// </summary>
        /// <returns></returns>
        public Site GetSiteInfo()
        {
            int siteId = SysSetting.GetSetting().GetSiteID();
            if (siteId == 0) return null;

            string key = BetModule.SITEINFO + "_" + siteId;
            Site site = (Site)HttpRuntime.Cache[key];
            if (site != null) return site;
            lock (key)
            {
                site = (Site)HttpRuntime.Cache[key];
                if (site != null) return site;

                site = this.GetSiteInfo(siteId);

                #region =========  站点缓存对象  ===========
                site.LotteryList = LotteryAgent.Instance().GetLotteryList(siteId).Where(t => t.IsOpen).ToArray();
                site.LotteryPlayer = new Dictionary<LotteryType, LotteryPlayer[]>();
                foreach (LotteryType type in site.LotteryList.Select(t => t.Game))
                {
                    site.LotteryPlayer.Add(type, LotteryAgent.Instance().GetPlayerList(siteId, type).Where(t => t.IsOpen && t.Player != null).ToArray());
                }

                //site.LimitedList = LotteryAgent.Instance().GetLimitedSettingList();
                #endregion
                HttpRuntime.Cache.Insert(key, site, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15));

                return site;
            }
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public void RemoveCache()
        {
            string key = BetModule.SITEINFO + "_" + SiteInfo.ID;
            HttpRuntime.Cache.Remove(key);
        }

        /// <summary>
        /// 保存站点参数
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool SaveSiteSetting(Site site)
        {
            site.ID = SiteInfo.ID;
            if (string.IsNullOrEmpty(site.Name))
            {
                base.Message("请输入站点名字");
                return false;
            }
            if (site.Setting.MinRebate >= site.Setting.MaxRebate)
            {
                base.Message("返点设置错误，最低返点应小于最大返点");
                return false;
            }
            if (site.Update() > 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "修改站点参数，修改内容：", site.ToJson());
                this.RemoveCache();
                return true;
            }
            else
            {
                this.RemoveCache();
            }
            base.Message("修改失败，可能站点不存在");
            return false;
        }

        /// <summary>
        /// 保存站点
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SaveSiteConfig(string key, object value)
        {
            XElement root = XElement.Parse(string.IsNullOrEmpty(SiteInfo.ConfigString) ? "<root />" : SiteInfo.ConfigString);
            var item = root.Elements().Where(t => t.Name == "item" && t.GetAttributeValue("key", string.Empty) == key).FirstOrDefault();
            if (item == null)
            {
                item = new XElement("item");
                item.SetAttributeValue("key", key);
                item.SetAttributeValue("value", value);
                root.Add(item);
            }
            else
            {
                item.SetAttributeValue("value", value);
            }
            SiteInfo.ConfigString = root.ToString();
            AdminInfo.Log(AdminLog.LogType.Site, "修改系统参数设定，Key:{0} Value:{1}", key, value);
            return SiteInfo.Update(null, t => t.ConfigString) == 1;
        }

        /// <summary>
        /// 获取站点的配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSiteConfig<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(SiteInfo.ConfigString)) return defaultValue;

            XElement root = XElement.Parse(SiteInfo.ConfigString);
            XElement item = root.Elements().Where(t => t.Name == "item" && t.GetAttributeValue("key", string.Empty) == key).FirstOrDefault();
            if (item == null) return defaultValue;
            return item.GetAttributeValue("value", defaultValue);
        }

        /// <summary>
        /// 从数据库中获取配置（适用于非web程序）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSiteConfig<T>(int siteId, string key, T defaultValue)
        {
            string config = SiteInfo == null ? BDC.Site.Where(t => t.ID == siteId).Select(t => t.ConfigString).FirstOrDefault() : SiteInfo.ConfigString;
            if (string.IsNullOrEmpty(config)) return defaultValue;
            XElement root = XElement.Parse(config);
            XElement item = root.Elements().Where(t => t.Name == "item" && t.GetAttributeValue("key", string.Empty) == key).FirstOrDefault();
            if (item == null) return defaultValue;
            return item.GetAttributeValue("value", defaultValue);
        }

        /// <summary>
        /// 保存站点的参数字段
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public bool SaveSiteSettingString(Site site)
        {
            site.SettingString = site.Setting;
            if (site.Update(null, t => t.SettingString) > 0)
            {
                AdminInfo.Log(AdminLog.LogType.Site, "修改站点参数，修改内容：" + site.SettingString);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取支付接口参数
        /// </summary>
        /// <param name="payId"></param>
        /// <returns></returns>
        public PaymentSetting GetPaymentSettingInfo(int payId)
        {
            if (payId == 0) return null;
            return BDC.PaymentSetting.Where(t => t.SiteID == SiteInfo.ID && t.ID == payId).FirstOrDefault();
        }

        /// <summary>
        /// 保存接口信息
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        public bool SavePaymentSetting(PaymentSetting payment)
        {
            if (string.IsNullOrEmpty(payment.Name))
            {
                base.Message("请输入接口名字");
                return false;
            }

            if (payment.MinMoney <= 0.00M || payment.MaxMoney <= 0.00M)
            {
                base.Message("请输入单笔充值金额范围");
                return false;
            }

            if (payment.MaxMoney < payment.MinMoney)
            {
                base.Message("单笔金额范围输入错误");
                return false;
            }

            if (payment.ID == 0)
            {
                payment.SiteID = SiteInfo.ID;
                if (payment.Add(true))
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "添加支付接口[{0}] {1} 参数：{2}", payment.ID, payment.Name, payment.SettingString);
                    return true;
                }
            }
            else
            {
                if (payment.SiteID != SiteInfo.ID)
                {
                    base.Message("接口编号错误");
                    return false;
                }
                if (payment.Update() == 1)
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "修改支付接口[{0}] {1} 参数：{2}", payment.ID, payment.Name, payment.SettingString);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取系统的支付接口列表
        /// </summary>
        /// <param name="isAll">是否包含全部状态</param>
        /// <returns></returns>
        public List<PaymentSetting> GetPaymentSettingList(bool isAll = false)
        {
            IQueryable<PaymentSetting> list = BDC.PaymentSetting.Where(t => t.SiteID == SiteInfo.ID);
            if (!isAll) list = list.Where(t => t.IsOpen);

            return list.OrderBy(t => t.Sort).ToList();
        }

        /// <summary>
        /// 删除一个支付接口
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeletePaymentSetting(int id)
        {
            PaymentSetting setting = this.GetPaymentSettingInfo(id);
            if (setting == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.PayID == id).Count() != 0)
            {
                base.Message("该接口已被使用，无法删除");
                return false;
            }

            return setting.Delete() != 0;
        }

        /// <summary>
        /// 根据类型和账户名获取当前的支付编号
        /// </summary>
        /// <param name="type"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public int GetPaymentID(PaymentType type, string account)
        {
            PaymentSetting setting = this.GetPaymentInfo(type, account);
            return setting == null ? 0 : setting.ID;
        }

        /// <summary>
        /// 根据类型和账户名获取当前的支付编号
        /// </summary>
        /// <param name="type"></param>
        /// <param name="account"></param>
        /// <param name="paymentObject"></param>
        /// <returns></returns>
        public PaymentSetting GetPaymentInfo(PaymentType type, string account)
        {
            List<PaymentSetting> list = BDC.PaymentSetting.Where(t => t.SiteID == SiteInfo.ID && t.Type == type && t.IsOpen).ToList();
            if (list.Count == 0) return null;
            return list.Where(t => t.PaymentObject.GetAccount() == account).FirstOrDefault();
        }

        /// <summary>
        /// 获取系统的提现设置
        /// </summary>
        /// <param name="isAll">是否包含全部状态</param>
        /// <returns></returns>
        public List<WithdrawSetting> GetWithdrawSettingList(bool isAll = false)
        {
            IQueryable<WithdrawSetting> list = BDC.WithdrawSetting.Where(t => t.SiteID == SiteInfo.ID);
            if (!isAll) list = list.Where(t => t.IsOpen);

            return list.OrderBy(t => t.Sort).ToList();
        }

        /// <summary>
        /// 获取提现接口设定
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WithdrawSetting GetWithdrawSettingInfo(int id)
        {
            return this.GetWithdrawSettingInfo(SiteInfo.ID, id);
        }

        /// <summary>
        /// 提现接口获取（非web程序）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public WithdrawSetting GetWithdrawSettingInfo(int siteId, int id)
        {
            int randomId = WebAgent.GetRandom(int.MinValue, int.MaxValue);
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT * FROM site_WithdrawSetting WHERE SiteID = @SiteID AND SettingID = @ID",
                    NewParam("@SiteID", siteId),
                    NewParam("@ID", id));
                if (ds.Tables[0].Rows.Count == 0) return null;
                return ds.Fill<WithdrawSetting>();
            }
            //return BDC.WithdrawSetting.Where(t => t.SiteID == siteId && t.ID == id && t.ID != randomId).FirstOrDefault();
        }

        /// <summary>
        /// 保存提现接口的设置
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool SaveWithdrawSetting(WithdrawSetting setting)
        {
            if (setting.ID == 0)
            {
                setting.SiteID = SiteInfo.ID;
                return setting.Add();
            }
            else
            {
                return setting.Update() != 0;
            }
        }

        /// <summary>
        /// 删除一个出款接口（如果已被使用则不能删除）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteWithdrawSetting(int id)
        {
            WithdrawSetting setting = this.GetWithdrawSettingInfo(id);
            if (setting == null)
            {
                base.Message("编号错误");
                return false;
            }

            if (BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.WithdrawSettingID == id).Count() != 0)
            {
                base.Message("该接口已被使用，无法删除");
                return false;
            }

            return setting.Delete() != 0;
        }

        /// <summary>
        /// 获取资金类型的设定（兼容非web程序）
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Site.MoneyTypeSetting GetMoneyTypeSetting(int siteId, MoneyLog.MoneyType type)
        {
            List<Site.MoneyTypeSetting> list = null;
            if (SiteInfo != null && SiteInfo.ID == siteId)
            {
                list = SiteInfo.Setting.MoneyTypeSetting;
            }
            else
            {
                list = this.GetSiteSetting(siteId).MoneyTypeSetting;
            }
            return list.Find(t => t.ID == (int)type);
        }

        #region ========= 配额管理  ============


        /// <summary>
        /// 获取系统配额设定
        /// </summary>
        /// <returns></returns>
        public List<QuotaSetting> GetQuotaSettingList()
        {
            return BDC.QuotaSetting.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.MinRebate).ToList();
        }

        /// <summary>
        /// 保存系统的配额设定
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool SaveQuotaSetting(QuotaSetting setting)
        {
            if (setting.MinRebate > setting.MaxRebate)
            {
                base.Message("返点区间设置错误");
                return false;
            }
            if (setting.MinRebate < SiteInfo.Setting.MinRebate)
            {
                base.Message("最低返点不能小于{0}", SiteInfo.Setting.MinRebate);
                return false;
            }
            if (setting.MaxRebate > SiteInfo.Setting.MaxRebate)
            {
                base.Message("最高返点不能大于{0}", SiteInfo.Setting.MaxRebate);
                return false;
            }
            if (setting.Number < 0)
            {
                base.Message("配额数量错误");
                return false;
            }

            List<QuotaSetting> list = this.GetQuotaSettingList();
            if (list.Exists(t =>
                (t.MinRebate <= setting.MinRebate && t.MaxRebate >= setting.MinRebate) ||
                (t.MinRebate <= setting.MaxRebate && t.MaxRebate >= setting.MaxRebate)
                ))
            {
                base.Message("返点区间设置重复");
                return false;
            }

            setting.SiteID = SiteInfo.ID;
            return setting.Add();
        }

        /// <summary>
        /// 获取一个配额配置选项
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public QuotaSetting GetQuotaSettingInfo(int id)
        {
            return BDC.QuotaSetting.Where(t => t.SiteID == SiteInfo.ID && t.ID == id).FirstOrDefault();
        }

        /// <summary>
        /// 删除一个配额配置
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteQuota(int id)
        {
            QuotaSetting setting = this.GetQuotaSettingInfo(id);
            if (setting == null)
            {
                base.Message("编号错误");
                return false;
            }

            return setting.Delete() == 1;
        }

        #endregion

        /// <summary>
        /// 删除域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public bool DeleteDomain(string domain)
        {
            if (BDC.SiteDomain.Where(t => t.Domain == domain && t.SiteID == SiteInfo.ID).Count() == 0)
            {
                base.Message("删除失败");
                return false;
            }

            return new SiteDomain() { SiteID = SiteInfo.ID, Domain = domain }.Delete(t => t.SiteID, t => t.Domain) != 0;
        }

        /// <summary>
        /// 保存域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public bool SaveDomain(string domain)
        {
            Regex regex = new Regex(@"^[a-z0-9\.\-_:]{5,}$");
            if (!regex.IsMatch(domain))
            {
                base.Message("域名错误");
                return false;
            }

            if (BDC.SiteDomain.Where(t => t.Domain == domain).Count() != 0)
            {
                base.Message("该域名不能添加");
                return false;
            }

            return new SiteDomain()
            {
                Domain = domain,
                SiteID = SiteInfo.ID
            }.Add();
        }

        /// <summary>
        /// 保存留言
        /// </summary>
        /// <param name="feed"></param>
        /// <returns></returns>
        public bool SaveFeedback(Feedback feed)
        {
            feed.IP = IPAgent.IP;
            feed.CreateAt = DateTime.Now;
            feed.SiteID = SiteInfo.ID;
            feed.UserID = UserInfo == null ? 0 : UserInfo.ID;

            if (!string.IsNullOrEmpty(feed.Content)) feed.Content = HttpUtility.HtmlDecode(feed.Content);
            if (string.IsNullOrEmpty(feed.QQ) && string.IsNullOrEmpty(feed.Skype) && string.IsNullOrEmpty(feed.Email) && string.IsNullOrEmpty(feed.Other))
            {
                base.Message("请填写联系信息");
                return false;
            }
            return feed.Add();
        }


        /// <summary>
        /// 删除反馈信息
        /// </summary>
        /// <param name="feedId"></param>
        /// <returns></returns>
        public bool DeleteFeedback(int feedId)
        {
            Feedback feed = BDC.Feedback.Where(t => t.SiteID == SiteInfo.ID && t.ID == feedId).FirstOrDefault();
            if (feed == null)
            {
                base.Message("编号错误");
                return false;
            }
            if (feed.Delete() == 1)
            {
                if (AdminInfo != null)
                {
                    AdminInfo.Log(AdminLog.LogType.Site, "删除留言反馈，编号：{0}。 内容：{1}", feed.ID, feed.ToJson());
                }
                return true;
            }
            base.Message("发生不可描述的错误");
            return false;
        }

        /// <summary>
        /// 保存跨站数据缓存
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Guid SaveCache(SiteCache.CacheType type, Dictionary<string, string> dic, params string[] args)
        {
            XElement root = new XElement("root");
            foreach (KeyValuePair<string, string> item in dic)
            {
                XElement t = new XElement("item");
                t.SetAttributeValue("key", item.Key);
                t.SetValue(item.Value);
                root.Add(t);
            }
            for (int i = 0; i < args.Length; i += 2)
            {
                root.SetAttributeValue(args[i], args[i + 1]);
            }

            Guid id = Guid.NewGuid();

            new SiteCache()
            {
                ID = id,
                CreateAt = DateTime.Now,
                SiteID = SiteInfo.ID,
                Data = root.ToString(),
                Type = type
            }.Add();

            return id;
        }
    }
}
