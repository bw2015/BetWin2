using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace SP.Studio.Data.Linq.Sqlite
{
    public class SqliteQueryable<T> : IOrderedQueryable<T>, IQueryable<T>, IQueryProvider, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable, IListSource
    {

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        Type IQueryable.ElementType
        {
            get { throw new NotImplementedException(); }
        }

        Expression IQueryable.Expression
        {
            get { throw new NotImplementedException(); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { throw new NotImplementedException(); }
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            throw new NotImplementedException();
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }

        object IQueryProvider.Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        bool IListSource.ContainsListCollection
        {
            get { throw new NotImplementedException(); }
        }

        IList IListSource.GetList()
        {
            throw new NotImplementedException();
        }
    }
}
