using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web;


namespace SP.Studio.Data
{
    public abstract class DbAgentBase<T> : DbAgent where T : class, new()
    {
        public DbAgentBase(string connection) : base(connection, DatabaseType.SqlServer, DataConnectionMode.Instance) { }

        private static T _instance;
        /// <summary>
        /// 返回单例对象
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }

        

        /// <summary>
        /// 设定排序值（从大到小）
        /// </summary>
        protected virtual bool UpdateSort<TABLE>(int siteId, Expression<Func<TABLE, int>> pk, params int[] ids) where TABLE : class, new()
        {
            int count = 0;
            short sort = (short)ids.Length;
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                string sql = string.Format("UPDATE [{0}] SET Sort = @Sort WHERE SiteID = @SiteID AND [{1}] = @ID", typeof(TABLE).GetTableName(), pk.ToPropertyInfo().GetColumnName());
                foreach (int id in ids)
                {
                    if (db.ExecuteNonQuery(CommandType.Text, sql,
                         NewParam("@Sort", sort),
                         NewParam("@SiteID", siteId),
                         NewParam("@ID", id)) == 1) count++;
                    sort--;
                }
                db.Commit();
            }
            return count != 0;
        }


        /// <summary>
        /// 更新栏目的下属数量统计
        /// </summary>
        /// <typeparam name="TABLE"></typeparam>
        /// <param name="siteId"></param>
        /// <param name="pk">主键</param>
        /// <param name="field">统计字段</param>
        /// <param name="addId">要增加的主键ID</param>
        /// <param name="lessId">要减少的主键ID</param>
        /// <returns></returns>
        protected virtual bool UpdateCount<TABLE>(int siteId, Expression<Func<TABLE, int>> pk, Expression<Func<TABLE, int>> field, int addId = 0, int lessId = 0)
        {
            if (addId == lessId) return false;
            int count = 0;
            using (DbExecutor db = NewExecutor(IsolationLevel.ReadUncommitted))
            {
                string sql = string.Format("UPDATE [{0}] SET [{2}] = [{2}] + @Increment WHERE SiteID = @SiteID AND [{1}] = @ID AND [{2}] + @Increment >= 0", typeof(TABLE).GetTableName(), pk.ToPropertyInfo().GetColumnName(), field.ToPropertyInfo().GetColumnName());
                if (addId != 0)
                {
                    if (db.ExecuteNonQuery(CommandType.Text, sql,
                         NewParam("@SiteID", siteId),
                         NewParam("@Increment", 1),
                         NewParam("@ID", addId)) == 1) count++;
                }
                if (lessId != 0)
                {
                    if (db.ExecuteNonQuery(CommandType.Text, sql,
                         NewParam("@SiteID", siteId),
                         NewParam("@Increment", -1),
                         NewParam("@ID", lessId)) == 1) count++;
                }
                db.Commit();
            }
            return count > 0;
        }
    }
}
