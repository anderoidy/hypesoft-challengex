using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore; 
using Hypesoft.Domain.Common;
using System.Data; 
using Hypesoft.Infrastructure.Repositories;

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

    public IRepository<TEntity> Repository<TEntity>() where TEntity : EntityBase 
        => new RepositoryBase<TEntity>(_context);

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

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void RejectChanges()
    {
        var changedEntries = _context.ChangeTracker
            .Entries()
            .Where(e => e.State != EntityState.Unchanged);

        foreach (var entry in changedEntries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                case EntityState.Deleted:
                    entry.Reload();
                    break;
            }
        }
    }

    public void DetachAllEntities()
    {
        var changedEntries = _context.ChangeTracker
            .Entries()
            .Where(e => e.State != EntityState.Detached)
            .ToList();

        foreach (var entry in changedEntries)
        {
            entry.State = EntityState.Detached;
        }
    }

    public async Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters)
        => await _context.Database.ExecuteSqlRawAsync(sql, parameters);

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => await _context.Database.CanConnectAsync(cancellationToken);

    public async Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => await _context.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);

    public async Task<bool> IsTagInUseAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        // Verifica se a tag está sendo usada em algum produto
        return await _context.Products
            .AnyAsync(p => p.ProductTags.Any(pt => pt.TagId == tagId), cancellationToken);
    }

    public async Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
        => await _context.Products.CountAsync(cancellationToken);

    public async Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default)
        => await _context.Categories.CountAsync(cancellationToken);

    public async Task<int> GetTotalTagCountAsync(CancellationToken cancellationToken = default)
        => await _context.Tags.CountAsync(cancellationToken);

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
        }
        await _context.DisposeAsync();
    }
}
