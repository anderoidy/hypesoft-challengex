using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hypesoft.Infrastructure.Persistence.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubcategoriesAsync(Guid parentCategoryId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetCategoryTreeAsync(Guid? rootCategoryId = null, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? parentCategoryId = null, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> GetProductCountAsync(Guid categoryId, bool includeSubcategories = true, CancellationToken cancellationToken = default);
    Task MoveCategoryAsync(Guid categoryId, Guid? newParentCategoryId, string? userId = null, CancellationToken cancellationToken = default);
}

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.ParentCategoryId == null);
        
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }
        
        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetSubcategoriesAsync(Guid parentCategoryId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(c => c.ParentCategoryId == parentCategoryId);
        
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }
        
        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetCategoryTreeAsync(Guid? rootCategoryId = null, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();
        
        if (rootCategoryId.HasValue)
        {
            // Get the category and all its descendants
            var categoryIds = new List<Guid> { rootCategoryId.Value };
            await GetDescendantIdsAsync(rootCategoryId.Value, categoryIds, includeInactive, cancellationToken);
            query = query.Where(c => categoryIds.Contains(c.Id));
        }
        else if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }
        
        var categories = await query.ToListAsync(cancellationToken);
        
        // Build the tree structure
        return BuildCategoryTree(categories, rootCategoryId);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? parentCategoryId = null, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
            
        var normalizedName = name.Trim().ToLower();
        
        var query = _dbSet.Where(c => 
            c.Name.ToLower() == normalizedName && 
            c.ParentCategoryId == parentCategoryId);
            
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<Product>()
            .AnyAsync(p => p.CategoryId == categoryId, cancellationToken);
    }

    public async Task<int> GetProductCountAsync(Guid categoryId, bool includeSubcategories = true, CancellationToken cancellationToken = default)
    {
        if (!includeSubcategories)
        {
            return await _dbContext.Set<Product>()
                .CountAsync(p => p.CategoryId == categoryId, cancellationToken);
        }
        
        // Get all descendant category IDs
        var categoryIds = new List<Guid> { categoryId };
        await GetDescendantIdsAsync(categoryId, categoryIds, true, cancellationToken);
        
        return await _dbContext.Set<Product>()
            .CountAsync(p => categoryIds.Contains(p.CategoryId), cancellationToken);
    }

    public async Task MoveCategoryAsync(Guid categoryId, Guid? newParentCategoryId, string? userId = null, CancellationToken cancellationToken = default)
    {
        var category = await _dbSet.FindAsync(new object[] { categoryId }, cancellationToken);
        
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
            
        // Prevent making a category a child of itself or one of its descendants
        if (newParentCategoryId.HasValue)
        {
            if (categoryId == newParentCategoryId.Value)
                throw new InvalidOperationException("A category cannot be a parent of itself.");
                
            var descendantIds = new List<Guid>();
            await GetDescendantIdsAsync(categoryId, descendantIds, true, cancellationToken);
            
            if (descendantIds.Contains(newParentCategoryId.Value))
                throw new InvalidOperationException("A category cannot be a child of one of its descendants.");
        }
        
        category.ParentCategoryId = newParentCategoryId;
        
        if (userId != null)
        {
            category.UpdatedAt = DateTime.UtcNow;
            category.UpdatedBy = userId;
        }
        
        _dbContext.Entry(category).Property(x => x.ParentCategoryId).IsModified = true;
        _dbContext.Entry(category).Property(x => x.UpdatedAt).IsModified = true;
        _dbContext.Entry(category).Property(x => x.UpdatedBy).IsModified = true;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    #region Private Helper Methods
    
    private async Task GetDescendantIdsAsync(Guid parentId, List<Guid> categoryIds, bool includeInactive, CancellationToken cancellationToken)
    {
        var query = _dbSet.Where(c => c.ParentCategoryId == parentId);
        
        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }
        
        var childIds = await query.Select(c => c.Id).ToListAsync(cancellationToken);
        
        foreach (var childId in childIds)
        {
            categoryIds.Add(childId);
            await GetDescendantIdsAsync(childId, categoryIds, includeInactive, cancellationToken);
        }
    }
    
    private IEnumerable<Category> BuildCategoryTree(List<Category> categories, Guid? parentId = null)
    {
        return categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategory = c.ParentCategory,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy,
                UpdatedAt = c.UpdatedAt,
                UpdatedBy = c.UpdatedBy,
                Subcategories = BuildCategoryTree(categories, c.Id).ToList()
            });
    }
    
    #endregion
}
