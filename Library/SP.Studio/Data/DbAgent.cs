using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Data.Common;

using SP.Studio.Core;

using System.ComponentModel;

namespace SP.Studio.Data
{
    public abstract class DbAgent : IDisposable
    {
        protected string ConnectionString { get; private set; }
        protected DatabaseType DataType { get; private set; }
        private DataConnectionMode ConnMode;

        private DbProviderFactory factory;

        protected DbAgent() { }

        protected DbAgent(string connectionString, DatabaseType dbType = DatabaseType.SqlServer, DataConnectionMode connMode = DataConnectionMode.None)
        {
            this.ConnectionString = connectionString;
            this.DataType = dbType;
            this.ConnMode = connMode;
            this.factory = DbProviderFactories.GetFactory(dbType.GetDescription());
        }

        protected DbExecutor NewExecutor(IsolationLevel tranLevel = IsolationLevel.Unspecified)
        {
            return DbFactory.CreateExecutor(this.ConnectionString, this.DataType, this.ConnMode, tranLevel);
        }

        /// <summary>
        /// 创建一个数据连接对象并且判断是否是新建对象
        /// </summary>
        /// <param name="db"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        protected DbExecutor NewExecutor(DbExecutor db, out bool isNew)
        {
            isNew = db == null;
            if (isNew) db = NewExecutor();
            return db;
        }

        /// <summary>
        /// 创建一个新的数据库操作对象
        /// 如果db为null则为当前单次操作。 操作完成之后就关闭数据库。 
        /// 如果需要多几次操作则需要指定db
        /// </summary>
        protected IDbOperation NewOperation(DbExecutor db = null)
        {
            if (db == null)
                db = DbFactory.CreateExecutor(this.ConnectionString, this.DataType);
            return DbFactory.CreateOperation(db);
        }


        protected DbParameter NewParam(string parameterName, object value)
        {
            return DbFactory.NewParam(parameterName, value, this.DataType);
        }

        protected DbParameter NewParam(string parameterName, object value, DbType dbType, int size, ParameterDirection direction)
        {
            return DbFactory.NewParam(parameterName, value, dbType, size, direction, this.DataType);
        }

        public virtual void Dispose()
        {

        }

        /// <summary>
        /// 获取一个数据库操作对象并且放入HttpContext进程中
        /// </summary>
        protected static DbExecutor GetDbExecutor()
        {
            return DataExtension.GetDbExecutor();
        }

        #region =========== 消息传递方法 ===========

        protected HttpContext _context;

        /// <summary>
        /// 当前web操作对象
        /// </summary>
        protected virtual HttpContext context
        {
            get
            {
                return HttpContext.Current ?? _context;
            }
            set
            {
                this._context = value;
            }
        }

        protected virtual string MESSAGE { get { return "MESSAGE"; } }

        /// <summary>
        /// 返回错误信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected bool FaildMessage(string msg, params object[] args)
        {
            this.Message(msg, args);
            return false;
        }

        private string _message = null;
        /// <summary>
        /// 写入或者获取需要传递的信息
        /// </summary>
        public virtual string Message(string msg = null, params object[] args)
        {
            if (!string.IsNullOrEmpty(msg) && args.Length > 0) msg = string.Format(msg, args);
            if (this.context == null)
            {
                _message += (string.IsNullOrEmpty(_message) ? "" : "\n\n") + msg;
                return _message;
            }
            string message = (string)this.context.Items[MESSAGE];
            if (!string.IsNullOrEmpty(msg))
            {
                if (string.IsNullOrEmpty(message))
                    message = msg;
                else
                    message += "\n\n" + msg;
                this.context.Items[MESSAGE] = message;
            }
            return message ?? string.Empty;
        }

        /// <summary>
        /// 清除保存的信息
        /// </summary>
        public virtual void MessageClean(string msg = null)
        {
            if (this.context == null)
            {
                _message = string.Empty;
            }
            else
            {
                this.context.Items.Remove(MESSAGE);
            }

            if (!string.IsNullOrEmpty(msg))
            {
                this.Message(msg);
            }
        }

        #endregion

    }

    #region ======== 公共枚举 ================

    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum DatabaseType : byte
    {
        /// <summary>
        /// SQL SERVER 2005 and high
        /// </summary>
        [Description("System.Data.SqlClient")]
        SqlServer,
        [Description("System.Data.Oledb")]
        Access,
        [Description("MySql.Data.MySqlClient")]
        MySql,
        [Description("System.Data.Sqlite")]
        SQLite,
        /// <summary>
        /// 在Mono下运行的Sqlite
        /// </summary>
        [Description("Mono.Data.Sqlite")]
        SQLiteMono
    }

    /// <summary>
    /// 保持数据连接的方式
    /// </summary>
    public enum DataConnectionMode : byte
    {
        /// <summary>
        /// 不使用数据持久化
        /// </summary>
        None,
        /// <summary>
        /// 实例。 以数据库的操作类为一个实例，操作类注销时关闭链接
        /// </summary>
        Instance,
        /// <summary>
        /// 访问过程。 以一次完整的访问过程为单位，需在golbal的Request_End事件中关闭连接。
        /// 此类型仅限于Web程序
        /// </summary>
        Context

    }

    #endregion
}
