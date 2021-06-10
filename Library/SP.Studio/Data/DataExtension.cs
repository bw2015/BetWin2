using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

using SP.Studio.Data.Mapping;
using SP.Studio.Array;
using SP.Studio.Configuration;
using SP.Studio.Core;


namespace SP.Studio.Data
{
    /// <summary>
    /// 数据操作的扩展方法
    /// </summary>
    public static class DataExtension
    {
        /// <summary>
        /// 约定。 参数名等于该值则表示是自增值
        /// </summary>
        internal const string FIELDSTEP = "FIELDSTEP";

        /// <summary>
        /// 注册一个数据库链接
        /// </summary>
        /// <param name="isOnlyOne">项目中是否只使用一个数据库</param>
        public static void RegisterContext(DbExecutor db, bool isOnlyOne = true)
        {
            string key = isOnlyOne ? Config.DATA_KEY : Config.DATA_KEY + db.CreateKey();
            if (HttpContext.Current == null)    //  非Web项目
            {
                if (!DbSetting.GetSetting().DbList.ContainsKey(key))
                    DbSetting.GetSetting().DbList.Add(key, db);
            }
            else if (!HttpContext.Current.Items.Contains(key))
            {
                HttpContext.Current.Items.Add(key, db);
            }
        }

        /// <summary>
        /// 从进程中获取数据操作对象
        /// 使用自动创建类型。 如果获取不到则创建一个
        /// </summary>
        /// <param name="throwOnError">如果没有找到内容是否抛出异常</param>
        public static DbExecutor GetDbExecutor(string key = null, bool throwOnError = true)
        {
            key = string.IsNullOrEmpty(key) ? Config.DATA_KEY : Config.DATA_KEY + key;
            DbExecutor db = HttpContext.Current != null ? (DbExecutor)HttpContext.Current.Items[key] :
                (DbSetting.GetSetting().DbList.ContainsKey(key) ? DbSetting.GetSetting().DbList[key] : null);
            if (db == null && throwOnError)
            {
                if (string.IsNullOrEmpty(DbSetting.GetSetting().DbConnection))
                {
                    throw new Exception(string.Format("在http进程：{0}中无法找到数据库操作对象。 原因：未配置数据库连接字符串", key));
                }
                else
                {
                    db = new DbExecutor(DbSetting.GetSetting().DbConnection, DbSetting.GetSetting().DbType, DataConnectionMode.Instance, IsolationLevel.Unspecified);
                    if (HttpContext.Current == null)
                        DbSetting.GetSetting().DbList.Add(key, db);
                    else
                        HttpContext.Current.Items.Add(key, db);
                    // throw new Exception(string.Format("可以找到数据库连接信息，但是找不到数据对象。可能的原因：\n1、未调用SP.Studio.Data.DataModule\n2、使用数据库操作的运行顺序在DataModule之前", key));             
                }
            }
            return db;
        }

        /// <summary>
        /// 获取系统进程中的所有数据库对象
        /// </summary>
        public static DbExecutor[] GetDbExecutors()
        {
            if (HttpContext.Current != null)
            {
                List<DbExecutor> list = new List<DbExecutor>();
                foreach (DictionaryEntry item in HttpContext.Current.Items)
                {
                    if (item.Key.ToString().StartsWith(Config.DATA_KEY))
                    {
                        ErrorLog.ErrorAgent.WriteLog("Data", string.Format("{0}  关闭数据进程:{1}", DateTime.Now, item.Key));
                        list.Add((DbExecutor)item.Value);
                    }
                }
                return list.ToArray();
            }
            return DbSetting.GetSetting().DbList.Values.ToArray();
        }

        /// <summary>
        /// 把自身插入数据库
        /// </summary>
        /// <param name="db">如果是多数据库则该参数不能省略</param>
        public static bool Add<T>(this T t, DbExecutor db = null) where T : class, new()
        {
            return t.Add(false, db);
        }

