using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Hypesoft.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Hypesoft.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="ModelBuilder"/> class.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Adds a query filter to automatically filter out soft-deleted entities.
        /// </summary>
        /// <param name="entityType">The entity type to add the filter to.</param>
        public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                var method = typeof(ModelBuilderExtensions)
                    .GetMethod(nameof(GetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    .MakeGenericMethod(entityType.ClrType);

                var filter = method.Invoke(null, Array.Empty<object>());

                if (filter != null)
                {
                    entityType.SetQueryFilter((LambdaExpression)filter);
                }
            }
        }

        private static LambdaExpression GetSoftDeleteFilter<TEntity>() where TEntity : class, ISoftDelete
        {
            Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
            return filter;
        }
    }
}
