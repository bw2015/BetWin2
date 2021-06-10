using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Configuration;
using System.Timers;

using BW.Agent;
using BW.Common.Sites;

using IMService.Common;
using IMService.Agent;
using Fleck;

namespace IMService.Framework
{
    public class SysSetting
    {
        #region ================常量================

        public const string KEY_ADMIN = "Admin";

        /// <summary>
        /// 更新缓存时候的锁
        /// </summary>
        public const string LOCK_CACHE = "LOCK_CACHE";

        #endregion

        /// <summary>
        /// 定时器（5分钟更新一次）
        /// </summary>
        private static Timer timer = new Timer(5 * 60 * 1000);

        static SysSetting()
        {
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SysSetting.GetSetting().Install();
        }

        private int _siteId;
        /// <summary>
        /// 当前站点
        /// </summary>
        internal int SiteID
        {
            get
            {
                return _siteId;
            }
            set
            {
                _siteId = value;
                this.Install();
            }
        }

        /// <summary>
        /// 当前站点对象
        /// </summary>
        public Site SiteInfo { get; private set; }

        /// <summary>
        /// 自定义的关键词
        /// </summary>
        internal Keyword[] Key { get; private set; }


        /// <summary>
        /// 当前连接的客户端 Type-ID
        /// </summary>
        public Dictionary<string, User> Client = new Dictionary<string, User>();

        /// <summary>
        /// 在线的链接
        /// </summary>
        public Dictionary<IWebSocketConnection, OnlineStatus> Online = new Dictionary<IWebSocketConnection, OnlineStatus>(); 


        private void Install()
        {
            this._loadCache();

            this.SiteInfo = SiteAgent.Instance().GetSiteInfo(this.SiteID);
        }

        /// <summary>
        /// 加载机器人相关（机器人设置+关键词回复）
        /// </summary>
        private void _loadCache()
        {
            this.Key = IMAgent.Instance().GetKeyword().ToArray();
        }



        public static SysSetting GetSetting()
        {
            return Nested.intance;
        }

        class Nested
        {
            internal readonly static SysSetting intance = new SysSetting();
        }
    }
}
