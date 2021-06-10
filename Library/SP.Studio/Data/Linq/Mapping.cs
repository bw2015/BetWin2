using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.Linq.SqlClient;
using System.Globalization;

using System.Reflection;

namespace SP.Studio.Data.Linq
{
    public class Mapping : MappingSource
    {
        public Func<Type, string> MetaTableName;

        protected override MetaModel CreateModel(Type dataContextType)
        {
            return new Meta(this, dataContextType);
        }

        /// <summary>
        /// 完成实体类和数据库表、字段名称之间的转换
        /// </summary>
        private class Meta : MetaModel
        {
            private MetaModel source;
            private Mapping mappingSource;

            
            internal Meta(Mapping mappingSource, Type contextType)
            {
                this.mappingSource = mappingSource;
                source = typeof(DataContext).Assembly.CreateInstance("System.Data.Linq.Mapping.AttributedMetaModel", false,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null, new object[] { mappingSource, contextType }, CultureInfo.CurrentCulture, null)
                    as MetaModel;
            }

            /// <summary>
            /// 得到DataContext的类型
            /// </summary>
            public override Type ContextType
            {
                get
                {
                    return source.ContextType;
                }
            }

            /// <summary>
            /// 获取数据库的名字
            /// </summary>
            public override string DatabaseName
            {
                get { return source.DatabaseName; }
            }

            public override MetaFunction GetFunction(MethodInfo method)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<MetaFunction> GetFunctions()
            {
                throw new NotImplementedException();
            }

            public override MetaType GetMetaType(Type type)
            {
                return source.GetMetaType(type);
            }

            public override MetaTable GetTable(Type rowType)
            {
                if (mappingSource.MetaTableName == null) return source.GetTable(rowType);

                var typeName = "System.Data.Linq.Mapping.AttributedMetaTable";
                var metaTable = typeof(DataContext).Assembly.CreateInstance(typeName, false, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null,
                    new object[] { source, new TableAttribute { Name = mappingSource.MetaTableName(rowType) }, rowType }, CultureInfo.CurrentCulture, null) as MetaTable;
                return metaTable;
            }

            public override IEnumerable<MetaTable> GetTables()
            {
                throw new NotImplementedException();
            }

            public override MappingSource MappingSource
            {
                get { return this.mappingSource; }
            }

            public override Type ProviderType
            {
                get
                {
                    return typeof(Linq.Sqlite.SqliteProvider);
                }
            }
        }
    }
}
