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
    /// Defines the interface(s) for generic repository for identity entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IIdentityRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Gets the <see cref="IUnitOfWork"/>.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }

        // Add common identity-specific methods here
        // For now, we'll just inherit the same methods as IRepository
        // but without the BaseEntity constraint
        
        /// <summary>
        /// Finds an entity with the given primary key values.
        /// </summary>
        /// <param name="id">The value of the primary key for the entity to be found.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the found entity or null.</returns>
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains all entities.</returns>
        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all entities based on a specification.
        /// </summary>
        /// <param name="spec">The specification to filter the entities.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains all entities that satisfy the specification.</returns>
        Task<IReadOnlyList<TEntity>> GetListBySpecAsync(ISpecification<TEntity> spec, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
        Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity.</returns>
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an entity by its primary key.
        /// </summary>
        /// <param name="id">The value of the primary key for the entity to be deleted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    }
}
