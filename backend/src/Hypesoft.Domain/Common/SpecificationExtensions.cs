using System;
using System.Linq.Expressions;

namespace Hypesoft.Domain.Common
{
    public static class SpecificationExtensions
    {
        /// <summary>
        /// Combina duas expressões usando AND lógico
        /// </summary>
        public static Expression<Func<T, bool>> And<T>(
            this Expression<Func<T, bool>> first,
            Expression<Func<T, bool>> second
        )
        {
            if (first == null)
                return second;
            if (second == null)
                return first;

            var param = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(first, param),
                Expression.Invoke(second, param)
            );

            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}
