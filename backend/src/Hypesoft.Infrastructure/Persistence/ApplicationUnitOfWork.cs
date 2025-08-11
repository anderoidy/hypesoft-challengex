using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hypesoft.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IApplicationUnitOfWork"/> interface.
    /// </summary>
    public class ApplicationUnitOfWork : IApplicationUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ApplicationUnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
        }

        /// <inheritdoc />
        public IProductRepository Products => GetRepository<IProductRepository>();

        /// <inheritdoc />
        public ICategoryRepository Categories => GetRepository<ICategoryRepository>();

        /// <inheritdoc />
        public ITagRepository Tags => GetRepository<ITagRepository>();

        /// <inheritdoc />
        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseEntity
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<TEntity>(_context);
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            return _currentTransaction;
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction == null)
                {
                    throw new InvalidOperationException("No transaction is currently active.");
                }

                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.RollbackAsync(cancellationToken);
                }
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        /// <summary>
        /// Gets the repository for the specified repository interface type.
        /// </summary>
        /// <typeparam name="TRepository">The type of the repository interface.</typeparam>
        /// <returns>The repository instance.</returns>
        private TRepository GetRepository<TRepository>() where TRepository : class
        {
            var type = typeof(TRepository);

            if (!_repositories.ContainsKey(type))
            {
                // This is a simplified example. In a real application, you would use dependency injection
                // to resolve the repository implementations.
                if (type == typeof(IProductRepository))
                {
                    _repositories[type] = new ProductRepository(_context);
                }
                else if (type == typeof(ICategoryRepository))
                {
                    _repositories[type] = new CategoryRepository(_context);
                }
                else if (type == typeof(ITagRepository))
                {
                    _repositories[type] = new TagRepository(_context);
                }
                else
                {
                    throw new InvalidOperationException($"Repository of type {type.Name} is not supported.");
                }
            }

            return (TRepository)_repositories[type];
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _currentTransaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
