using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Common;
using SP.Studio.Array;
using System.Text.RegularExpressions;

namespace SP.Studio.Data
{
    /// <summary>
    /// Linq的扩展方法
    /// 此方法已过时 请使用 SP.Studio.Data.Linq.DataContenxtExtension
    /// </summary>
    [Obsolete("已经移至Linq命令空间下")]
    internal static class DataContextExtension
    {
        /// <summary>
        /// 把linq的查询改为实体类泛型List
        /// 作用：用于只需要返回一个表的部分字段且不想返回匿名的场景
        /// </summary>
        [Obsolete("已经移至Linq命令空间下")]
        private static List<T> Translate<T>(this DataContext dataContext, IQueryable query)
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
        [Obsolete("已经移至Linq命令空间下")]
        private static int Update<TSource>(this Table<TSource> table, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class
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
        [Obsolete("已经移至Linq命令空间下")]
        private static int Update<TSource>(this Table<TSource> table, DbExecutor db, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class
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
        /// 插入或替换
        /// 使用默认数据库链接
        /// </summary>
        private static int Replace<TSource>(this Table<TSource> table, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class, new()
        {
            return table.Replace(null, tsource, predicate, fields);
        }

        /// <summary>
        /// 插入或者替换
        /// 如果有符合条件的记录则执行Update，否则将执行Insert
        /// </summary>
        private static int Replace<TSource>(this Table<TSource> table, DbExecutor db, TSource tsource, Expression<Func<TSource, bool>> predicate, params Expression<Func<TSource, object>>[] fields) where TSource : class, new()
        {
            int row = table.Update(db, tsource, predicate, fields);
            if (row > 0) return row;

            return tsource.Add(db ?? DataExtension.GetDbExecutor()) ? 1 : 0;
        }

        /// <summary>
        /// 删除记录
        /// 使用默认数据库链接
        /// </summary>
        private static int Remove<TSource>(this Table<TSource> table, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            return table.Remove(table.Where(predicate));
        }

        /// <summary>
        /// 删除
        /// </summary>
        private static int Remove<TSource>(this Table<TSource> table, DbExecutor db, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
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
        private static int Remove<TSource>(this Table<TSource> table, IEnumerable<TSource> entities) where TSource : class
        {
            var dc = table.Context;
            int count = entities.Count();
            table.DeleteAllOnSubmit(entities);
            dc.SubmitChanges();
            return count;
        }

        /// <summary>
        /// 把linq中的查询条件翻译成为sql语句
        /// </summary>
        private static string ToCondition<TSource>(this Table<TSource> table, Expression<Func<TSource, bool>> predicate, out List<DbParameter> parameters) where TSource : class
        {
            var query = table.Where(predicate);
            IDbCommand command = table.Context.GetCommand(query);
            parameters = new List<DbParameter>();
            foreach (DbParameter param in command.Parameters)
                parameters.Add(DbFactory.NewParam(param.ParameterName, param.Value));
            var sql = command.CommandText.Replace("[t0]", typeof(TSource).GetTableName());
            return sql.Substring(sql.IndexOf("WHERE"));
        }


    }
}
