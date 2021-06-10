using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Reflection;
using System.Xml.Linq;

using BW.Common.Sites;
using BW.Agent;
using SP.Studio.Core;
using SP.Studio.Model;
using SP.Studio.Web;

namespace BW.Handler.site
{
    /// <summary>
    /// 站点信息
    /// </summary>
    public class info : IHandler
    {

        /// <summary>
        /// 当前站点的信息
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void get(HttpContext context)
        {
            if (SiteInfo.Status != Site.SiteStatus.Normal)
            {
                context.Response.Write(false, SiteInfo.StopDesc, new
                {
                    Type = ErrorType.Stop
                });
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                SiteInfo.ID,
                SiteInfo.Name,
                Setting = new JsonString(SiteInfo.Setting.ToJson(t => t.CardTime, t => t.CustomerServer, t => t.IsSameRebate, t => t.DefaultPassword, t => t.IsWithdrawTime, t => t.MaxCard,
                    t => t.MaxRebate, t => t.MinRebate, t => t.SameAccountName, t => t.ServiceServer, t => t.WithdrawBank, t => t.LotteryMode,
                    t => t.WithdrawCount, t => t.WithdrawMax, t => t.WithdrawMin, t => t.WithdrawTime, t => t.WithdrawTime1, t => t.Turnover,
                    t => t.WithdrawTime2, t => t.CustomerServer, t => t.APPAndroid, t => t.APPIOS, t => t.APPPC, t => t.Wechat, t => t.RegisterInvite, t => t.APP, t => t.APPVersion, t => t.Guaji)),
                Domain = new JsonString(BDC.SiteDomain.Where(t => t.SiteID == SiteInfo.ID).OrderByDescending(t => t.Sort).ToList().ToJson(t => t.Link, t => t.IsCDN, t => t.IsSpeed))
            });
        }

