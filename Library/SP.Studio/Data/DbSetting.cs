using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Web;

using System.Data.Linq;

namespace SP.Studio.Data
{

    /// <summary>
    /// 数据库参数配置/常量存储
    /// </summary>
    /// <remarks>
    /// 使用单例模式存储数据库的链接的相关配置。
    /// 目前实现的：1、获取数据库的表、列结构。 在进行Data to Entity 的时候进行实体类与数据库字段对应关系的判断。
    /// </remarks>
    public class DbSetting
    {
        /// <summary>
        /// Linq的DataContext所使用的名字。 保证每次http生命周期里面只有一个
        /// </summary>
        public const string KEY_DataContext = "DATACONTEXT";

        [ThreadStatic]
        private static Dictionary<string, DataContext> _createDataContext;

        /// <summary>
        /// 创建一个在http生命周期里面唯一的DataContext对象
        /// </summary>
        /// <typeparam name="T">继承自DataContext的对象类型</typeparam>
        /// <returns></returns>
        public T CreateDataContext<T>(params object[] args) where T : DataContext, new()
        {
            string fullName = string.Concat(typeof(T).FullName, "_", string.Join("_", args));

            T dataObj;

            if (HttpContext.Current == null)
            {
                if (_createDataContext == null) _createDataContext = new Dictionary<string, DataContext>();
                dataObj = args.Length == 0 ? new T() : (T)Activator.CreateInstance(typeof(T), args);
                if (!_createDataContext.ContainsKey(fullName)) _createDataContext.Add(fullName, dataObj);
                return (T)_createDataContext[fullName];
            }
            string key = KEY_DataContext + "_" + fullName;

            dataObj = (T)HttpContext.Current.Items[key];
            if (dataObj == null)
            {
                dataObj = args.Length == 0 ? new T() : (T)Activator.CreateInstance(typeof(T), args); ;
                HttpContext.Current.Items.Add(key, dataObj);
            }
            return dataObj;
        }

        private string _dbConnection;
        internal string DbConnection
        {
            get
            {
                return _dbConnection;
            }
            set
            {
                _dbConnection = value;
            }
        }

        public DatabaseType DbType { internal get; set; }

        /// <summary>
        /// 数据库表结构的缓存
        /// 结构: Key --> 数据库
        ///       Value --> Hashtable --> TableName,ColumnList
        /// </summary>
        private Hashtable TableInfo = new Hashtable();

        /// <summary>
        /// 得到表下面的字段列表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>如果不存在该表则返回null</returns>
        public List<string> GetColumns(IDbOperation operation, string tableName)
        {
            lock (this.GetType())
            {
                string key = operation.CreateKey();
                if (!TableInfo.ContainsKey(key)) TableInfo.Add(key, new Hashtable());
                Hashtable ht = (Hashtable)TableInfo[key];
                if (ht.ContainsKey(tableName))
                    return (List<string>)ht[tableName];
                ht.Add(tableName, operation.GetColumns(tableName));
                return (List<string>)ht[tableName];
            }
        }

        public static DbSetting GetSetting()
        {
            return Nested.instance;
        }

        /// <summary>
        /// 用于非Web项目的存储
        /// </summary>
        public Dictionary<string, DbConnection> ConnectionList = new Dictionary<string, DbConnection>();

        public Dictionary<string, DbExecutor> DbList = new Dictionary<string, DbExecutor>();

        class Nested
        {
            internal static readonly DbSetting instance = new DbSetting();
        }
    }

}
