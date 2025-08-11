using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;

namespace Hypesoft.Domain.Common.Interfaces
{
    /// <summary>
    /// Defines the interface(s) for generic repository.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// Gets the <see cref="IUnitOfWork"/>.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <returns>The <see cref="IQueryable{TEntity}"/>.</returns>
        IQueryable<TEntity> GetAll();

        /// <summary>
        /// Gets all entities that match the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="IQueryable{TEntity}"/>.</returns>
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the entity with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{TEntity}"/>.</returns>
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the first entity that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{TEntity}"/>.</returns>
        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Update(TEntity entity);

        /// <summary>
        /// Updates the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void UpdateRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Removes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        void Remove(TEntity entity);

        /// <summary>
        /// Removes the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        void RemoveRange(IEnumerable<TEntity> entities);

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{bool}"/>.</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{int}"/>.</returns>
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{int}"/>.</returns>
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of entities that match the specified specification.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{IReadOnlyList{TEntity}}"/>.</returns>
        Task<IReadOnlyList<TEntity>> ListAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a list of entities that match the specified specification.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{IReadOnlyList{TResult}}"/>.</returns>
        Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{TEntity}"/>.</returns>
        Task<TEntity?> FirstOrDefaultAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>The <see cref="Task{TResult}"/>.</returns>
        Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<TEntity, TResult> specification, CancellationToken cancellationToken = default);
    }
}
