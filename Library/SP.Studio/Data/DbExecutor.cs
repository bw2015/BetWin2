using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Web;
using System.Web.Security;

using SP.Studio.Core;

namespace SP.Studio.Data
{
    public class DbExecutor : IDisposable
    {
        private readonly DbProviderFactory factory;
        internal readonly string connectionString;
        internal readonly DatabaseType databaseType;
        internal readonly DataConnectionMode connectionMode;
        internal readonly string ContextKey;

        private DbConnection conn;
        private DbCommand comm;

        private List<Action> callback;

        /// <summary>
        /// commit之後要執行的回調方法
        /// </summary>
        /// <param name="action"></param>
        public void AddCallback(Action action)
        {
            if (this.callback == null) this.callback = new List<Action>();
            this.callback.Add(action);
        }

        /// <summary>
        /// 创建一个数据库连接
        /// </summary>
        /// <returns></returns>
        private DbConnection CreateConnection()
        {
            switch (this.connectionMode)
            {
                case DataConnectionMode.Instance:
                    if (conn != null) return conn;
                    break;
                case DataConnectionMode.Context:
                    if (conn != null) return conn;
                    if (HttpContext.Current != null)
                        conn = (DbConnection)HttpContext.Current.Items[this.ContextKey];
                    else
                        conn = DbSetting.GetSetting().ConnectionList[this.ContextKey];

                    if (conn != null) return conn;
                    break;
            }

            conn = factory.CreateConnection();
            //ErrorLog.ErrorAgent.WriteLog("Data", string.Format("{0}\t创建数据库链接 conn = null ? {1}", DateTime.Now, (conn == null)));
            conn.ConnectionString = connectionString;
            conn.Open();
            if (connectionMode == DataConnectionMode.Context)
            {
                if (HttpContext.Current == null)
                    DbSetting.GetSetting().ConnectionList.Add(this.ContextKey, conn);
                else
                    HttpContext.Current.Items.Add(this.ContextKey, conn);
            }
            return conn;
        }

        /// <summary>
        /// 关闭数据库链接
        /// </summary>
        /// <param name="isDispose">是否是实例注销</param>
        private void CloseConnection(bool isDispose = false)
        {
            if (conn == null || conn.State != ConnectionState.Open) return;

            if (this.connectionMode == DataConnectionMode.None || (this.connectionMode == DataConnectionMode.Instance && isDispose))
            {
                this.DisposeConnection();
            }
        }

        /// <summary>
        /// 强制关闭数据库链接
        /// </summary>
        public void DisposeConnection()
        {
            if (tran != null) tran.Dispose();
            if (comm != null) comm.Dispose();
            if (conn != null)
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                conn.Dispose();
            }
        }

        /// <summary>
        /// 填充参数以及事务
        /// </summary>
        private void FillParameterAndTransaction(params DbParameter[] parameters)
        {
            foreach (DbParameter param in parameters)
            {
                comm.Parameters.Add(param);
            }
            if (tran != null) comm.Transaction = tran;
        }

        /// <summary>
        /// 事务
        /// </summary>
        private DbTransaction tran;

        /// <summary>
        /// 是否开启事务
        /// </summary>
        public bool IsTransaction { get; set; }

        /// <summary>
        /// 构造函数 赋值
        /// </summary>
        /// <param name="connectionString">链接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="isTran">是否启用事务</param>
        public DbExecutor(string connectionString, DatabaseType dbType = DatabaseType.SqlServer, DataConnectionMode connMode = DataConnectionMode.None, IsolationLevel tranLevel = IsolationLevel.Unspecified)
        {
            factory = DbProviderFactories.GetFactory(dbType.GetDescription());
            this.databaseType = dbType;
            this.connectionString = connectionString;
            this.connectionMode = connMode;
            if (connMode == DataConnectionMode.Context) this.ContextKey = this.CreateKey();
            this.conn = this.CreateConnection();
            if (tranLevel != IsolationLevel.Unspecified) tran = conn.BeginTransaction(tranLevel);
        }

        /// <summary>
        /// 根据当前的链接字符串生成KEY
        /// </summary>
        /// <returns></returns>
        internal string CreateKey()
        {
            return SP.Studio.Security.MD5.toMD5(connectionString).Substring(8, 8);
        }


        public DbParameter NewParam(string parameterName, object value)
        {
            DbType dbType;
            switch (value.GetType().Name)
            {
                case "DateTime":
                    dbType = DbType.DateTime;
                    break;
                case "Boolean":
                    dbType = DbType.Boolean;
                    break;
                case "Int32":
                    dbType = DbType.Int32;
                    break;
                case "Int16":
                    dbType = DbType.Int16;
                    break;
                case "Decimal":
                    dbType = DbType.Decimal;
                    break;
                case "Byte":
                    dbType = DbType.Byte;
                    break;
                case "Guid":
                    dbType = DbType.Guid;
                    break;
                default:
                    dbType = DbType.String;
                    break;
            }
            return this.NewParam(parameterName, value, dbType, 0, ParameterDirection.Input);
        }

