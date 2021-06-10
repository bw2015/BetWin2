using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Common.Admins;
using BW.Common.Sites;
using BW.Common.Users;
using BW.Framework;

namespace BW.PageBase
{
    /// <summary>
    /// web请求基类
    /// </summary>
    public abstract class HandlerBase : SP.Studio.PageBase.HandlerBase
    {
        protected Site SiteInfo
        {
            get
            {
                return (Site)HttpContext.Current.Items[BetModule.SITEINFO];
            }
        }

        /// <summary>
        /// 是否允许实例化本Handler实例
        /// </summary>
        public override bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        /// 执行方法
        /// </summary>
        /// <param name="context"></param>
        public override void ProcessRequest(System.Web.HttpContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 当前登录的管理员
        /// </summary>
        protected Admin AdminInfo
        {
            get
            {
                return (Admin)HttpContext.Current.Items[BetModule.ADMININFO];
            }
        }

        /// <summary>
        /// 当前登录的用户
        /// </summary>
        protected User UserInfo
        {
            get
            {
                return (User)HttpContext.Current.Items[BetModule.USERINFO];
            }
        }
    }
}
