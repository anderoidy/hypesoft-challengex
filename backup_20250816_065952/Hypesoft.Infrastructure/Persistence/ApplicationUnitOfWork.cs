using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Repositories;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Persistence
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IApplicationUnitOfWork"/> interface.
    /// </summary>
    public class ApplicationUnitOfWork : IApplicationUnitOfWork, IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationUnitOfWork> _logger;
        private readonly Dictionary<Type, object> _repositories;
        private bool _disposed;
 
        // Repository properties
        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }
        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }

        public IMongoDatabase Database => _context.Database;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationUnitOfWork"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger.</param>
        public ApplicationUnitOfWork(ApplicationDbContext context, ILogger<ApplicationUnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repositories = new Dictionary<Type, object>();

            // Initialize repositories
            Products = new ProductRepository(this, _logger);
            Categories = new CategoryRepository(this, _logger);
            Users = new UserRepository(this, _logger);
            Roles = new RoleRepository(this, _logger);
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

        /// <inheritdoc />
        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // In MongoDB, changes are typically saved immediately, but we can ensure any pending operations are flushed
                if (_session != null)
                {
                    await _session.CommitTransactionAsync(cancellationToken);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to the database");
                if (_session != null)
                {
                    await _session.AbortTransactionAsync();
                }
                throw;
            }
        }

        /// <inheritdoc />
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_session == null)
            {
                _session = await _context.Database.Client.StartSessionAsync(cancellationToken: cancellationToken);
                _session.StartTransaction();
            }
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.CommitTransactionAsync(cancellationToken);
                await _session.DisposeAsync();
                _session = null;
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.AbortTransactionAsync(cancellationToken);
                await _session.DisposeAsync();
                _session = null;
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _context.Products.CountDocumentsAsync(
                    Builders<Product>.Filter.Eq(x => x.CategoryId, categoryId), 
                    cancellationToken: cancellationToken);
                
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category is in use: {CategoryId}", categoryId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _context.Products.CountDocumentsAsync(
                    Builders<Product>.Filter.Empty, 
                    cancellationToken: cancellationToken);
                
                return (int)count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total product count");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _context.Categories.CountDocumentsAsync(
                    Builders<Category>.Filter.Empty, 
                    cancellationToken: cancellationToken);
                
                return (int)count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total category count");
                throw;
            }
        }

        #region IDisposable

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
                    _session?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        ~ApplicationUnitOfWork()
        {
            Dispose(false);
        }

        #endregion
    }
}
