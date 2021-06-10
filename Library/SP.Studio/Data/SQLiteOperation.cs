using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SP.Studio.Data
{
    /// <summary>
    /// SQLite数据库的常用操作
    /// </summary>
    public class SQLiteOperation : IDbOperation
    {
        public SQLiteOperation(DbExecutor db) : base(db) { }

        public override DataSet GetList(int pageIndex, int pageSize, string tableName, string fields, string condition, string sort, out int recordCount, params DbParameter[] parameters)
        {
            if (db.connectionMode == DataConnectionMode.None)
            {
                db.Dispose();
                db = new DbExecutor(db.connectionString, db.databaseType, DataConnectionMode.Instance, IsolationLevel.Unspecified);
            }
            try
            {
                condition = base.Condition(condition);
                string sql = string.Format("SELECT {0} FROM {1} WHERE {2} {3} limit {4},{5}",
                   string.IsNullOrEmpty(fields) ? "*" : "", tableName, condition, string.IsNullOrEmpty(sort) ? "" : "ORDER BY " + sort, (pageIndex - 1) * pageSize, pageSize);
                DataSet ds = db.GetDataSet(CommandType.Text, sql, parameters);
                ds.Tables[0].TableName = tableName;

                sql = string.Format("SELECT COUNT(*) FROM {0} WHERE {1}", tableName, condition);
                int.TryParse(db.ExecuteScalar(CommandType.Text, sql, parameters).ToString(), out recordCount);
                return ds;
            }
            finally
            {
                db.Dispose();
            }
        }

        public override DataSet GetList(string tableName, string fields = null, string condition = null, string sort = null, int top = 0, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public override void Insert(string tableName, out object identity, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public override bool InsertIdentity(string tableName, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
