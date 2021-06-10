using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Linq;
using System.Text;
using SP.Studio.Array;

namespace SP.Studio.Data
{
    /// <summary>
    /// 常用数据操作方法的抽象类。 
    /// 此类中封装了通用的SQL语法。 数据库单独的操作由继承类实现
    /// </summary>
    public abstract class IDbOperation : IDisposable
    {
        protected DbExecutor db;

        public IDbOperation(DbExecutor db)
        {
            this.db = db;
        }

        /// <summary>
        /// 条件语句的过滤和拼接
        /// </summary>
        protected internal string Condition(string condition)
        {
            if (string.IsNullOrEmpty(condition)) return "1=1";
            if (condition.ToLower().StartsWith("where")) return condition.Substring("where".Length);
            return "1=1 AND " + condition;
        }

        /// <summary>
        /// 插入数据库
        /// </summary>
        /// <param name="tableName">数据库表名</param>
        /// <param name="values">插入的值 与数据库字段顺序一致</param>
        public virtual bool Insert(string tableName, params object[] values)
        {
            List<string> fieldList = new List<string>();
            List<DbParameter> paramList = new List<DbParameter>();
            for (int i = 0; i < values.Length; i++)
            {
                fieldList.Add("@p" + i);
                paramList.Add(db.NewParam("@p" + i, values[i]));
            }
            string sql = string.Format("INSERT INTO [{0}] VALUES({1})", tableName, fieldList.Join(','));
            return db.ExecuteNonQuery(CommandType.Text, sql,
                paramList.ToArray()) > 0;
        }

        /// <summary>
        /// 插入数据库
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="parameters">插入的参数化对象 ParameterName为字段名</param>
        public virtual bool Insert(string tableName, params DbParameter[] parameters)
        {
            string sql = "";
            List<string> fields = new List<string>();
            List<string> values = new List<string>();
            foreach (DbParameter param in parameters)
            {
                fields.Add("[" + param.ParameterName.Substring(1) + "]");
                values.Add(param.ParameterName);
            }
            sql += string.Format("INSERT INTO {0}({1}) VALUES({2})", tableName, fields.Join(','), values.Join(','));
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters) > 0;
        }

        /// <summary>
        /// 插入数据库 并且获取自动编号的返回值
        /// </summary>
        public abstract void Insert(string tableName, out object identity, params DbParameter[] parameters);

        /// <summary>
        /// 插入数据库 允许插入自动编号的字段
        /// </summary>
        /// <param name="tableName">数据表名</param>
        /// <param name="parameters">插入的参数化对象 ParameterName为字段名</param>
        public abstract bool InsertIdentity(string tableName, params DbParameter[] parameters);

        /// <summary>
        /// 修改内容
        /// 被修改的字段不能位于查询条件中
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="condition">修改条件</param>
        /// <param name="parameters">要修改的字段对象 ParameterName为字段名</param>
        /// <returns>返回受影响的总行数s</returns>
        public virtual int Update(string tableName, string condition, params DbParameter[] parameters)
        {
            List<DbParameter> paramList = parameters.ToList();
            foreach (DbParameter param in parameters)
            {
                if (condition.Contains(param.ParameterName + " ") || condition.EndsWith(param.ParameterName))
                    paramList.Remove(param);
            }
            if (!string.IsNullOrEmpty(condition)) condition = " WHERE " + condition;
            string fields = paramList.ConvertAll(t =>
                t.ParameterName.Substring(1).StartsWith(DataExtension.FIELDSTEP) ? string.Format("[{0}] = [{0}] + {1}", t.ParameterName.Substring(t.ParameterName.IndexOf(':') + 1), t.Value)
                :
                string.Format("[{0}] = {1}", t.ParameterName.Substring(1), t.ParameterName)).Join(',');
            string sql = string.Format("UPDATE {0} SET {1} {2}", tableName, fields, condition);

            return db.ExecuteNonQuery(CommandType.Text, sql, parameters.ToList().FindAll(t => !t.ParameterName.Contains(":")).ToArray());
        }

