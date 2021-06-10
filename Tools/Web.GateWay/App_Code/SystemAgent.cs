using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using SP.Studio.ErrorLog;
using SP.Studio.Data;

namespace Web.GateWay.App_Code
{
    public class SystemAgent : AgentBase
    {
        private static SystemAgent _instance;
        /// <summary>
        /// 返回单例对象
        /// </summary>
        /// <returns></returns>
        public static SystemAgent Instance()
        {
            if (_instance == null)
            {
                _instance = new SystemAgent();
            }

            return _instance;
        }

        /// <summary>
        /// 保存错误信息
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        public void AddError(Exception ex, string title)
        {
            string errorId;
            int httpCode;
            string content = ErrorAgent.CreateDetail(ex, out errorId, out httpCode);
            if (string.IsNullOrEmpty(title)) title = ex.Message;

            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.StoredProcedure, "SaveErrorLog",
                    NewParam("@SiteID", 0),
                    NewParam("@ErrorID", errorId),
                    NewParam("@HttpCode", httpCode),
                    NewParam("@Content", content),
                    NewParam("@Title", title));
            }
        }
    }
}