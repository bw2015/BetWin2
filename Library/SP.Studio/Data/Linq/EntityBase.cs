using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Linq;

namespace SP.Studio.Data.Linq
{
    /// <summary>
    /// 实体类基类
    /// </summary>
    public class EntityBase<TEntity> where TEntity : class,new()
    {
        public static Table<TEntity> Table()
        {
            throw new NotImplementedException();
        }

        public EntityBase()
        {

        }

        public string ToControl(Expression<Func<TEntity, object>> expression)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 查询对象
    /// </summary>
    public sealed class TableQuery<TEntity> : IQueryable<TEntity>, IQueryProvider, IOrderedQueryable<TEntity> 
    {
        public TableQuery() { }

        public TableQuery(Expression expression)
        {
            this._expression = expression;
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Type ElementType
        {
            get { return Expression.Type; }
        }

        private Expression _expression;
        /// <summary>
        /// 第二步、第四步
        /// </summary>
        public Expression Expression
        {
            get { return _expression ?? Expression.Constant(this); }
        }

        /// <summary>
        /// 第一步
        /// </summary>
        public IQueryProvider Provider
        {
            get { return this; }
        }

        /// <summary>
        /// 第三步
        /// </summary>
        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return new TableQuery<TElement>(expression);
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

        public override string ToString()
        {
            if (_expression == null)
                return string.Format("Table:{0}", typeof(TEntity).Name);
            else
                return _expression.ToString();
        }
    }

}
