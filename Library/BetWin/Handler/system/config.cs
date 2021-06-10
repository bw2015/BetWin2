using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using SP.Studio.Model;
using SP.Studio.Core;
using SP.Studio.Data;
using SP.Studio.Data.Linq;
using SP.Studio.Web;
using BW.Common.Users;

using BW.Agent;

using SP.Studio.ErrorLog;
namespace BW.Handler.system
{
    /// <summary>
    /// 系统提供的接口
    /// </summary>
    public class config : IHandler
    {
        /// <summary>
        /// 获取枚举的属性
        /// </summary>
        /// <param name="context"></param>
        private void enumtype(HttpContext context)
        {
            string name = QF("Enum");

            Type type = this.GetType().Assembly.GetType(name);
            if (type == null)
            {
                context.Response.Write(false, string.Format("找不到类型：{0}", name));
            }

            context.Response.Write(true, name, type.ToList().ConvertAll(t => new
            {
                value = t.Name,
                text = t.Description
            }));
        }

        /// <summary>
        /// 记录HTTP发送过来的数据log
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void httplog(HttpContext context)
        {
            SystemAgent.Instance().AddErrorLog(SiteInfo.ID, new Exception("HttpLog Http日志记录"));
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (string key in context.Request.Headers.AllKeys)
            {
                dic.Add(key, context.Request.Headers[key]);
            }
            context.Response.Write(true, this.StopwatchMessage(context), dic.ToJson());
        }

        /// <summary>
        /// 响应测速请求
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void ping(HttpContext context)
        {
            context.Response.Write(true, string.Empty);
        }

        /// <summary>
        /// 重写到当前域名的https接口
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void https(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Write(BW.Resources.Res.HTTPS);
        }

        [Guest]
        private void test(HttpContext context)
        {
            var list = BDC.LotteryOrder.Where(t => t.SiteID == SiteInfo.ID).Select(t => new
            {
                t.ID,
                t.UserID
            }).OrderByDescending(t => t.ID);

            context.Response.Write(true, this.StopwatchMessage(context), this.ShowLinqResult(list, BDC.LotteryOrder, LockType.READPAST, t => new
            {
                t.ID,
                t.UserID
            }));
        }

        [Guest]
        private void playerlist(HttpContext context)
        {
            context.Response.Write(true, this.StopwatchMessage(context), SiteInfo.LotteryPlayerInfo.ToJson());
        }

        /// <summary>
        /// 资金类型
        /// </summary>
        /// <param name="context"></param>
        [Guest]
        private void moneytype(HttpContext context)
        {
            StringBuilder sb = new StringBuilder();
            foreach (MoneyLog.MoneyType type in Enum.GetValues(typeof(MoneyLog.MoneyType)))
            {
                MoneyLog.MoneyCategoryType category = type.GetCategory();
                bool isWin = !MoneyLog.NO_WIN_CATEGORY.Contains(category);

                sb.AppendFormat("IF EXISTS(SELECT 0 FROM sys_MoneyType WHERE [Type] = {0}) BEGIN " +
"    UPDATE sys_MoneyType SET Category = {1}, IsWin = {2} WHERE[Type] = {0} " +
"END ELSE BEGIN " +
"    INSERT INTO sys_MoneyType VALUES({0}, {1}, {2}) " +
"END", (int)type, (int)category, isWin ? 1 : 0)
.AppendLine();
            }
            context.Response.Write(sb);
        }

    }
}
