using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using SP.Studio.Array;

namespace SP.Studio.Data
{
    /// <summary>
    /// MSSQL的数据操作实现类
    /// </summary>
    public sealed class SqlDbOperation : IDbOperation
    {
        public SqlDbOperation(DbExecutor db) : base(db) { }

        public override bool InsertIdentity(string tableName, params DbParameter[] parameters)
        {
            throw new NotImplementedException();
        }

        public override void Insert(string tableName, out object identity, params DbParameter[] parameters)
        {
            string sql = "";
            List<string> fields = new List<string>();
            List<string> values = new List<string>();
            foreach (DbParameter param in parameters)
            {
                fields.Add("[" + param.ParameterName.Substring(1) + "]");
                values.Add(param.ParameterName);
            }
            sql += string.Format("INSERT INTO {0}({1}) VALUES({2});select @@IDENTITY;", tableName, fields.Join(','), values.Join(','));
            identity = db.ExecuteScalar(CommandType.Text, sql, parameters);
        }

        public override DataSet GetList(int pageIndex, int pageSize, string tableName, string fields, string condition, string sort, out int recordCount, params DbParameter[] parameters)
        {
            using (DbExecutor DB = new DbExecutor(db.connectionString, db.databaseType, DataConnectionMode.Instance))
            {
                condition = base.Condition(condition);
                recordCount = (int)DB.ExecuteScalar(CommandType.Text, string.Format("SELECT COUNT(*) FROM {0} WHERE {1}", tableName, condition), parameters);
                if (string.IsNullOrEmpty(fields)) fields = "*";

                string sql = string.Format("WITH arg1 AS(	SELECT {3} , ROW_NUMBER() OVER(ORDER BY {0}) as __rows FROM {1} WHERE {2}) " +
                    "SELECT {3} FROM arg1 WHERE __rows BETWEEN {4} AND {5}",
                    sort, tableName, condition, fields, (pageIndex - 1) * pageSize + 1, pageIndex * pageSize);
                DataSet ds = DB.GetDataSet(CommandType.Text, sql, parameters);
                ds.Tables[0].TableName = tableName;
                return ds;
            }
        }

        public override DataSet GetList(string tableName, string fields = null, string condition = null, string sort = null, int top = 0, params DbParameter[] parameters)
        {
            condition = base.Condition(condition);
            string sql = string.Format("SELECT {0} {1} FROM {2} WHERE {3} {4}",
                top == 0 ? "" : "TOP " + top, string.IsNullOrEmpty(fields) ? "*" : fields,
                tableName, condition, string.IsNullOrEmpty(sort) ? "" : "ORDER BY " + sort);
            DataSet ds = db.GetDataSet(CommandType.Text, sql, parameters);
            ds.Tables[0].TableName = tableName;
            return ds;
        }
        
    }
}
