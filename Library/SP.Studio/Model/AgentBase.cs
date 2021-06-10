using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data.Linq;

using SP.Studio.Data;

namespace SP.Studio.Model
{
    public abstract class AgentBase<T> : DbAgent where T : class, new()
    {
        public AgentBase(string dbConnection, DatabaseType dbType = DatabaseType.SqlServer, DataConnectionMode mode = DataConnectionMode.Instance)
            : base(dbConnection, dbType, mode) { }

        private static T _instance;
        public static T Instance()
        {
            if (_instance == null) _instance = new T();
            return _instance;
        }

        public virtual T1 DC<T1>() where T1 : DataContext, new()
        {
            return DbSetting.GetSetting().CreateDataContext<T1>();
        }

        /// <summary>
        /// 写入或者获取需要传递的信息
        /// </summary>
        public override string Message(string msg = null, params object[] args)
        {
            if (HttpContext.Current == null) return null;
            string message = (string)HttpContext.Current.Items["MESSAGE"];
            if (!string.IsNullOrEmpty(msg))
            {
                msg = string.Format(msg, args);
                if (string.IsNullOrEmpty(message))
                    message = msg;
                else
                    message += @"\n\n" + msg;
                HttpContext.Current.Items["MESSAGE"] = message;
            }
            return message;
        }
    }
}
