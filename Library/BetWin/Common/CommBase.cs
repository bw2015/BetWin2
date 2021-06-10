using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Reflection;

using BW.Common.Sites;
using BW.Framework;

namespace BW.Common
{
    /// <summary>
    /// 实体类的基类
    /// </summary>
    public abstract class CommBase
    {
        public CommBase() { }

        protected internal Site SiteInfo
        {
            get
            {
                if (HttpContext.Current == null) return null;
                return (Site)HttpContext.Current.Items[BetModule.SITEINFO];
            }
        }
    }
}
