using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using BW.Agent;
using SP.Studio.Model;

namespace BW.Handler.manage
{
    public class test : IHandler
    {
        private void plan(HttpContext context)
        {
            SiteAgent.Instance().PlanRun();
            context.Response.Write(true, this.StopwatchMessage(context));
        }

        /// <summary>
        /// 提现时间测试
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void withdrawtime(HttpContext context)
        {
            context.Response.Write(SiteInfo.Setting.IsWithdrawTime, SiteInfo.Setting.WithdrawTime);
        }
    }
}
