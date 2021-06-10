using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BW.Framework;
using SP.Studio.Data;
using System.Data;
using System.Web;
using SP.Studio.Web;

namespace BW.Agent
{
    /// <summary>
    /// 日志相关
    /// </summary>
    public partial class SNAPAgent : AgentBase<SNAPAgent>
    {
        public SNAPAgent() : base(SysSetting.GetSetting().SNAPConnection) { }

        /// <summary>
        /// 添加一个错误日志
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="errorId"></param>
        /// <param name="httpCode"></param>
        /// <param name="content"></param>
        /// <param name="title"></param>
        public void AddErrorLog(int siteId, string errorId, int httpCode, string content, string title)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveErrorLog",
                    NewParam("@SiteID", siteId),
                    NewParam("@ErrorID", errorId),
                    NewParam("@HttpCode", httpCode),
                    NewParam("@Content", content),
                    NewParam("@Title", title));
            }
        }

        /// <summary>
        /// 添加一个日志处理时间
        /// </summary>
        /// <param name="time">处理时间</param>
        public void AddHandlerLog(short time)
        {
            HttpContext context = HttpContext.Current;
            if (context == null) return;

            string data = string.Empty;
            if (context.Request.HttpMethod == "POST")
            {
                data = Encoding.UTF8.GetString(WebAgent.GetInputSteam(context));
            }
            else
            {
                data = context.Request.QueryString.ToString();
            }
            int siteID = SiteInfo.ID;
            int userId = WebAgent.QC(BetModule.USERINFO, 0);
            string method = context.Request.RawUrl;
            if (method.Contains('?')) method = method.Substring(0, method.IndexOf('?'));
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveHandler",
                    NewParam("@SiteID", siteID),
                    NewParam("@UserID", userId),
                    NewParam("@Method", method),
                    NewParam("@Data", data),
                    NewParam("@Time", time));
            }
        }

        /// <summary>
        /// 添加一条系统处理日志
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="content"></param>
        public void AddSystemLog(int siteId, string content)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveSystemLog",
                    NewParam("@SiteID", siteId),
                    NewParam("@Content", content));
            }
        }
    }
}