        public DbParameter NewParam(string parameterName, object value, DbType dbType, int size, ParameterDirection direction)
        {
            DbParameter param = factory.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            param.DbType = dbType;
            param.Size = size;
            param.Direction = direction;
            return param;
        }


        public IDataReader ReadData(string commandText)
        {
            return this.ReadData(CommandType.Text, commandText);
        }

        public IDataReader ReadData(string procedureName, DbParameter[] parameters)
        {
            return this.ReadData(CommandType.Text, procedureName, parameters);
        }

        public IDataReader ReadData(CommandType cmdType, string cmdText, params DbParameter[] parameters)
        {
            comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.CommandType = cmdType;
            comm.CommandText = cmdText;
            this.FillParameterAndTransaction(parameters);
            try
            {
                return comm.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw new Exception(this.ThrowException(ex, cmdText, parameters));
            }
            finally
            {
                comm.Parameters.Clear();
                //comm.Dispose();
                //this.CloseConnection();
            }
        }



        public object ExecuteScalar(string commandText)
        {
            return this.ExecuteScalar(CommandType.Text, commandText);
        }

        public object ExecuteScalar(string procedureName, DbParameter[] parameters)
        {
            return this.ExecuteScalar(CommandType.StoredProcedure, procedureName, parameters);
        }

        public object ExecuteScalar(CommandType cmdType, string cmdText, params DbParameter[] parameters)
        {
            comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.CommandType = cmdType;
            comm.CommandText = cmdText;
            this.FillParameterAndTransaction(parameters);
            try
            {
                return comm.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception(this.ThrowException(ex, cmdText, parameters));
            }
            finally
            {
                comm.Parameters.Clear();
                this.CloseConnection();
            }
        }



        public int ExecuteNonQuery(string commandText)
        {
            return this.ExecuteNonQuery(CommandType.Text, commandText);
        }

        public int ExecuteNonQuery(string procedureName, DbParameter[] parameters)
        {
            return this.ExecuteNonQuery(CommandType.StoredProcedure, procedureName, parameters);
        }

        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params DbParameter[] parameters)
        {
            comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.CommandType = cmdType;
            comm.CommandText = cmdText;
            this.FillParameterAndTransaction(parameters);
            try
            {
                return comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(this.ThrowException(ex, cmdText, parameters));
            }
            finally
            {
                comm.Parameters.Clear();
                this.CloseConnection();
            }
        }



        public DataSet GetDataSet(string commandText)
        {
            return this.GetDataSet(CommandType.Text, commandText);
        }

        public DataSet GetDataSet(string procedureName, params DbParameter[] parameter)
        {
            return this.GetDataSet(CommandType.StoredProcedure, procedureName, parameter);
        }

        public DataSet GetDataSet(CommandType cmdType, string cmdText, params DbParameter[] parameter)
        {
            comm = factory.CreateCommand();
            comm.Connection = conn;
            comm.CommandType = cmdType;
            comm.CommandText = cmdText;
            this.FillParameterAndTransaction(parameter);
            DbDataAdapter adapter = factory.CreateDataAdapter();
            try
            {
                adapter.SelectCommand = comm;
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(this.ThrowException(ex, cmdText, parameter));
            }
            finally
            {
                comm.Parameters.Clear();
                this.CloseConnection();
            }
        }


        /// <summary>
        /// 提交事务
        /// 如果未开启事务则会引发异常
        /// </summary>
        public void Commit()
        {
            if (tran == null)
                throw new Exception("未开启事务");
            if (this.connectionMode == DataConnectionMode.None)
                throw new Exception("如果要使用事务则必须使用数据的长连接");
            tran.Commit();

            if (callback != null)
            {
                foreach (Action action in this.callback)
                {
                    action.Invoke();
                }
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            if (tran == null)
                throw new Exception("未开启事务");
            if (this.connectionMode == DataConnectionMode.None)
                throw new Exception("如果要使用事务则必须使用数据的长连接");
            tran.Rollback();
        }

        public void Dispose()
        {
            this.CloseConnection(true);
        }

        /// <summary>
        /// 抛出异常
        /// </summary>
        private string ThrowException(Exception ex, string cmdText, params DbParameter[] parameters)
        {
            StringBuilder sb = new StringBuilder(ex.Message);
            sb.AppendLine(cmdText);
            foreach (DbParameter param in parameters)
            {
                sb.AppendFormat("{0} : {1} \n", param.ParameterName, param.Value);
            }
            return sb.ToString();
        }
    }
}
