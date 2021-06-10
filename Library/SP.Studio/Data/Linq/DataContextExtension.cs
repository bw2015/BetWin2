using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Common;
using SP.Studio.Array;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

using SP.Studio.Data;

namespace SP.Studio.Data.Linq
{
    /// <summary>
    /// Linq的扩展方法
    /// </summary>
    public static class DataContextExtension
    {
        /// <summary>
        /// 把linq的查询改为实体类泛型List
        /// 作用：用于只需要返回一个表的部分字段且不想返回匿名的场景
        /// </summary>
        public static List<T> Translate<T>(this DataContext dataContext, IQueryable query) where T : class
        {
            DbCommand command = dataContext.GetCommand(query);
            if (dataContext.Connection.State == ConnectionState.Closed)
            {
                dataContext.Connection.Open();
            }

            using (DbDataReader reader = command.ExecuteReader())
            {
                return dataContext.Translate<T>(reader).ToList();
            }
        }

        /// <summary>
        /// 把sql使用DataContext对象转化成为实体类泛型列表
        /// </summary>
        public static List<T> Translate<T>(this DataContext dataContext, string sql, params DbParameter[] parameters)
        {

            if (dataContext.Connection.State == ConnectionState.Closed)
            {
                dataContext.Connection.Open();
            }

            DbCommand command = dataContext.Connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = sql;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
            using (DbDataReader reader = command.ExecuteReader())
            {
                return dataContext.Translate<T>(reader).ToList();
            }
        }

