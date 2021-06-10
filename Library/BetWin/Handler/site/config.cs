using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

using BW.Agent;
using SP.Studio.Model;

namespace BW.Handler.site
{
    public class config : IHandler
    {
        private void cache(HttpContext context)
        {
            SiteAgent.Instance().RemoveCache();
            context.Response.Write(true, "缓存刷新成功");
        }

        [Guest]
        private void withdrawtime(HttpContext context)
        {
            context.Response.Write(SiteInfo.Setting.IsWithdrawTime);
        }
    }
}
