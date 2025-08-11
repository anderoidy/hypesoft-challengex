using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Application.Common.Interfaces;

public interface IApplicationUnitOfWork : IDisposable, IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous transaction initialization. The task result contains the transaction.</returns>
    Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    void RollbackTransaction();

    /// <summary>
    /// Gets a repository for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity for which to get a repository.</typeparam>
    /// <returns>A repository for the specified entity type.</returns>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity;

    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ITagRepository Tags { get; }
    
    // Adicionar outros repositórios específicos do domínio aqui quando necessário
    
    // Métodos específicos de negócio que envolvem múltiplos repositórios
    Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> IsTagInUseAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetTotalTagCountAsync(CancellationToken cancellationToken = default);
}