        /// <summary>
        /// 自定义复合查询条件更新数据
        /// 只支持SqlServer
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="funs">要更新的字段</param>
        /// <returns></returns>
        public static int Update<TSource>(this Table<TSource> table, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class
        {
            return table.Update(null, tsource, predicate, fields);
        }

        /// <summary>
        /// 自定义复合查询条件更新数据
        /// 只支持SqlServer
        /// </summary>
        /// <param name="db">传递进来的数据库执行对象</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="funs">要更新的字段</param>
        /// <returns></returns>
        public static int Update<TSource>(this Table<TSource> table, DbExecutor db, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class
        {
            List<DbParameter> parameters;
            string condition = table.ToCondition(predicate, out parameters);
            string sql = string.Format("UPDATE {0} SET ", typeof(TSource).GetTableName());
            List<DbParameter> fieldParameter = fields.ToDbParameterList(tsource, DatabaseType.SqlServer);
            sql += fieldParameter.ConvertAll(t => string.Format("{0} = {1} ", t.ParameterName.Substring(1), t.ParameterName)).Join(',');
            sql += condition;
            parameters.AddRange(fieldParameter);

            db = db ?? DataExtension.GetDbExecutor();
            return db.ExecuteNonQuery(CommandType.Text, sql,
               parameters.ToArray());
        }

        /// <summary>
        /// 用对象直接操作更新
        /// </summary>
        public static int Update<TSource>(this TSource tsource, DbExecutor db, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class
        {
            List<DbParameter> parameters;
            db = db ?? DataExtension.GetDbExecutor();
            Table<TSource> table = new DataContext(db.connectionString).GetTable<TSource>();
            string condition = table.ToCondition(predicate, out parameters);
            string sql = string.Format("UPDATE {0} SET ", typeof(TSource).GetTableName());
            List<DbParameter> fieldParameter = fields.ToDbParameterList(tsource, DatabaseType.SqlServer);
            sql += fieldParameter.ConvertAll(t => string.Format("{0} = {1} ", t.ParameterName.Substring(1), t.ParameterName)).Join(',');
            sql += condition;
            parameters.AddRange(fieldParameter);

            return db.ExecuteNonQuery(CommandType.Text, sql,
               parameters.ToArray());
        }

        /// <summary>
        /// 插入或替换
        /// 使用默认数据库链接
        /// </summary>
        public static int Replace<TSource>(this Table<TSource> table, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class, new()
        {
            return table.Replace(null, tsource, predicate, fields);
        }

        /// <summary>
        /// 插入或者替换
        /// 如果有符合条件的记录则执行Update，否则将执行Insert
        /// </summary>
        public static int Replace<TSource>(this Table<TSource> table, DbExecutor db, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class, new()
        {
            int row = table.Update(db, tsource, predicate, fields);
            if (row > 0) return row;

            return tsource.Add(db ?? DataExtension.GetDbExecutor()) ? 1 : 0;
        }

        /// <summary>
        /// 删除记录
        /// 使用默认数据库链接
        /// </summary>
        public static int Remove<TSource>(this Table<TSource> table, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return table.Remove(null, predicate);
        }

        /// <summary>
        /// 删除
        /// </summary>
        public static int Remove<TSource>(this Table<TSource> table, DbExecutor db, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            string sql = string.Format("DELETE FROM {0} ", typeof(TSource).GetTableName());
            List<DbParameter> parameters;
            sql += table.ToCondition(predicate, out parameters);
            return (db ?? DataExtension.GetDbExecutor()).ExecuteNonQuery(CommandType.Text, sql, parameters.ToArray());
        }

        /// <summary>
        /// 删除
        /// </summary>
        public static int Remove<TSource>(TSource tsource, DbExecutor db, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            Table<TSource> table = new DataContext(db.connectionString).GetTable<TSource>();
            string sql = string.Format("DELETE FROM {0} ", typeof(TSource).GetTableName());
            List<DbParameter> parameters;
            sql += table.ToCondition(predicate, out parameters);
            return (db ?? DataExtension.GetDbExecutor()).ExecuteNonQuery(CommandType.Text, sql, parameters.ToArray());
        }

        /// <summary>
        /// 删除 通过集合的方式
        /// </summary>
        /// <typeparam name="TSource">数据表的映射类</typeparam>
        /// <param name="table"></param>
        /// <param name="entities">查询集合</param>
        public static int Remove<TSource>(this Table<TSource> table, IEnumerable<TSource> entities) where TSource : class
        {
            var dc = table.Context;
            int count = entities.Count();
            table.DeleteAllOnSubmit(entities);
            dc.SubmitChanges();
            return count;
        }

        /// <summary>
        /// 把linq中的查询条件翻译成为sql语句
        /// 注意： 此处未做兼容MONO的设置。 -- By S.P 2012-8-13 20:34
        /// </summary>
        public static string ToCondition<TSource>(this Table<TSource> table, Expression<Func<TSource, bool>> predicate, out List<DbParameter> parameters) where TSource : class
        {
            var query = table.Where(predicate);
            return table.ToCondition(query, out parameters);
        }

        /// <summary>
        /// 把linq的查询翻译成为sql
        /// </summary>
        public static string ToCondition<TSource>(this Table<TSource> table, IQueryable<TSource> list, out List<DbParameter> parameters) where TSource : class
        {
            IDbCommand command = table.Context.GetCommand(list);
            parameters = new List<DbParameter>();
            foreach (DbParameter param in command.Parameters)
                parameters.Add(DbFactory.NewParam(param.ParameterName, param.Value));
            var sql = command.CommandText.Replace("[t0]", typeof(TSource).GetTableName());
            return sql.Substring(sql.IndexOf("WHERE"));
        }



        /// <summary>
        /// 执行扩展的linq查询方法
        /// </summary>
        /// <typeparam name="T">一定要有 IDataReader 构造</typeparam>
        /// <param name="data">linq数据库操作对象</param>
        /// <param name="list"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> WITH<T, TSource>(this IQueryable<T> list, Table<TSource> table, LockType type = LockType.READPAST, DbExecutor db = null) where TSource : class, new()
        {
            IDbCommand command = table.Context.GetCommand(list);
            Regex regex = new Regex(@"\[(?<Name>[^\]]+)\] AS \[t0\]");
            string sql = command.CommandText;
            if (!regex.IsMatch(sql))
            {
                throw new Exception("不支持的查询类型\n" + sql);
            }
            sql = regex.Replace(sql, "[${Name}] AS [t0] WITH(" + type + ")");

            db = db ?? DataExtension.GetDbExecutor();

            List<DbParameter> parameters = new List<DbParameter>();
            foreach (DbParameter param in command.Parameters)
                parameters.Add(DbFactory.NewParam(param.ParameterName, param.Value));
    
            IDataReader read = db.ReadData(command.CommandType, sql,
                parameters.ToArray());
            while (read.Read())
            {
                yield return read.Fill<TSource>();
            }
        }
    }
}
