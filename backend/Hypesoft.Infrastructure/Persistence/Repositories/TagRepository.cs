using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hypesoft.Infrastructure.Persistence.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetTagsForProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task AddTagToProductAsync(Guid productId, Guid tagId, bool isFeatured = false, int displayOrder = 0, DateTime? startDate = null, DateTime? endDate = null, string? userId = null, CancellationToken cancellationToken = default);
    Task RemoveTagFromProductAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default);
    Task UpdateProductTagAsync(Guid productId, Guid tagId, bool? isFeatured = null, int? displayOrder = null, DateTime? startDate = null, DateTime? endDate = null, string? userId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetProductsByTagAsync(Guid tagId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetProductCountByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
}

public class TagRepository : Repository<Tag>, ITagRepository
{
    private readonly DbSet<ProductTag> _productTags;

    public TagRepository(ApplicationDbContext context) : base(context)
    {
        _productTags = context.Set<ProductTag>();
    }

    public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsActive)
            .OrderByDescending(t => _productTags.Count(pt => pt.TagId == t.Id))
            .ThenBy(t => t.Name)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetPagedAsync(
                pageNumber, 
                pageSize, 
                t => t.IsActive, 
                q => q.OrderBy(t => t.Name), 
                cancellationToken: cancellationToken);
        }

        var searchTermLower = searchTerm.Trim().ToLower();
        
        Expression<Func<Tag, bool>> searchPredicate = t => 
            t.IsActive &&
            (t.Name.ToLower().Contains(searchTermLower) ||
             (t.Description != null && t.Description.ToLower().Contains(searchTermLower)));
            
        return await GetPagedAsync(
            pageNumber, 
            pageSize, 
            searchPredicate, 
            q => q.OrderBy(t => t.Name), 
            cancellationToken: cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeTagId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        var normalizedName = name.Trim().ToLower();
        
        var query = _dbSet.Where(t => t.Name.ToLower() == normalizedName);
            
        if (excludeTagId.HasValue)
        {
            query = query.Where(t => t.Id != excludeTagId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetTagsForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _productTags
            .Where(pt => pt.ProductId == productId && pt.IsActive())
            .OrderBy(pt => pt.DisplayOrder)
            .ThenBy(pt => pt.Tag.Name)
            .Select(pt => pt.Tag)
            .ToListAsync(cancellationToken);
    }

    public async Task AddTagToProductAsync(Guid productId, Guid tagId, bool isFeatured = false, int displayOrder = 0, DateTime? startDate = null, DateTime? endDate = null, string? userId = null, CancellationToken cancellationToken = default)
    {
        // Check if the relationship already exists
        var existingProductTag = await _productTags
            .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
            
        if (existingProductTag != null)
        {
            // Update existing relationship
            existingProductTag.Update(isFeatured, displayOrder, startDate, endDate, userId);
            _dbContext.Entry(existingProductTag).State = EntityState.Modified;
        }
        else
        {
            // Create new relationship
            var product = await _dbContext.Set<Product>().FindAsync(new object[] { productId }, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {productId} not found.");
                
            var tag = await _dbSet.FindAsync(new object[] { tagId }, cancellationToken);
            if (tag == null)
                throw new KeyNotFoundException($"Tag with ID {tagId} not found.");
                
            var productTag = new ProductTag(product, tag, isFeatured, displayOrder, startDate, endDate, userId);
            await _productTags.AddAsync(productTag, cancellationToken);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveTagFromProductAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var productTag = await _productTags
            .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
            
        if (productTag != null)
        {
            _productTags.Remove(productTag);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateProductTagAsync(Guid productId, Guid tagId, bool? isFeatured = null, int? displayOrder = null, DateTime? startDate = null, DateTime? endDate = null, string? userId = null, CancellationToken cancellationToken = default)
    {
        var productTag = await _productTags
            .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
            
        if (productTag == null)
            throw new KeyNotFoundException($"Tag with ID {tagId} is not associated with product ID {productId}.");
            
        productTag.Update(isFeatured, displayOrder, startDate, endDate, userId);
        _dbContext.Entry(productTag).State = EntityState.Modified;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetProductsByTagAsync(Guid tagId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _productTags
            .Where(pt => pt.TagId == tagId && pt.IsActive())
            .OrderBy(pt => pt.DisplayOrder)
            .ThenByDescending(pt => pt.CreatedAt)
            .Select(pt => pt.Product);
            
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetProductCountByTagAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _productTags
            .CountAsync(pt => pt.TagId == tagId && pt.IsActive(), cancellationToken);
    }
}
