using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

using SP.Studio.Data;
using IMService.Framework;
using SP.Studio.Array;

using IMService.Common;
using IMService.Common.Send;
using Rebot = IMService.Common.Rebot;
using Fleck;

namespace IMService.Agent
{
    /// <summary>
    /// 逻辑处理类
    /// </summary>
    public class IMAgent : BW.Agent.AgentBase<IMAgent>
    {
        /// <summary>
        /// 获取系统的关键词设置
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Keyword> GetKeyword()
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetKeyword",
                    NewParam("@SiteID", SysSetting.GetSetting().SiteID));
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    yield return new Keyword(dr);
                }
            }
        }
    }
}
