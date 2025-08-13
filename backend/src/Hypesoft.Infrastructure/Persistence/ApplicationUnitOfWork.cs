using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Repositories;
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

        // Repository properties
        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ApplicationUnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new Dictionary<Type, object>();
            
            // Initialize repositories
            Products = new ProductRepository(context);
            Categories = new CategoryRepository(context);
        }

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

        #region IUnitOfWork Implementation

        /// <inheritdoc />
        public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            return GetRepository<TEntity>();
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        #endregion

        /// <inheritdoc />
        public async Task<IDisposable> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                return _currentTransaction;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _currentTransaction;
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                _currentTransaction?.Commit();
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
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
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

        /// <inheritdoc />
        public async Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            // Check if any product is using this category
            var isInUse = await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
                
            return isInUse;
        }

        /// <inheritdoc />
        public async Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories.CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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
