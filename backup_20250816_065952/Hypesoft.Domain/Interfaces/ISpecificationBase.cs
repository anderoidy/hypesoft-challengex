using System.Collections.Generic;
using System.Linq.Expressions;
using Hypesoft.Domain.Interfaces;

namespace Hypesoft.Domain.Common.Interfaces
{
    /// <summary>
    /// Base interface for specifications with common properties
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface ISpecificationBase<T>
    {
        /// <summary>
        /// Gets the filter criteria.
        /// </summary>
        Expression<Func<T, bool>>? Criteria { get; }

        /// <summary>
        /// Gets the collection of navigation properties to be included.
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Gets the collection of order expressions.
        /// </summary>
        List<OrderExpression<T>> OrderExpressions { get; }

        /// <summary>
        /// Gets the number of elements to skip.
        /// </summary>
        int? Skip { get; }

        /// <summary>
        /// Gets the number of elements to take.
        /// </summary>
        int? Take { get; }
    }

    /// <summary>
    /// Represents an order expression for specifications
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class OrderExpression<T>
    {
        /// <summary>
        /// Gets the key selector for ordering.
        /// </summary>
        public Expression<Func<T, object>> KeySelector { get; set; }

        /// <summary>
        /// Gets the order type.
        /// </summary>
        public OrderTypeEnum OrderType { get; set; }
    }

    /// <summary>
    /// Enumeration for order types
    /// </summary>
    public enum OrderTypeEnum
    {
        OrderBy,
        OrderByDescending,
        ThenBy,
        ThenByDescending,
    }
}
