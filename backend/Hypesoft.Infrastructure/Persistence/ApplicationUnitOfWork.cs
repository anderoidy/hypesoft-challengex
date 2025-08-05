using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Application.Common.Interfaces;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hypesoft.Infrastructure.Persistence;

public class ApplicationUnitOfWork : IApplicationUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;
    private IDbContextTransaction _transaction;

    public ApplicationUnitOfWork(
        ApplicationDbContext context,
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        Products = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        Categories = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        Tags = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public ITagRepository Tags { get; }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            return;
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            _transaction?.Commit();
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsCategoryInUseAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken) ||
               await Categories.AnyAsync(c => c.ParentCategoryId == categoryId, cancellationToken);
    }

    public async Task<bool> IsTagInUseAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await Tags.GetProductCountByTagAsync(tagId, cancellationToken) > 0;
    }

    public async Task<int> GetTotalProductCountAsync(CancellationToken cancellationToken = default)
    {
        return await Products.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> GetTotalCategoryCountAsync(CancellationToken cancellationToken = default)
    {
        return await Categories.CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> GetTotalTagCountAsync(CancellationToken cancellationToken = default)
    {
        return await Tags.CountAsync(cancellationToken: cancellationToken);
    }

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
                _transaction?.Dispose();
                _context.Dispose();
            }

            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsync(true);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                }
                
                await _context.DisposeAsync();
            }

            _disposed = true;
        }
    }
}
