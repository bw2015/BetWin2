using System;
using System.Linq.Expressions;

namespace SP.Studio.Data.Linq
{
    /// <summary>
    /// Expression表达式树扩展
    /// </summary>
    public static class ExpressionExtend
    {
        /// <summary>
        /// And 操作
        /// </summary>
        /// <typeparam name="TInfo">实体类</typeparam>
        /// <param name="left">左树</param>
        /// <param name="right">右树</param>
        public static Expression<Func<TInfo, bool>> AndAlso<TInfo>(this Expression<Func<TInfo, bool>> left, Expression<Func<TInfo, bool>> right)
            where TInfo : class
        {
            if (left == null) { return right; }

            ParameterExpression param = left.Parameters[0];
            if (ReferenceEquals(param, right.Parameters[0]))
            {
                return Expression.Lambda<Func<TInfo, bool>>(Expression.AndAlso(left.Body, right.Body), param);
            }
            
            return Expression.Lambda<Func<TInfo, bool>>(Expression.AndAlso(left.Body, Expression.Invoke(right, param)), param);

            
        }

        /// <summary>
        /// OR 操作
        /// </summary>
        /// <typeparam name="TInfo">实体类</typeparam>
        /// <param name="left">左树</param>
        /// <param name="right">右树</param>
        public static Expression<Func<TInfo, bool>> OrElse<TInfo>(this Expression<Func<TInfo, bool>> left, Expression<Func<TInfo, bool>> right)
            where TInfo : class
        {
            if (left == null) { return right; }

            ParameterExpression param = left.Parameters[0];
            if (ReferenceEquals(param, right.Parameters[0]))
            {
                return Expression.Lambda<Func<TInfo, bool>>(Expression.OrElse(left.Body, right.Body), param);
            }

            return Expression.Lambda<Func<TInfo, bool>>(Expression.OrElse(left.Body, Expression.Invoke(right, param)), param);
        }
    }
}
