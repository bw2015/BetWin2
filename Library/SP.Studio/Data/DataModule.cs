using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Data.Linq;

using SP.Studio.ErrorLog;

namespace SP.Studio.Data
{
    /// <summary>
    /// 注册数据库连接
    /// Add To httpModules : <add name="DataContext" type="SP.Studio.Data.DataModule" />
    /// 在系统启动的时候一定要给 DbConnection，DbType 赋值
    /// </summary>
    public class DataModule : IHttpModule
    {

        public static string DbConnection
        {
            set
            {
                DbSetting.GetSetting().DbConnection = value;
            }
            private get
            {
                return DbSetting.GetSetting().DbConnection;
            }
        }

        public static DatabaseType DbType
        {
            set
            {
                DbSetting.GetSetting().DbType = value;
            }
        }


        public void Dispose()
        {

        }


        void DisposeConnection()
        {
            // 关闭DataContext对象
            foreach (object key in HttpContext.Current.Items.Keys)
            {
                if (key.GetType() == typeof(String))
                {
                    if (((string)key).StartsWith(DbSetting.KEY_DataContext))
                    {
                        DataContext dc = (DataContext)HttpContext.Current.Items[key];
                        if (dc != null)
                        {
                            dc.Dispose();
                        }
                    }
                }
            }


            foreach (DbExecutor db in DataExtension.GetDbExecutors())
            {
                db.DisposeConnection();
            }
        }

        public virtual void Init(HttpApplication context)
        {
            // context.BeginRequest += new EventHandler(context_BeginRequest);   
            // 取消自动创建数据库对象 原因：当使用通配符解释时候消耗了大量的数据库连接资源。 
            // 修改成为了获取db对象时候自动创建

            context.EndRequest += new EventHandler((sender, e) =>
            {
                this.DisposeConnection();
            });

            // context.Error += new EventHandler(context_Error);
            // 就算出错也会执行EndRequest事件
        }

        /*
        void context_BeginRequest(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(DbSetting.GetSetting().DbConnection)) return;
            DataExtension.RegisterContext(new DbExecutor(DbSetting.GetSetting().DbConnection, DbSetting.GetSetting().DbType, DataConnectionMode.Instance, IsolationLevel.Unspecified), true);
        }

        void context_Error(object sender, EventArgs e)
        {
            //ErrorAgent.WriteLog("Data", string.Format("{0} {1}  {2}", DateTime.Now, HttpContext.Current.Request.RawUrl, "context_Error  关闭数据库"));
            this.DisposeConnection();
        }
        */
    }
}
