using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hypesoft.Infrastructure.Persistence.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<int> GetStockQuantityAsync(Guid productId, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(Guid productId, int quantityChange, string? userId = null, CancellationToken cancellationToken = default);
    Task PublishProductAsync(Guid productId, string? userId = null, CancellationToken cancellationToken = default);
    Task UnpublishProductAsync(Guid productId, string? userId = null, CancellationToken cancellationToken = default);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.CategoryId == categoryId && p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.IsFeatured && p.IsPublished)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetPagedAsync(pageNumber, pageSize, null, q => q.OrderByDescending(p => p.CreatedAt), cancellationToken: cancellationToken);
        }

        var searchTermLower = searchTerm.Trim().ToLower();
        
        Expression<Func<Product, bool>> searchPredicate = p => 
            p.Name.ToLower().Contains(searchTermLower) ||
            (p.Description != null && p.Description.ToLower().Contains(searchTermLower)) ||
            (p.Sku != null && p.Sku.ToLower().Contains(searchTermLower)) ||
            (p.Barcode != null && p.Barcode.ToLower().Contains(searchTermLower));
            
        return await GetPagedAsync(
            pageNumber, 
            pageSize, 
            searchPredicate, 
            q => q.OrderByDescending(p => p.CreatedAt), 
            cancellationToken: cancellationToken);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;
            
        var query = _dbSet.Where(p => p.Sku != null && p.Sku.ToLower() == sku.Trim().ToLower());
        
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;
            
        var query = _dbSet.Where(p => p.Barcode != null && p.Barcode.ToLower() == barcode.Trim().ToLower());
        
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<int> GetStockQuantityAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _dbSet
            .Where(p => p.Id == productId)
            .Select(p => new { p.StockQuantity })
            .FirstOrDefaultAsync(cancellationToken);
            
        return product?.StockQuantity ?? 0;
    }

    public async Task UpdateStockAsync(Guid productId, int quantityChange, string? userId = null, CancellationToken cancellationToken = default)
    {
        var product = await _dbSet.FindAsync(new object[] { productId }, cancellationToken);
        
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {productId} not found.");
            
        product.StockQuantity += quantityChange;
        
        if (userId != null)
        {
            product.UpdatedAt = DateTime.UtcNow;
            product.UpdatedBy = userId;
        }
        
        _dbContext.Entry(product).Property(x => x.StockQuantity).IsModified = true;
        _dbContext.Entry(product).Property(x => x.UpdatedAt).IsModified = true;
        _dbContext.Entry(product).Property(x => x.UpdatedBy).IsModified = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishProductAsync(Guid productId, string? userId = null, CancellationToken cancellationToken = default)
    {
        var product = await _dbSet.FindAsync(new object[] { productId }, cancellationToken);
        
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {productId} not found.");
            
        product.Publish(userId);
        
        _dbContext.Entry(product).Property(x => x.IsPublished).IsModified = true;
        _dbContext.Entry(product).Property(x => x.PublishedAt).IsModified = true;
        _dbContext.Entry(product).Property(x => x.UpdatedAt).IsModified = true;
        _dbContext.Entry(product).Property(x => x.UpdatedBy).IsModified = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishProductAsync(Guid productId, string? userId = null, CancellationToken cancellationToken = default)
    {
        var product = await _dbSet.FindAsync(new object[] { productId }, cancellationToken);
        
        if (product == null)
            throw new KeyNotFoundException($"Product with ID {productId} not found.");
            
        product.Unpublish(userId);
        
        _dbContext.Entry(product).Property(x => x.IsPublished).IsModified = true;
        _dbContext.Entry(product).Property(x => x.PublishedAt).IsModified = true;
        _dbContext.Entry(product).Property(x => x.UpdatedAt).IsModified = true;
        _dbContext.Entry(product).Property(x => x.UpdatedBy).IsModified = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public override async Task<TEntity?> GetByIdAsync(Guid id, string includeProperties, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _dbSet;
        
        foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            query = query.Include(includeProperty.Trim());
        }
        
        return await query
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken) as TEntity;
    }

    public override async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<Product, object>>[] includeProperties)
    {
        IQueryable<Product> query = _dbSet;
        
        if (includeProperties != null)
        {
            query = includeProperties.Aggregate(
                query, 
                (current, includeProperty) => current.Include(includeProperty));
        }
        
        return await query
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken: default) as TEntity;
    }
}
