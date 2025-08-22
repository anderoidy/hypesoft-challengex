using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Ardalis.Specification;

namespace Hypesoft.Infrastructure.Persistence;

public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(
        IQueryable<T> inputQuery,
        ISpecification<T> specification,
        bool evaluateCriteriaOnly = false)
    {
        if (inputQuery == null) throw new ArgumentNullException(nameof(inputQuery));
        if (specification == null) throw new ArgumentNullException(nameof(specification));

        var query = inputQuery;

        // Apply the criteria/filter expression
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // Apply ordering if expressions are set
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // Apply the paging if enabled
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip)
                         .Take(specification.Take);
        }

        // Apply ordering by multiple columns if specified
        if (specification.OrderExpressions != null && specification.OrderExpressions.Any())
        {
            IOrderedQueryable<T>? orderedQuery = null;
            var isFirst = true;

            foreach (var orderExpression in specification.OrderExpressions)
            {
                if (isFirst)
                {
                    orderedQuery = orderExpression.IsDescending
                        ? query.OrderByDescending(orderExpression.KeySelector)
                        : query.OrderBy(orderExpression.KeySelector);
                    isFirst = false;
                }
                else if (orderedQuery != null)
                {
                    orderedQuery = orderExpression.IsDescending
                        ? ((IOrderedQueryable<T>)orderedQuery).ThenByDescending(orderExpression.KeySelector)
                        : ((IOrderedQueryable<T>)orderedQuery).ThenBy(orderExpression.KeySelector);
                }
            }

            if (orderedQuery != null)
            {
                query = orderedQuery;
            }
        }

        return query;
    }

    public static IQueryable<TResult> GetQuery<TResult>(
        IQueryable<T> inputQuery,
        ISpecification<T, TResult> specification)
    {
        if (inputQuery == null) throw new ArgumentNullException(nameof(inputQuery));
        if (specification == null) throw new ArgumentNullException(nameof(specification));

        // Apply the base query (filtering, ordering, paging)
        var query = GetQuery(inputQuery, specification);

        // Apply the selector to project to the result type
        return query.Select(specification.Selector);
    }
}
