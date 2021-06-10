using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using SP.Studio.Data;

namespace UpdateBetWin
{
    public class OldAgent : DbAgent
    {
        private static string DbConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["Old"].ConnectionString;
            }
        }
        public OldAgent()
            : base(DbConnection, DatabaseType.SqlServer, DataConnectionMode.Instance)
        {

        }

        public DataSet GetUserList()
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.GetDataSet(CommandType.Text, "SELECT * FROM Users WHERE IsDeleted = 0 ORDER BY UserID ASC");
            }
        }

        public DataSet GetMoneyLog()
        {
            using (DbExecutor db = NewExecutor())
            {
                return db.GetDataSet(CommandType.Text, "SELECT * FROM usr_MoneyLog ORDER BY LogID ASC");
            }
        }
    }
}
