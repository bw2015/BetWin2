using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using SP.Studio.Data;

using BW.Common.Users;
using BW.Common.Games;

using BW.Common.Reports;

namespace BW.Agent
{
    /// <summary>
    /// 报表查询
    /// </summary>
    partial class UserAgent
    {
        /// <summary>
        /// 获取用户下面所有直属代理的业绩（包括自身）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <param name="self">是否仅显示自己</param>
        /// <returns></returns>
        public List<UserReport> GetTeamStatistic(int userId, DateTime startAt, DateTime endAt, bool self = false)
        {
            List<UserReport> list = new List<UserReport>();
            DataSet ds;
            using (DbExecutor db = NewExecutor())
            {
                ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_TeamStatistic",
                    NewParam("@UserID", userId),
                   NewParam("@StartAt", startAt),
                   NewParam("@EndAt", endAt),
                   NewParam("@Self", self));
            }

            foreach (int id in ds.Tables[0].ToList<int>())
            {
                list.Add(new UserReport(id,
                    ds.Tables[1].Select("UserID = " + id).ToDictionary(t => (MoneyLog.MoneyType)t["Type"], t => (decimal)t["Money"]),
                    ds.Tables[2].Select("UserID = " + id).ToDictionary(t => (GameType)t["Type"], t => (decimal)t["Money"])
                    ));
            }
            return list;
        }

        /// <summary>
        /// 获取整个团队的报表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <returns></returns>
        public Dictionary<MoneyLog.MoneyType, decimal> GetTeamReport(int userId, DateTime startAt, DateTime endAt, bool isSelf = false)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_TeamReport",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", userId),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt),
                    NewParam("@IsSelf", isSelf));

                Dictionary<MoneyLog.MoneyType, decimal> list = new Dictionary<MoneyLog.MoneyType, decimal>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    MoneyLog.MoneyType type = (MoneyLog.MoneyType)dr["Type"];
                    decimal money = (decimal)dr["Money"];

                    list.Add(type, money);
                }
                return list;
            }
        }

        /// <summary>
        /// 获取团队多个用户的报表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <param name="isSelf"></param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<MoneyLog.MoneyType, decimal>> GetTeamReport(int[] userId, DateTime startAt, DateTime endAt, bool isSelf = false)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_TeamReport",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", string.Join(",", userId)),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt),
                    NewParam("@IsSelf", isSelf));

                Dictionary<int, Dictionary<MoneyLog.MoneyType, decimal>> dic = new Dictionary<int, Dictionary<MoneyLog.MoneyType, decimal>>();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    int user = (int)dr["UserID"];
                    if (!dic.ContainsKey(user)) dic.Add(user, new Dictionary<MoneyLog.MoneyType, decimal>());
                    MoneyLog.MoneyType type = (MoneyLog.MoneyType)dr["Type"];
                    decimal money = (decimal)dr["Money"];

                    dic[user].Add(type, money);
                }
                return dic;
            }
        }

        /// <summary>
        /// 获取团队的第三方游戏报表（按类型）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <returns></returns>
        public GameReport GetTeamGameReport(int userId, DateTime startAt, DateTime endAt)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_GameTeamReport",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", userId),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt));

                return new GameReport(userId, ds);
            }
        }

        /// <summary>
        /// 获取团队的第三方游戏报表（按游戏）
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <returns></returns>
        public Dictionary<GameType, decimal> GetTeamGameReportByType(int userId, DateTime startAt, DateTime endAt)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.StoredProcedure, "rpt_GameTeamReportByType",
                    NewParam("@SiteID", SiteInfo.ID),
                    NewParam("@UserID", userId),
                    NewParam("@StartAt", startAt),
                    NewParam("@EndAt", endAt));

                Dictionary<GameType, decimal> dic = new Dictionary<GameType, decimal>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    dic.Add((GameType)dr["Type"], (decimal)dr["Money"]);
                }
                return dic;
            }
        }

        /// <summary>
        /// 获取在指定日期内达到有效投注额的用户数量
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        public int GetTeamMemberCount(int teamId, DateTime startAt, DateTime endAt, decimal money)
        {
            int siteId = this.GetSiteID(teamId);
            MoneyLog.MoneyType type = MoneyLog.MoneyType.Bet;

            return BDC.UserDateMoney.Where(t => t.SiteID == siteId && t.Type == type && t.Date >= startAt && t.Date < endAt).GroupBy(t => t.UserID).Select(t => new
            {
                UserID = t.Key,
                Money = t.Sum(p => p.Money)
            }).Where(t => t.Money >= money && BDC.UserDepth.Where(p => p.SiteID == siteId && p.UserID == teamId).Select(p => p.ChildID).Contains(t.UserID)).Count();

        }


    }
}
