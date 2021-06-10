using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Web.Security;

using SP.Studio.Core;
namespace SP.Studio.Data
{
    internal class DbFactory
    {
        internal static DbExecutor CreateExecutor(string connectionString, DatabaseType dbType = DatabaseType.SqlServer, DataConnectionMode connMode = DataConnectionMode.None, IsolationLevel tranLevel = IsolationLevel.Unspecified)
        {
            return new DbExecutor(connectionString, dbType, connMode, tranLevel);
        }

        internal static IDbOperation CreateOperation(DbExecutor db)
        {
            IDbOperation operation = null;
            switch (db.databaseType)
            {
                case DatabaseType.SqlServer:
                    operation = new SqlDbOperation(db);
                    break;
                case DatabaseType.SQLite:
                case DatabaseType.SQLiteMono:
                    operation = new SQLiteOperation(db);
                    break;
                case DatabaseType.Access:
                    operation = new OledbOperation(db);
                    break;
            }
            return operation;
        }

        internal static DbParameter NewParam(string parameterName, object value, DatabaseType databaseType = DatabaseType.SqlServer)
        {
            DbType dbType;
            if (value == null) value = string.Empty;
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
                case "Object[]":
                    dbType = DbType.String;
                    value = value.ToJsonString();
                    break;
                default:
                    if (value is Enum) value = Convert.ChangeType(value, typeof(int));
                    dbType = DbType.String;
                    break;
            }
            return NewParam(parameterName, value, dbType, 0, ParameterDirection.Input, databaseType);
        }

        internal static DbParameter NewParam(string parameterName, object value, DbType dbType, int size, ParameterDirection direction, DatabaseType databaseType = DatabaseType.SqlServer)
        {
            if (!parameterName.StartsWith("@")) parameterName = string.Concat("@", parameterName);
            if (dbType == DbType.String) value = value.ToString();
            DbProviderFactory factory = DbProviderFactories.GetFactory(databaseType.GetDescription());
            DbParameter param = factory.CreateParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            param.DbType = dbType;
            param.Size = size;
            param.Direction = direction;
            return param;
        }
    }
}
