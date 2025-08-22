using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Interfaces;
using Hypesoft.Domain.Common.Interfaces; 


namespace Hypesoft.Domain.Interfaces
{
    /// <summary>
    /// Defines the interface(s) for generic repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity>
        where TEntity : BaseEntity
    {
        IUnitOfWork UnitOfWork { get; }

        IQueryable<TEntity> GetAll();

        IQueryable<TEntity> Find(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<TEntity?> FirstOrDefaultAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        Task<TResult?> FirstOrDefaultAsync<TResult>(
            ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        );

        Task<int> CountAsync<TResult>(
            ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        );

        Task<bool> AnyAsync<TResult>(
            ISpecification<TEntity, TResult> specification,
            CancellationToken cancellationToken = default
        );

        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task AddRangeAsync(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default
        );

        void Update(TEntity entity);

        void UpdateRange(IEnumerable<TEntity> entities);

        void Remove(TEntity entity);

        void RemoveRange(IEnumerable<TEntity> entities);

        Task<bool> AnyAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default
        );
    }
}
