using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Linq.Provider;

namespace SP.Studio.Data.Linq.Sqlite
{
    public class SqliteProvider : System.Linq.IQueryProvider
    {
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            Type elementType = expression.Type;
            try
            {
                return (IQueryable<TElement>)Activator.CreateInstance(
                    typeof(SqliteQueryable<>).MakeGenericType(elementType),
                    new object[] { this, expression });
            }
            catch
            {
                throw new Exception();
            }
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