        /// <summary>
        /// 把自身插入数据库 可定义是否返回自动编号值
        /// </summary>
        /// <param name="isIdentity">如果有自动编号字段是否返回插入之后的值</param>
        public static bool Add<T>(this T t, bool isIdentity, DbExecutor db = null) where T : class, new()
        {
            if (db == null) db = GetDbExecutor();
            string name = GetTableName(t.GetType());
            IDbOperation operation = DbFactory.CreateOperation(db);
            List<string> list = DbSetting.GetSetting().GetColumns(operation, name);
            if (list == null) throw new Exception(string.Format("在对象:{0}插入数据时无法找到表或者视图:{1}，请确认配置是否正确或者要插入的对象是否是数据库映射对象。", t.GetType().FullName, name));
            List<DbParameter> paramList = new List<DbParameter>();
            PropertyInfo identityProperty = null;   // 拥有自动编号属性的值
            foreach (PropertyInfo property in t.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                string propertyName = null;
                foreach (object att in property.GetCustomAttributes(typeof(ColumnAttribute), true))
                {
                    ColumnAttribute column = (ColumnAttribute)att;
                    propertyName = string.IsNullOrEmpty(column.Name) ? property.Name : column.Name;
                    if (column.IsDbGenerated)
                    {
                        propertyName = null;
                        identityProperty = property;
                    }
                }
                if (list.Exists(str => str.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    object value = property.GetValue(t, null);
                    if (value != null)
                    {
                        if (value.GetType() == typeof(DateTime) && (DateTime)value == DateTime.MinValue) value = DateTime.Parse("1900-1-1");
                    }
                    paramList.Add(DbFactory.NewParam("@" + propertyName, value, db.databaseType));
                }
            }
            if (isIdentity)
            {
                object identity;
                operation.Insert(name, out identity, paramList.ToArray());
                if (identityProperty != null)
                    identityProperty.SetValue(t, Convert.ChangeType(identity, identityProperty.PropertyType), null);
                return true;
            }
            else
            {
                return operation.Insert(name, paramList.ToArray());
            }
        }

        /// <summary>
        /// 把自身更新入数据库 以主键为查找条件
        /// </summary>
        /// <param name="t">要更新入数据库的对象</param>
        /// <param name="db">如果是多数据库则该参数不能省略</param>
        /// <param name="funs">更新指定的字段 (此参数为空则更新整个实体类)</param>
        public static int Update<T>(this T t, DbExecutor db = null, params Expression<Func<T, object>>[] funs) where T : class, new()
        {
            return t.Update(db, null, funs);
        }

        /// <summary>
        /// 将自身更新写入数据库
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">要更新入数据库的对象</param>
        /// <param name="db">如果是多数据库则该参数不能省略。 单数据库可为null</param>
        /// <param name="conditions">要查询的条件。 如为null则使用主键作为查找条件</param>
        /// <param name="fields">要更新的字段 (此参数为空则更新整个实体类)</param>
        public static int Update<T>(this T t, DbExecutor db, List<Expression<Func<T, object>>> conditions, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            if (db == null) db = GetDbExecutor();
            if (conditions == null) conditions = new List<Expression<Func<T, object>>>();
            string name = GetTableName(typeof(T));
            IDbOperation operation = DbFactory.CreateOperation(db);
            List<string> columnList = DbSetting.GetSetting().GetColumns(operation, name);
            if (columnList == null) throw new Exception(string.Format("在对象:{0}插入数据时无法找到表:{1}，请确认配置是否正确或者要插入的对象是否是数据库映射对象。", t.GetType().FullName, name));

            List<DbParameter> fieldParamList = new List<DbParameter>();
            List<DbParameter> conditionParamList = new List<DbParameter>();

            if (fields.Length > 0)
                fieldParamList = fields.ToDbParameterList(t, db.databaseType);
            if (conditions.Count > 0) { conditionParamList = conditions.ToArray().ToDbParameterList(t, db.databaseType); }

            if (fields.Length == 0 || conditions.Count == 0)
            {
                foreach (PropertyInfo property in t.GetType().GetProperties())
                {
                    string propertyName = property.GetPropertyName();
                    if (string.IsNullOrEmpty(propertyName)) continue;
                    bool isPrimaryKey = property.IsPrimaryKey();
                    object value = property.GetValue(t, null).GetValue(property.PropertyType);
                    if (columnList.Exists(str => str.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        DbParameter param = DbFactory.NewParam("@" + propertyName, value, db.databaseType);
                        if (fields.Length == 0 && !isPrimaryKey) fieldParamList.Add(param);
                        if (conditions.Count == 0 && isPrimaryKey) conditionParamList.Add(param);
                    }
                }
            }

            string condition = conditionParamList.ToCondition();
            List<DbParameter> paramList = fieldParamList.Merge(param => param.ParameterName, conditionParamList);

            return operation.Update(name, condition, paramList.ToArray());
        }

        /// <summary>
        /// 添加或者累加更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="db"></param>
        /// <param name="funs">如果不指定则是全部更新</param>
        /// <returns></returns>
        public static int AddOrUpdate<T>(this T t, DbExecutor db, params Expression<Func<T, object>>[] fields) where T : class, new()
        {
            if (db == null) db = GetDbExecutor();
            List<DbParameter> fieldParamList = new List<DbParameter>();
            List<DbParameter> conditionParamList = new List<DbParameter>();
            string name = GetTableName(typeof(T));
            IDbOperation operation = DbFactory.CreateOperation(db);
            List<string> columnList = DbSetting.GetSetting().GetColumns(operation, name);

            if (fields.Length > 0) fieldParamList = fields.ToDbParameterList(t, db.databaseType);

            foreach (PropertyInfo property in t.GetType().GetProperties())
            {
                string propertyName = property.GetPropertyName();
                if (string.IsNullOrEmpty(propertyName)) continue;
                bool isPrimaryKey = property.IsPrimaryKey();
                object value = property.GetValue(t, null).GetValue(property.PropertyType);
                if (columnList.Exists(str => str.Equals(propertyName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    DbParameter param = DbFactory.NewParam("@" + propertyName, value, db.databaseType);
                    if (fields.Length == 0 && !isPrimaryKey) fieldParamList.Add(param);
                    if (isPrimaryKey) conditionParamList.Add(param);
                }
            }

            string condition = conditionParamList.ToCondition();
            List<DbParameter> paramList = fieldParamList.Merge(param => param.ParameterName, conditionParamList);

            return operation.AddOrUpdate(name, condition, paramList.ToArray());
        }


        /// <summary>
        /// 更新字段（默认数据库）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="t"></param>
        /// <param name="db"></param>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int UpdateField<T, TKey>(this T t, Expression<Func<T, TKey>> field, TKey value) where TKey : struct
        {
            return t.UpdateField(null, field, value);
        }

        /// <summary>
        /// 更新字段（自增）
        /// </summary>
        public static int UpdateField<T, TKey>(this T t, DbExecutor db, Expression<Func<T, TKey>> field, TKey value) where TKey : struct
        {
            if (db == null) db = GetDbExecutor();
            PropertyInfo propertyInfo = field.ToPropertyInfo();
            IDbOperation operation = DbFactory.CreateOperation(db);
            List<DbParameter> paramList = new Expression<Func<T, object>>[] { }.ToDbParameterList(t);
            string condition = paramList.ToCondition();
            paramList.Add(DbFactory.NewParam("@Value", value));
            return db.ExecuteNonQuery(CommandType.Text, string.Format("UPDATE {0} SET {1} = {1} + @Value WHERE {2}", typeof(T).GetTableName(),
                propertyInfo.GetColumnName(), condition), paramList.ToArray());
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <typeparam name="T">要删除的实体</typeparam>
        /// <param name="db">要操作的数据库对象 如果是单数据库请填null</param>
        /// <param name="funs">lambda表达式描述的删除条件</param>
        public static int Delete<T>(this T t, DbExecutor db = null, params Expression<Func<T, object>>[] funs) where T : class, new()
        {
            if (db == null) db = GetDbExecutor();
            string name = GetTableName(t.GetType());
            IDbOperation operation = DbFactory.CreateOperation(db);
            List<DbParameter> paramList = funs.ToDbParameterList(t, db.databaseType);
            string condition = paramList.ToCondition();
            string sql = string.Format("DELETE FROM {0} WHERE {1}", name, condition);
            return db.ExecuteNonQuery(CommandType.Text, sql, paramList.ToArray());
        }

        /// <summary>
        /// 删除记录 使用单数据库配置
        /// </summary>
        /// <param name="funs">要删除对象的条件。 不填写则表示使用主键进行删除</param>
        public static int Delete<T>(this T t, params Expression<Func<T, object>>[] funs) where T : class, new()
        {
            return Delete(t, null, funs);
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int Delete<TSource>(this TSource t, Expression<Func<TSource, bool>> predicate, DbExecutor db = null) where TSource : class, new()
        {
            if (db == null) db = GetDbExecutor();
            throw new Exception("暂未实现");
        }

        /// <summary>
        /// 获取单条记录
        /// </summary>
        public static T Info<T>(this T t, DbExecutor db, params Expression<Func<T, object>>[] funs) where T : class
        {
            if (db == null) db = GetDbExecutor();
            string name = typeof(T).GetTableName();
            List<string> list = GetColumnList(typeof(T), db);
            List<DbParameter> paramList = funs.ToDbParameterList(t, db.databaseType);
            string condition, sql;
            DataSet ds;
            if (paramList.Count == 0)
            {
                foreach (PropertyInfo property in typeof(T).GetProperties().ToList().FindAll(proper => proper.IsPrimaryKey()))
                {
                    string propertyName = GetPropertyName(property);
                    paramList.Add(DbFactory.NewParam("@" + propertyName, property.GetValue(t, null), db.databaseType));
                }
            }
            if (paramList.Count == 0) throw new Exception("没有指定查询条件而且实体类没有设定主键。");
            condition = paramList.ConvertAll(param => string.Format("[{0}] = {1}", param.ParameterName.Substring(1), param.ParameterName)).Join(" AND ");
            sql = string.Format("SELECT * FROM {0} WHERE {1}", name, condition);
            ds = db.GetDataSet(CommandType.Text, sql, paramList.ToArray());
            if (ds.Tables[0].Rows.Count == 0)
                t = default(T);
            else
                t = ds.Fill<T>();
            return t;
        }

        /// <summary>
        /// 获取单条数据 使用单数据库配置
        /// </summary>
        /// <param name="funs">查找条件</param>
        public static T Info<T>(this T t, params Expression<Func<T, object>>[] funs) where T : class
        {
            return t.Info(null, funs);
        }

        /// <summary>
        /// 检查数据库中是否存在值
        /// </summary>
        /// <param name="funs">条件</param>
        public static bool Exists<T>(this T t, DbExecutor db = null, params Expression<Func<T, object>>[] funs) where T : class
        {
            if (db == null) db = GetDbExecutor();
            string name = typeof(T).GetTableName();
            List<DbParameter> paramList = funs.ToDbParameterList(t, db.databaseType);
            string condition = paramList.ToCondition();
            return db.ExecuteScalar(CommandType.Text, string.Format("SELECT 0 FROM [{0}] WITH(nolock) {1} {2}", name, string.IsNullOrEmpty(condition) ? "" : "WHERE", condition), paramList.ToArray()) != null;
        }


        /// <summary>
        /// 把DataRow填充到实体类
        /// </summary>
        public static T Fill<T>(this DataRow dr, bool hasPropertyName)
        {
            switch (typeof(T).Name)
            {
                case "Int32":
                case "String":
                    return (T)dr[0];

            }
            T t = (T)Activator.CreateInstance(typeof(T));
            foreach (PropertyInfo property in typeof(T).GetProperties())
            {
                string name = GetPropertyName(property, hasPropertyName);
                if (property.CanWrite && dr.Table.Columns.Contains(name))
                {
                    try
                    {
                        property.SetValue(t, dr[name].GetValue(property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("属性名:{0} 类型:{1} \n 原始错误信息:{2}", property.Name, property.PropertyType, ex.Message));
                    }
                }
            }
            return t;
        }

        public static T Fill<T>(this DataRow dr)
        {
            return dr.Fill<T>(false);
        }

        /// <summary>
        /// 给实体类赋值
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="obj"></param>
        public static void Fill<T>(this DataRow dr, T obj) where T : class
        {
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                ColumnAttribute column = property.GetAttribute<ColumnAttribute>();
                if (column == null) continue;
                Object value = dr.GetValue(column.Name, Activator.CreateInstance(property.PropertyType));
                property.SetValue(obj, value);
            }
        }

        /// <summary>
        /// 把DataSet填充到实体类
        /// </summary>
        public static T Fill<T>(this DataSet ds, bool hasPropertyName)
        {
            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return default(T);
            return ds.Tables[0].Rows[0].Fill<T>(hasPropertyName);
        }

        public static T Fill<T>(this DataSet ds)
        {
            return ds.Fill<T>(false);
        }

        /// <summary>
        /// 填充read
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="read"></param>
        /// <returns></returns>
        public static T Fill<T>(this IDataReader read) where T : new()
        {
            T t = new T();
            for (int i = 0; i < read.FieldCount; i++)
            {
                string name = read.GetName(i);
                PropertyInfo property = typeof(T).GetProperty(name);
                if (property != null)
                {
                    property.SetValue(t, read[i]);
                }
            }
            return t;
        }

        /// <summary>
        /// 把DataTable转化成为列表
        /// </summary>
        public static List<T> ToList<T>(this DataTable dt, bool hasPropertyName = false)
        {
            List<T> list = new List<T>();
            foreach (DataRow dr in dt.Rows)
                list.Add(dr.Fill<T>(hasPropertyName));
            return list;
        }

        /// <summary>
        /// 把DataSet转化成为列表
        /// </summary>
        /// <param name="hasPropertyName">是否包含没有Column属性的字段</param>
        public static List<T> ToList<T>(this DataSet ds, bool hasPropertyName = false)
        {
            return ds.Tables[0].ToList<T>(hasPropertyName);
        }

        /// <summary>
        /// 获取一个数据行的数据，如果没有该列则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static T GetValue<T>(this DataRow dr, string columnName)
        {
            return dr.GetValue(columnName, default(T));
        }

        /// <summary>
        /// 获取一个数据行的数据，如果没有该列则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetValue<T>(this DataRow dr, string columnName, T defaultValue)
        {
            if (!dr.Table.Columns.Contains(columnName) || dr[columnName] == DBNull.Value) return defaultValue;
            return (T)dr[columnName];
        }

        /// <summary>
        /// 把匿名委托转化成为SQL参数
        /// </summary>
        /// <param name="funCount">本次需要转换的数量</param>
        public static DbParameter ToDbParamter<T>(this Expression<Func<T, object>> fun, T t, DatabaseType dbType = DatabaseType.SqlServer, int funCount = 0)
        {
            PropertyInfo property = fun.ToPropertyInfo();
            string propertyName = GetPropertyName(property);
            object value = property.GetValue(t, null);
            if (property.HasAttribute(typeof(DbGeneratedAttribute)) && (int)value == 0 && funCount == 1)
            {
                propertyName = string.Format("{0}:{1}", FIELDSTEP, propertyName);
                value = ((DbGeneratedAttribute)property.GetCustomAttributes(typeof(DbGeneratedAttribute), false)[0]).Step;
            }
            return DbFactory.NewParam("@" + propertyName, value, dbType);
        }

        /// <summary>
        /// 把匿名委托表达式转成属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fun">返回T某个属性的表达式 (如果是其他类型会引发错误)</param>
        /// <returns></returns>
        public static PropertyInfo ToPropertyInfo<T, TKey>(this Expression<Func<T, TKey>> fun)
        {
            PropertyInfo property = null;
            switch (fun.Body.NodeType)
            {
                case ExpressionType.Convert:
                    property = (PropertyInfo)((MemberExpression)((UnaryExpression)fun.Body).Operand).Member;
                    break;
                case ExpressionType.MemberAccess:
                    property = (PropertyInfo)((MemberExpression)fun.Body).Member;
                    break;
            }
            return property;
        }

        public static FieldInfo ToFieldInfo<T, TKey>(this Expression<Func<T, TKey>> fun)
        {
            FieldInfo field = null;
            switch (fun.Body.NodeType)
            {
                case ExpressionType.Convert:
                    field = (FieldInfo)((MemberExpression)((UnaryExpression)fun.Body).Operand).Member;
                    break;
                case ExpressionType.MemberAccess:
                    field = (FieldInfo)((MemberExpression)fun.Body).Member;
                    break;
            }
            return field;
        }


        /// <summary>
        /// 把匿名委托数组转化成为SQL参数组
        /// </summary>
        /// <param name="funs">如果没有选定则自动查找T的主键属性</param>
        public static List<DbParameter> ToDbParameterList<T>(this IEnumerable<Expression<Func<T, object>>> funs, T t, DatabaseType dbType = DatabaseType.SqlServer)
        {
            List<DbParameter> paramList = new List<DbParameter>();
            if (funs.Count() == 0)
            {
                foreach (PropertyInfo property in typeof(T).GetProperties().Where(p => p.IsPrimaryKey()))
                {
                    paramList.Add(DbFactory.NewParam("@" + GetPropertyName(property), property.GetValue(t, null), dbType));
                }
                return paramList;
            }
            return funs.Select(fun => fun.ToDbParamter(t, dbType, funs.Count())).ToList();
        }

        /// <summary>
        /// 获取对象的主键列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<DbParameter> GetPrimaryKeyDbParameterList<T>(this T t, DatabaseType dbType = DatabaseType.SqlServer) where T : class
        {
            foreach (PropertyInfo property in typeof(T).GetProperties().Where(p => p.IsPrimaryKey()))
            {
                yield return DbFactory.NewParam("@" + GetPropertyName(property), property.GetValue(t, null), dbType);
            }
        }

        /// <summary>
        /// 把参数列表转化成为条件参数
        /// </summary>
        public static string ToCondition(this IEnumerable<DbParameter> paramList)
        {
            return paramList.ToList().ConvertAll(t => string.Format("[{0}] = {1}", t.ParameterName.Substring(1), t.ParameterName)).Join(" AND ");
        }

        /// <summary>
        /// 从属性中获取映射到的数据库表名
        /// </summary>
        public static string GetTableName(this Type type)
        {
            TableAttribute table = type.GetAttribute<TableAttribute>();
            return table == null ? type.Name : table.Name;
        }



        /// <summary>
        /// 获取属性名称（数据库字段） 如为null则返回属性名
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal static string GetPropertyName(this PropertyInfo property, bool hasPropertyName = false)
        {
            return property.GetColumnName(hasPropertyName);
            //return propertyName == null ? property.Name : propertyName;
        }

        /// <summary>
        /// 获取属性对应的数据库字段名
        /// </summary>
        public static string GetColumnName(this PropertyInfo property, bool hasPropertyName = false)
        {
            string propertyName = hasPropertyName ? property.Name : string.Empty;    //? 为什么要用空@2013-5-17   因为实体类可能存在Column名字和其他的属性名字一样@05-18，增加了  hasPropertyName 参数来解决这个问题。 
            foreach (object att in property.GetCustomAttributes(typeof(ColumnAttribute), false))
            {
                propertyName = ((ColumnAttribute)att).Name;
            }
            return propertyName;
        }

        /// <summary>
        /// 获取属性设置的默认值 (DefaultValue 属性)
        /// </summary>
        internal static object GetDefaultValue(this PropertyInfo property)
        {
            object value = null;
            foreach (object att in property.GetCustomAttributes(false))
            {
                if (att is DefaultValueAttribute)
                    value = ((DefaultValueAttribute)att).Value;
            }
            return value;
        }

        /// <summary>
        /// 是否是主键
        /// </summary>
        internal static bool IsPrimaryKey(this PropertyInfo property)
        {
            string propertyName = property.Name;
            foreach (object att in property.GetCustomAttributes(typeof(ColumnAttribute), false))
            {
                return ((ColumnAttribute)att).IsPrimaryKey;
            }
            return false;
        }

        /// <summary>
        /// 获取实体类对应数据库的主键字段名
        /// </summary>
        /// <returns>如果有多个主键则用逗号隔开</returns>
        public static string GetPrimaryKeyName(this Type type)
        {
            return string.Join(",", type.GetProperties().Where(t => t.IsPrimaryKey()).ToList().ConvertAll(t => t.GetColumnName()));
        }

        /// <summary>
        /// 把数据行转成列表形式
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<DataRow> ToList(this DataRowCollection list)
        {
            foreach (DataRow dr in list)
                yield return dr;
        }


        #region ==============   私有方法   =================

        /// <summary>
        /// 获取对象所映射的数据库字段列表
        /// </summary>
        private static List<string> GetColumnList(Type type, DbExecutor db)
        {
            string name = GetTableName(type);
            IDbOperation operation = DbFactory.CreateOperation(db);
            List<string> list = DbSetting.GetSetting().GetColumns(operation, name);
            if (list == null) throw new Exception(string.Format("在对象:{0}插入数据时无法找到表:{1}，请确认配置是否正确或者要插入的对象是否是数据库映射对象。", type.FullName, name));
            return list;
        }

        #endregion
    }
}
