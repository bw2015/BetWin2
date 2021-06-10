using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

using BW.Common.Sites;


namespace BW.Common
{
    /// <summary>
    /// 用户对象
    /// </summary>
    partial class BetDataContext : DataContext, IDisposable
    {
        /// <summary>
        /// 站点对象
        /// </summary>
        public Table<Site> Site
        {
            get { return this.GetTable<Site>(); }
        }

        /// <summary>
        /// 站点域名
        /// </summary>
        public Table<SiteDomain> SiteDomain
        {
            get
            {
                return this.GetTable<SiteDomain>();
            }
        }

        /// <summary>
        /// 站点的支付渠道
        /// </summary>
        public Table<PaymentSetting> PaymentSetting
        {
            get
            {
                return this.GetTable<PaymentSetting>();
            }
        }

        /// <summary>
        /// 提现接口
        /// </summary>
        public Table<WithdrawSetting> WithdrawSetting
        {
            get
            {
                return this.GetTable<WithdrawSetting>();
            }
        }

        /// <summary>
        /// 配额设定
        /// </summary>
        public Table<QuotaSetting> QuotaSetting
        {
            get
            {
                return this.GetTable<QuotaSetting>();
            }
        }

        /// <summary>
        /// 短信发送日志
        /// </summary>
        public Table<SMSLog> SMSLog
        {
            get
            {
                return this.GetTable<SMSLog>();
            }
        }

        /// <summary>
        /// 短信验证码
        /// </summary>
        public Table<SMSCode> SMSCode
        {
            get
            {
                return this.GetTable<SMSCode>();
            }
        }

        /// <summary>
        /// 公告栏目
        /// </summary>
        public Table<NewsColumn> NewsColumn
        {
            get
            {
                return this.GetTable<NewsColumn>();
            }
        }

        /// <summary>
        /// 公告
        /// </summary>
        public Table<News> News
        {
            get
            {
                return this.GetTable<News>();
            }
        }

        /// <summary>
        /// 新闻公告的读取记录
        /// </summary>
        public Table<NewsRead> NewsRead
        {
            get
            {
                return this.GetTable<NewsRead>();
            }
        }

        /// <summary>
        /// 任务状态
        /// </summary>
        public Table<TaskStatus> TaskStatus
        {
            get
            {
                return this.GetTable<TaskStatus>();
            }
        }

        /// <summary>
        /// 会员活动设置
        /// </summary>
        public Table<Planning> Planning
        {
            get
            {
                return this.GetTable<Planning>();
            }
        }

        /// <summary>
        /// 活动的发放状态
        /// </summary>
        public Table<PlanStatus> PlanStatus
        {
            get
            {
                return this.GetTable<PlanStatus>();
            }
        }

        /// <summary>
        /// 常用语分类
        /// </summary>
        public Table<ReplyCategory> ReplyCategory
        {
            get
            {
                return this.GetTable<ReplyCategory>();
            }
        }

        /// <summary>
        /// 常用语
        /// </summary>
        public Table<Reply> Reply
        {
            get
            {
                return this.GetTable<Reply>();
            }
        }

        /// <summary>
        /// 关键字自动回复
        /// </summary>
        public Table<ReplyKeyword> ReplyKeyword
        {
            get
            {
                return this.GetTable<ReplyKeyword>();
            }
        }

        /// <summary>
        /// 自定义区域
        /// </summary>
        public Table<Region> Region
        {
            get
            {
                return this.GetTable<Region>();
            }
        }

        /// <summary>
        /// 留言反馈
        /// </summary>
        public Table<Feedback> Feedback
        {
            get
            {
                return this.GetTable<Feedback>();
            }
        }
    }
}
