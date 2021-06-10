using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Reflection;

namespace SP.Studio.Data.Linq.Sqlite
{
    public class SqliteDataContext : DataContext
    {
        public SqliteDataContext(IDbConnection conn) : base(conn) { }

        public new Table<TEntity> GetTable<TEntity>() where TEntity : class
        {
            return (Table<TEntity>)this.GetTable(typeof(TEntity));
        }

        public new ITable GetTable(Type type)
        {
            var tableNew = Activator.CreateInstance(typeof(Table<>).MakeGenericType(type), BindingFlags.NonPublic | BindingFlags.Instance , null, new object[] { this } , 
                System.Globalization.CultureInfo.CurrentCulture) as ITable;

            return tableNew;

        }
    }
}
