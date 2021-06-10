using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

using SP.Studio.Data;
using SP.Studio.Array;
using BW.IM.Common;

namespace BW.IM.Agent
{
    public class SiteAgent : AgentBase<SiteAgent>
    {
        private readonly Regex siteRegex = new Regex(@"\/1\d{3}\/");

        /// <summary>
        /// 获取当前站点的SiteID
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public int GetSiteID(HttpContext context)
        {
            string domain = context.Request.Url.Authority;
            if (!siteRegex.IsMatch(domain)) return 0;
            return int.Parse(siteRegex.Match(domain).Value);
        }

        /// <summary>
        /// 插入一条错误日志信息
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        public void AddErrorLog(int siteId, Exception ex, string title = null)
        {
            string errorId;
            int httpCode;
            string content = SP.Studio.ErrorLog.ErrorAgent.CreateDetail(ex, out errorId, out httpCode);
            if (string.IsNullOrEmpty(title)) title = ex.Message;
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "INSERT INTO log_Error(SiteID,ErrorID,HttpCode,Content,Title,CreateAt) VALUES(@SiteID,@ErrorID,@HttpCode,@Content,@Title,GETDATE())",
                    NewParam("@SiteID", siteId),
                    NewParam("@ErrorID", errorId),
                    NewParam("@HttpCode", httpCode),
                    NewParam("@Content", content),
                    NewParam("@Title", title));
            }
        }

        public void AddSystemLog(int siteId, string content)
        {
            using (DbExecutor db = NewExecutor())
            {
                db.ExecuteNonQuery(CommandType.Text, "INSERT INTO log_System(SiteID,Content,CreateAt) VALUES(@SiteID,@Content,GETDATE())",
                    NewParam("@SiteID", siteId),
                    NewParam("@Content", content));
            }
        }


        /// <summary>
        /// 获取自定义的彩种名字
        /// </summary>
        /// <param name="type"></param>
        public string GetLotteryName(GroupType type)
        {
            Dictionary<string, string> _lotteryName = (Dictionary<string, string>)HttpRuntime.Cache["GetLotteryName"];
            if (_lotteryName != null)
            {
                return _lotteryName.Get(string.Concat(this.UserInfo.SiteID, "-", type), string.Empty);
            }

            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT SiteID,Game,GameName FROM lot_Setting WHERE IsOpen = 1");
                _lotteryName = new Dictionary<string, string>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    GroupType game = (GroupType)dr["Game"];
                    string key = string.Concat(dr["SiteID"], "-", game);
                    _lotteryName.Add(key, (string)dr["GameName"]);
                }
            }

            HttpRuntime.Cache.Insert("GetLotteryName", _lotteryName, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10));
            return this.GetLotteryName(type);
        }

        /// <summary>
        /// 获取系统所有的群参数设定
        /// </summary>
        /// <returns></returns>
        public List<GroupSetting> GetGroupSetting()
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetGroupSetting");

                List<GroupSetting> list = new List<GroupSetting>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new GroupSetting(dr));
                }
                return list;
            }
        }

        /// <summary>
        /// 获取系统中全部开放的机器人
        /// </summary>
        /// <returns></returns>
        public List<Rebot> GetRebot()
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "IM_GetRebot");

                List<Rebot> list = new List<Rebot>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    list.Add(new Rebot(dr));
                }
                return list;
            }
        }
    }
}
