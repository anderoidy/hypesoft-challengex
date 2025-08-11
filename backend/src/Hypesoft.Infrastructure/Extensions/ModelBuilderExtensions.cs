using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Hypesoft.Domain.Common.Interfaces;

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
        public static void AddSoftDeleteQueryFilter(this IMutableEntityType entityType)
        {
            var methodToCall = typeof(ModelBuilderExtensions)
                .GetMethod(nameof(GetSoftDeleteFilter), 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Static)
                ?.MakeGenericMethod(entityType.ClrType);

            var filter = methodToCall?.Invoke(null, new object[] { });

            if (filter != null)
            {
                entityType.SetQueryFilter((LambdaExpression)filter);
            }
        }

        private static LambdaExpression GetSoftDeleteFilter<TEntity>() where TEntity : class, ISoftDelete
        {
            System.Linq.Expressions.Expression<Func<TEntity, bool>> filter = x => !x.IsDeleted;
            return filter;
        }
    }
}