        /// <summary>
        /// 支付接口列表
        /// </summary>
        /// <param name="context"></param>
        private void paymentlist(HttpContext context)
        {
            if (SiteInfo.Setting.RechargeNeedBank && UserAgent.Instance().GetBankAccountList(UserInfo.ID).Count == 0)
            {
                context.Response.Write(false, "请先绑定提现银行卡", new
                {
                    Type = ErrorType.BankAccount
                });
            }
            List<PaymentSetting> list = SiteAgent.Instance().GetPaymentSettingList();
            if (!string.IsNullOrEmpty(QF("Platform")))
            {
                PaymentSetting.PlatformType platform = QF("Platform").ToEnum<PaymentSetting.PlatformType>();
                list = list.FindAll(t => t.Platform.HasFlag(platform));
            }

            if (SiteInfo.UserGroup.ContainsKey(UserInfo.GroupID))
            {
                list = list.FindAll(t => SiteInfo.UserGroup[UserInfo.GroupID].Setting.Payment.Length == 0 || SiteInfo.UserGroup[UserInfo.GroupID].Setting.Payment.Contains(t.ID));
            }

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list, t => t.ToString()));
        }

        /// <summary>
        /// 获取支付接口的信息
        /// </summary>
        /// <param name="context"></param>
        private void paymentinfo(HttpContext context)
        {
            int id = QF("id", 0);
            PaymentSetting payment = SiteAgent.Instance().GetPaymentSettingInfo(id);
            if (payment == null || !payment.IsOpen)
            {
                context.Response.Write(false, "接口编号错误");
            }
            context.Response.Write(true, this.StopwatchMessage(context), payment.ToString());
        }

        /// <summary>
        /// 栏目列表
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void columnlist(HttpContext context)
        {
            NewsColumn.ContentType type = QF("Type").ToEnum<NewsColumn.ContentType>();
            bool isTitle = QF("Title", 0) == 1;
            List<News> list = null;
            if (isTitle)
            {
                list = BDC.News.Where(t => t.SiteID == SiteInfo.ID).Join(BDC.NewsColumn.Where(t => t.SiteID == SiteInfo.ID && t.Type == type), t => t.ColID, t => t.ID, (t, col) => new
                {
                    t.ID,
                    t.ColID,
                    t.Title
                }).ToList().ConvertAll(t => new News()
                {
                    ID = t.ID,
                    ColID = t.ColID,
                    Title = t.Title
                });
            }
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(SiteAgent.Instance().GetNewsColumnList(type), t => new
            {
                t.ID,
                t.Name,
                Content = new JsonString(list == null ? "null" : list.FindAll(p => p.ColID == t.ID).ToJson(p => p.ID, p => p.Title))
            }));
        }

        /// <summary>
        /// 新闻公告
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void newslist(HttpContext context)
        {
            NewsColumn.ContentType type = QF("Type").ToEnum<NewsColumn.ContentType>();

            IQueryable<News> list = BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.Type == type);
            // if (type == NewsColumn.ContentType.News) list = list.Where(t => t.CreateAt > SiteInfo.StartDate);

            Dictionary<int, string> column = BDC.NewsColumn.Where(t => t.SiteID == SiteInfo.ID && t.Type == type).ToDictionary(t => t.ID, t => t.Name);
            if (QF("ColID", 0) != 0) list = list.Where(t => t.ColID == QF("ColID", 0));
            bool isContent = QF("Content", 0) == 1;


            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.Sort).ThenByDescending(t => t.ID), t => new
            {
                Column = column[t.ColID],
                t.ID,
                t.Title,
                CreateAt = t.CreateAt.ToShortDateString(),
                t.IsTip,
                Cover = t.CoverShow,
                Content = isContent ? t.Content : ""
            }, new
            {
                Tip = SiteAgent.Instance().GetNewsTip(type)
            }));
        }

        /// <summary>
        /// 需要弹出提示的通知公告
        /// </summary>
        /// <param name="context"></param>
        private void newstip(HttpContext context)
        {
            int newsId = SiteAgent.Instance().GetNewsTip(NewsColumn.ContentType.News);
            if (newsId == 0)
            {
                context.Response.Write(false, "没有弹窗");
            }
            else
            {
                context.Response.Write(true, this.StopwatchMessage(context), new
                {
                    ID = newsId
                });
            }
        }

        /// <summary>
        /// 包括内容的文章列表
        /// </summary>
        /// <param name="context"></param>
        private void newscontentlist(HttpContext context)
        {
            NewsColumn column = SiteAgent.Instance().GetNewsColumnInfo(QF("ColID", 0));
            if (column == null)
            {
                context.Response.Write(false, "栏目编号错误");
            }
            IQueryable<News> list = BDC.News.Where(t => t.SiteID == SiteInfo.ID && t.ColID == column.ID);
            int index = 1;
            context.Response.Write(true, this.StopwatchMessage(context), this.ShowResult(list.OrderByDescending(t => t.Sort).ThenByDescending(t => t.ID).ToArray(), t => new
            {
                t.ID,
                Title = string.Format("{0}#{1}", index++, t.Title),
                t.Content,
                t.CreateAt
            }, new
            {
                Name = column.Name
            }));
        }

        /// <summary>
        /// 获取新闻详情
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void newsinfo(HttpContext context)
        {
            News next;
            News previous;
            News news = SiteAgent.Instance().GetNewsInfo(QF("ID", 0), out next, out previous);
            if (news == null)
            {
                context.Response.Write(false, "编号错误");
            }
            if (UserInfo != null)
            {
                SiteAgent.Instance().NewsRead(news.ID);
            }
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                news.Title,
                CreateAt = news.CreateAt.ToLongDateString(),
                news.Content,
                Next = new JsonString(next == null ? "null" : next.ToJson(t => t.ID, t => t.Title, t => t.CreateAt)),
                Previous = new JsonString(previous == null ? "null" : previous.ToJson(t => t.ID, t => t.Title, t => t.CreateAt))
            });
        }

        /// <summary>
        /// 删除站内信
        /// </summary>
        /// <param name="context"></param>
        private void messagedelete(HttpContext context)
        {
            this.ShowResult(context, UserAgent.Instance().MessageDelete(QF("id", 0)), "删除成功");
        }

        /// <summary>
        /// 当前站点启用的域名
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void domain(HttpContext context)
        {
            context.Response.Write(true, SiteInfo.Name, this.ShowResult(SiteAgent.Instance().GetSiteDomain().Where(t => t.Value == SiteInfo.ID).OrderBy(t => Guid.NewGuid()), t => new
            {
                Domain = t.Key
            }));
        }

        /// <summary>
        /// 获取当前站点指定权重的域名
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void domainlist(HttpContext context)
        {
            int sort = QF("Sort", 0);
            context.Response.Write(true, this.StopwatchMessage(context),
                string.Concat("[", string.Join(",", SiteAgent.Instance().GetSiteDomain(sort).Select(t => "\"" + t + "\"")), "]")
                );
        }

        /// <summary>
        /// 检查域名是否站点所有
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void checkdomain(HttpContext context)
        {
            string name = QF("Domain");
            if (string.IsNullOrEmpty(name))
            {
                context.Response.Write(false, "请输入域名");
            }
            Dictionary<string, int> domain = SiteAgent.Instance().GetSiteDomain();
            Regex regex = new Regex(@"//(?<Domain>[\w\.\:]+)/{0,}|^(?<Domain>[\w\.\:]+)/{0,}", RegexOptions.IgnoreCase);
            if (regex.IsMatch(name)) name = regex.Match(name).Groups["Domain"].Value;
            name = name.ToLower();

            this.ShowResult(context, domain.ContainsKey(name), "检测成功");
        }


        /// <summary>
        /// 根据邀请码获取站点的域名列表
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void invitedomain(HttpContext context)
        {
            string code = QF("code");

            context.Response.ContentType = "text/xml";
            XElement root = new XElement("root");
            SiteAgent.Instance().GetSiteDomain(code).ForEach(t =>
            {
                XElement item = new XElement("itme");
                item.SetValue(t);
                root.Add(item);
            });
            context.Response.Write(root);
        }

        /// <summary>
        /// 根据站点id输出站点的设置信息
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void setting(HttpContext context)
        {
            int siteId = QF("SiteID", SiteInfo.ID);
            Site site = SiteAgent.Instance().GetSiteInfo(siteId);
            XElement root = new XElement("root");
            root.Add(new XElement("SiteName", site.Name));
            string[] names = QF("Property").Split(',');
            foreach (PropertyInfo property in typeof(Site.SiteSetting).GetProperties().Where(t => names.Contains(t.Name)))
            {
                root.Add(new XElement(property.Name, property.GetValue(site.Setting)));
            }
            context.Response.ContentType = "text/xml";
            context.Response.Write(root);
        }

        /// <summary>
        /// 獲取區域信息
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void regioninfo(HttpContext context)
        {
            int id = QF("ID", 0);
            string name = QF("Name");
            Region region = null;
            if (id != 0)
            {
                region = SiteAgent.Instance().GetRegionInfo(id);
            }
            else if (!string.IsNullOrEmpty(name))
            {
                region = SiteAgent.Instance().GetRegionInfo(name);
            }
            if (region == null)
                context.Response.Write(false, "参数错误");
            context.Response.Write(true, this.StopwatchMessage(context), new
            {
                region.ID,
                region.Name,
                region.Title,
                region.Content,
                region.CreateAt
            });
        }

        /// <summary>
        /// 获取区域列表信息（根据区域名称）
        /// </summary>
        /// <param name="contenxt"></param>
        [Guest]
        private void regionlist(HttpContext contenxt)
        {
            string name = QF("Name");
            if (string.IsNullOrEmpty(name))
            {
                contenxt.Response.Write(false, "未指定名称");
            }
            var list = BDC.Region.Where(t => t.SiteID == SiteInfo.ID && t.Name.Contains(name)).Select(t => new
            {
                t.ID,
                t.Name,
                t.Title,
                TimeStamp = WebAgent.GetTimeStamp(t.CreateAt)
            }).OrderBy(t => t.Name);
            contenxt.Response.Write(true, this.StopwatchMessage(contenxt), this.ShowResult(list, t => t));
        }

        /// <summary>
        /// 站点数据统计
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void statistics(HttpContext context)
        {
            string key = "statistics-" + SiteInfo.ID;
            object result = HttpRuntime.Cache[key];
            if (result == null)
            {

                int user = BDC.User.Where(t => t.SiteID == SiteInfo.ID).Count();
                int bet = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID).Count() + BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID).Count() + BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID).Count();
                int recharge = BDC.RechargeOrder.Where(t => t.SiteID == SiteInfo.ID && t.IsPayment).Count();
                int withdraw = BDC.WithdrawOrder.Where(t => t.SiteID == SiteInfo.ID && t.Status == Common.Users.WithdrawOrder.WithdrawStatus.Finish).Count();
                decimal reward = this.Show(BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID).Sum(t => (decimal?)t.Reward)) +
                   this.Show(BDC.VideoLog.Where(t => t.SiteID == SiteInfo.ID).Where(t => t.Money > 0).Sum(t => (decimal?)t.Money)) +
                   this.Show(BDC.SlotLog.Where(t => t.SiteID == SiteInfo.ID).Where(t => t.Money > 0).Sum(t => (decimal?)t.Money));

                result = new
                {
                    User = user,
                    Bet = bet,
                    Recharge = recharge,
                    Withdraw = withdraw,
                    Reward = reward
                };

                HttpRuntime.Cache.Insert(key, result, BW.Framework.BetModule.SiteCacheDependency, DateTime.Now.AddMinutes(10), System.Web.Caching.Cache.NoSlidingExpiration);
            }

            context.Response.Write(true, this.StopwatchMessage(context), result);
        }

        /// <summary>
        /// CDN国内高防域名（与站点无关，仅接受get请求）
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void cdndomain(HttpContext context)
        {
            context.Response.ContentType = "text/javascript:";
            if (context.Request.UrlReferrer == null) return;
            string domain = context.Request.UrlReferrer.Authority;
            SiteDomain siteDomain = BDC.SiteDomain.Where(t => t.Domain == domain).FirstOrDefault();
            if (siteDomain == null) return;
            string[] list = BDC.SiteDomain.Where(t => t.SiteID == siteDomain.SiteID && t.Domain != domain && t.IsCDN && !t.IsSpeed).Select(t => string.Format("\"{0}\"", t.Domain)).ToArray();

            Site site = SiteAgent.Instance().GetSiteInfo(siteDomain.SiteID);

            StringBuilder sb = new StringBuilder();
            sb.Append("var SITE = {")
                .AppendFormat("\"ID\":{0},\"Name\":\"{1}\"", site.ID, site.Name)
                .AppendFormat(",\"Domain\":[{0}]", string.Join(",", list.OrderBy(t => Guid.NewGuid())))
                .Append("}");

            context.Response.Write(sb);
        }

        [Guest]
        private void savefeedback(HttpContext context)
        {
            Feedback feed = context.Request.Form.Fill<Feedback>();
            this.ShowResult(context, SiteAgent.Instance().SaveFeedback(feed), "保存成功");
        }
    }
}
