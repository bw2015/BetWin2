using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using SP.Studio.Data;

namespace UpdateBetWin
{
    public class NewAgent : DbAgent
    {
        private static string DbConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["New"].ConnectionString;
            }
        }
        public NewAgent()
            : base(DbConnection, DatabaseType.SqlServer, DataConnectionMode.Instance)
        {

        }

        /// <summary>
        /// 导入一个用户
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public bool ImportUser(int siteId, DataRow dr)
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.ExecuteNonQuery(CommandType.Text, @"IF NOT EXISTS(SELECT 0 FROM Users WHERE SiteID = @SiteID AND UserName = @UserName) BEGIN
	INSERT INTO Users
		VALUES(@SiteID,@UserName,@NickName,@Password,@PayPassword,@CreateAt,@RegIP,@Money,0,0,0,@Type,@AgentID,@LoginAt,@LoginIP,@Rebate,@QQ,@Email,@Mobile,@GroupID,0,0,0,'',@AccountName,'',0,'1900-1-1',@IsTest)
END",
    NewParam("@SiteID", siteId),
    NewParam("@UserName", dr["UserName"]),
    NewParam("@NickName", dr["NickName"]),
    NewParam("@Password", dr["Password"]),
    NewParam("@PayPassword", dr["PayPassword"]),
    NewParam("@CreateAt", dr["CreateAt"]),
    NewParam("@RegIP", dr["RegIP"]),
    NewParam("@Money", dr["Money"]),
    NewParam("@Type", dr["Type"]),
    NewParam("@AgentID", dr["AgentID"]),
    NewParam("@LoginAt", dr["LoginAt"]),
    NewParam("@LoginIP", dr["LoginIP"]),
    NewParam("@Rebate", dr["Rebate"]),
    NewParam("@QQ", dr["QQ"]),
    NewParam("@Email", dr["Email"]),
    NewParam("@Mobile", dr["Mobile"]),
    NewParam("@GroupID", dr["GroupID"]),
    NewParam("@AccountName", dr["AccountName"]),
    NewParam("@IsTest", dr["IsTest"])
    ) > 0;
            }
        }

        public Dictionary<string, int> GetUserID(int siteId)
        {
            Dictionary<string, int> list = new Dictionary<string, int>();
            using (DbExecutor db = NewExecutor())
            {
                foreach (DataRow dr in db.GetDataSet(CommandType.Text, "SELECT UserID,UserName FROM Users WHERE SiteID = @SiteID",
                    NewParam("@SiteID", siteId)).Tables[0].Rows)
                {
                    list.Add((string)dr["UserName"], (int)dr["UserID"]);
                }
            }
            return list;
        }

        /// <summary>
        /// 导入资金流水数据
        /// </summary>
        public bool ImportUser(int siteId, int userId, DataRow dr)
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.ExecuteNonQuery(CommandType.Text, @"IF NOT EXISTS(SELECT 0 FROM usr_MoneyLog WHERE SiteID = @SiteID AND UserID = @UserID AND Type = @Type AND SourceID = @SourceID AND CreateAt = @CreateAt) BEGIN
	INSERT INTO usr_MoneyLog
		VALUES(@SiteID,@UserID,@Money,@Balance,@IP,@CreateAt,@Type,@SourceID,@LogDesc)
END",
      NewParam("@SiteID", siteId),
      NewParam("@UserID", userId),
      NewParam("@Money", dr["Money"]),
      NewParam("@Balance", dr["Balance"]),
      NewParam("@IP", dr["IP"]),
      NewParam("@CreateAt", dr["CreateAt"]),
      NewParam("@Type", dr["Type"]),
      NewParam("@SourceID", dr["SourceID"]),
      NewParam("@LogDesc", dr["LogDesc"])) > 0;
            }
        }

        /// <summary>
        /// 更新代理关系
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="user"></param>
        public void UpdateAgent(int siteId, Dictionary<int, int> user)
        {
            using (DbExecutor db = NewExecutor())
            {
                DataSet ds = db.GetDataSet(CommandType.Text, "SELECT UserID,AgentID FROM Users WHERE SiteID = @SiteID",
                    NewParam("@SiteID",siteId));
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (!user.ContainsKey((int)dr["AgentID"])) continue;

                    db.ExecuteNonQuery(CommandType.Text, "UPDATE Users SET AgentID = @AgentID WHERE UserID = @UserID",
                        NewParam("@AgentID", user[(int)dr["AgentID"]]),
                        NewParam("@UserID", dr["UserID"]));
                }

                db.ExecuteNonQuery(CommandType.StoredProcedure, "data_BuidUserDepth",
                    NewParam("@SiteID", siteId));
            }
        }
    }
}