        public virtual int AddOrUpdate(string tableName, string condition, params DbParameter[] parameters)
        {
            List<DbParameter> paramList = parameters.ToList();
            foreach (DbParameter param in parameters)
            {
                if (condition.Contains(param.ParameterName + " ") || condition.EndsWith(param.ParameterName))
                    paramList.Remove(param);
            }

            List<string> fields = new List<string>();
            List<string> values = new List<string>();
            foreach (DbParameter param in parameters)
            {
                fields.Add("[" + param.ParameterName.Substring(1) + "]");
                values.Add(param.ParameterName);
            }

            string updateFields = string.Join(",", paramList.Select(t =>
               string.Format("[{0}] = [{0}] + {1}", t.ParameterName.Substring(1), t.ParameterName)));

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("IF NOT EXISTS(SELECT 0 FROM [{0}] WHERE {1}) BEGIN ", tableName, condition);
            sb.AppendFormat("INSERT INTO [{0}]({1}) VALUES({2}) ", tableName, string.Join(",", fields), string.Join(",", values));
            sb.Append("END ELSE BEGIN ");
            sb.AppendFormat("UPDATE [{0}] SET {1} WHERE {2}", tableName, updateFields, condition);
            sb.Append(" END");

            string sql = sb.ToString();
            return db.ExecuteNonQuery(CommandType.Text, sql, parameters.ToList().FindAll(t => !t.ParameterName.Contains(":")).ToArray());
        }

        /// <summary>
        /// 获取数据分页
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="tableName">要查询的表名</param>
        /// <param name="fields">要查询的字段。 多个字段使用,号隔开。 如果为空则是全部字段</param>
        /// <param name="condition">查询条件。 为空则表示全部记录</param>
        /// <param name="sort">排序条件</param>
        /// <param name="recordCount">符合条件的总记录数</param>
        /// <param name="parameters">查询参数列表</param>
        public abstract DataSet GetList(int pageIndex, int pageSize, string tableName, string fields, string condition, string sort, out int recordCount, params DbParameter[] parameters);

        /// <summary>
        /// 获取符合条件的数据列表
        /// </summary>
        /// <param name="tableName">要查询的表名</param>
        /// <param name="fields">要查询的字段。 多个字段使用,号隔开。 如果为空则是全部字段</param>
        /// <param name="condition">查询条件。 为空则表示全部记录</param>
        /// <param name="sort">排序条件。 为空表示使用默认排序</param>
        /// <param name="top">取前多少条。  为空表示所有符合条件的记录</param>
        /// <returns></returns>
        public abstract DataSet GetList(string tableName, string fields, string condition, string sort, int top, params DbParameter[] parameters);

        /// <summary>
        /// 得到表下面的字段名列表
        /// </summary>
        public virtual List<string> GetColumns(string tableName)
        {
            DataSet ds = db.GetDataSet(string.Format("SELECT * FROM {0} WHERE 1 = 2", tableName));
            List<string> list = new List<string>();
            foreach (DataColumn column in ds.Tables[0].Columns)
                list.Add(column.ColumnName);
            return list;
        }

        /// <summary>
        /// 检查是否有数据
        /// </summary>
        public virtual bool Exists(string tableName, string condition = null, params DbParameter[] parameters)
        {
            if (string.IsNullOrEmpty(condition))
                condition = parameters.ToList().ToCondition();
            if (!string.IsNullOrEmpty(condition)) condition += "WHERE " + condition;
            return db.ExecuteScalar(CommandType.Text, string.Format("SELECT 0 FROM {0} {1}", tableName, condition),
                parameters) != null;
        }

        /// <summary>
        /// 获取单条记录
        /// </summary>
        public virtual DataRow GetInfo(string tableName, string fields = null, string condition = null, string sort = null, params DbParameter[] parameters)
        {
            condition = this.Condition(condition);
            string sql = string.Format("SELECT {0} FROM {1} WHERE {2} {3}", string.IsNullOrEmpty(fields) ? "*" : fields,
                tableName, condition, string.IsNullOrEmpty(sort) ? "" : "ORDER BY " + sort);
            DataSet ds = db.GetDataSet(CommandType.Text, sql, parameters);
            if (ds.Tables[0].Rows.Count == 0) return null;
            return ds.Tables[0].Rows[0];
        }


        /// <summary>
        /// 同时注销
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }

        /// <summary>
        /// 根据当前的链接创建Key
        /// </summary>
        /// <returns></returns>
        internal string CreateKey()
        {
            return db.CreateKey();
        }



    }
}
