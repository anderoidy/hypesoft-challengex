using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hypesoft.Infrastructure.Persistence;

public sealed class ApplicationUnitOfWork : IApplicationUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private ITagRepository? _tags;

    private IDbContextTransaction? _currentTransaction;

    public ApplicationUnitOfWork(ApplicationDbContext context,
                                 IProductRepository products,
                                 ICategoryRepository categories,
                                 ITagRepository tags)
    {
        _context = context;
        _products = products;
        _categories = categories;
        _tags = tags;
    }

    public IProductRepository Products => _products ?? throw new InvalidOperationException("Repository not resolved");
    public ICategoryRepository Categories => _categories ?? throw new InvalidOperationException("Repository not resolved");
    public ITagRepository Tags => _tags ?? throw new InvalidOperationException("Repository not resolved");

    public IRepository<TEntity> Repository<TEntity>() where TEntity : EntityBase => new RepositoryBase<TEntity>(_context);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);

    public void RejectChanges()
    {
        foreach (var entry in _context.ChangeTracker.Entries().Where(e => e.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged))
        {
            switch (entry.State)
            {
                case Microsoft.EntityFrameworkCore.EntityState.Modified:
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Added:
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    break;
                case Microsoft.EntityFrameworkCore.EntityState.Deleted:
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Unchanged;
                    break;
            }
        }
    }

    public void DetachAllEntities()
        => _context.ChangeTracker.Clear();

    public Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        => _context.Database.ExecuteSqlRawAsync(sql, parameters);

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => _context.Database.CanConnectAsync(cancellationToken);

    // Application-specific helpers
    public Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);

    public Task<bool> IsTagInUseAsync(Guid tagId, CancellationToken cancellationToken = default)
        => _context.Products.AnyAsync(p => p.ProductTags.Any(pt => pt.TagId == tagId), cancellationToken);

    public Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
        => _context.Products.CountAsync(cancellationToken);

    public Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default)
        => _context.Categories.CountAsync(cancellationToken);

    public Task<int> GetTotalTagCountAsync(CancellationToken cancellationToken = default)
        => _context.Tags.CountAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
    public ValueTask DisposeAsync() => _context.DisposeAsync();
}
