using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories
{
    /// <summary>
    /// Represents a repository for managing categories.
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region IRepository<Category> Implementation

        public async Task<Category> AddAsync(Category entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Generate slug if not provided
            if (string.IsNullOrEmpty(entity.Slug))
            {
                entity.Slug = GenerateSlug(entity.Name);
            }

            await _context.Categories.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<Category>> AddRangeAsync(IEnumerable<Category> entities, CancellationToken cancellationToken = default)
        {
            var categories = entities.ToList();
            var now = DateTime.UtcNow;
            
            foreach (var category in categories)
            {
                category.CreatedAt = now;
                category.UpdatedAt = now;
                
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = GenerateSlug(category.Name);
                }
            }

            await _context.Categories.AddRangeAsync(categories, cancellationToken);
            return categories;
        }

        public async Task<bool> AnyAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories.CountAsync(cancellationToken);
        }

        public void Delete(Category entity)
        {
            _context.Categories.Remove(entity);
        }

        public void DeleteRange(IEnumerable<Category> entities)
        {
            _context.Categories.RemoveRange(entities);
        }

        public async Task<Category?> FirstOrDefaultAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<Category, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> ListAsync(ISpecification<Category> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<Category, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public void Update(Category entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<Category> entities)
        {
            var now = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                entity.UpdatedAt = now;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        #endregion

        #region ICategoryRepository Implementation

        public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<Category?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(c => c.ParentId == null)
                .Include(c => c.Children)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetChildCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId)
                .Include(c => c.Children)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
        {
            var allCategories = await _context.Categories
                .Include(c => c.Children)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            // Build the tree structure
            var lookup = allCategories.ToLookup(c => c.ParentId);
            
            // Get root categories (where ParentId is null)
            var rootCategories = lookup[null].ToList();
            
            // Build the tree recursively
            foreach (var category in allCategories)
            {
                category.Children = lookup[category.Id].OrderBy(c => c.Name).ToList();
            }

            return rootCategories;
        }

        public async Task<PaginatedList<Category>> GetPaginatedCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .OrderBy(c => c.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Category>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.Where(c => c.Name == name);
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithSlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Categories.Where(c => c.Slug == slug);
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> GetProductCountAsync(Guid categoryId, bool includeChildren = false, CancellationToken cancellationToken = default)
        {
            if (!includeChildren)
            {
                return await _context.Products
                    .CountAsync(p => p.CategoryId == categoryId, cancellationToken);
            }
            
            // Get all descendant category IDs including the current one
            var categoryIds = await GetDescendantCategoryIdsAsync(categoryId, cancellationToken);
            categoryIds.Add(categoryId);
            
            return await _context.Products
                .CountAsync(p => categoryIds.Contains(p.CategoryId), cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetDescendantCategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            var allCategories = await _context.Categories.ToListAsync(cancellationToken);
            var result = new List<Category>();
            
            // Get all categories and build a lookup for parent-child relationships
            var lookup = allCategories.ToLookup(c => c.ParentId);
            
            // Recursively get all descendants
            await GetDescendantsRecursive(categoryId, lookup, result, cancellationToken);
            
            return result;
        }

        public async Task<IReadOnlyList<Category>> GetAncestorCategoriesAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            var result = new List<Category>();
            var category = await _context.Categories
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
            
            if (category == null)
                return result;
            
            // Get all ancestors by traversing up the parent chain
            var current = category.Parent;
            while (current != null)
            {
                result.Insert(0, current); // Insert at beginning to maintain order from parent to root
                
                // Get the parent with its parent included
                current = await _context.Categories
                    .Include(c => c.Parent)
                    .FirstOrDefaultAsync(c => c.Id == current.Id, cancellationToken)?.Parent;
            }
            
            return result;
        }

        public async Task<bool> MoveCategoryAsync(Guid categoryId, Guid? newParentId, CancellationToken cancellationToken = default)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
                
            if (category == null)
                return false;
                
            // Check if the new parent exists (if provided)
            if (newParentId.HasValue)
            {
                var newParentExists = await _context.Categories
                    .AnyAsync(c => c.Id == newParentId.Value, cancellationToken);
                    
                if (!newParentExists)
                    return false;
                    
                // Prevent circular references
                if (await IsCircularReference(categoryId, newParentId.Value, cancellationToken))
                    return false;
            }
            
            // Update the parent ID
            category.ParentId = newParentId;
            category.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(category).State = EntityState.Modified;
            return true;
        }

        #endregion

        #region Private Methods

        private IQueryable<Category> ApplySpecification(ISpecification<Category> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.Categories.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<Category, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.Categories.AsQueryable(), specification);
        }

        private string GenerateSlug(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // Convert to lowercase and replace spaces with hyphens
            var slug = name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and");

            // Remove invalid characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9-]", "");

            // Remove multiple consecutive hyphens
            slug = System.Text.RegularExpressions.Regex.Replace(slug, "-+", "-");

            // Trim hyphens from the beginning and end
            return slug.Trim('-');
        }

        private async Task GetDescendantsRecursive(Guid parentId, ILookup<Guid?, Category> lookup, List<Category> result, CancellationToken cancellationToken)
        {
            var children = lookup[parentId].ToList();
            
            foreach (var child in children)
            {
                result.Add(child);
                await GetDescendantsRecursive(child.Id, lookup, result, cancellationToken);
            }
        }

        private async Task<HashSet<Guid>> GetDescendantCategoryIdsAsync(Guid categoryId, CancellationToken cancellationToken)
        {
            var result = new HashSet<Guid>();
            var allCategories = await _context.Categories.ToListAsync(cancellationToken);
            var lookup = allCategories.ToLookup(c => c.ParentId);
            
            await GetDescendantIdsRecursive(categoryId, lookup, result, cancellationToken);
            return result;
        }

        private async Task GetDescendantIdsRecursive(Guid parentId, ILookup<Guid?, Category> lookup, HashSet<Guid> result, CancellationToken cancellationToken)
        {
            var children = lookup[parentId].ToList();
            
            foreach (var child in children)
            {
                result.Add(child.Id);
                await GetDescendantIdsRecursive(child.Id, lookup, result, cancellationToken);
            }
        }

        private async Task<bool> IsCircularReference(Guid categoryId, Guid newParentId, CancellationToken cancellationToken)
        {
            // If the new parent is the same as the category, it's a circular reference
            if (categoryId == newParentId)
                return true;
                
            // Get all ancestors of the new parent
            var newParentAncestors = await GetAncestorCategoriesAsync(newParentId, cancellationToken);
            
            // If any ancestor is the category being moved, it's a circular reference
            return newParentAncestors.Any(a => a.Id == categoryId);
        }

        #endregion
    }
}
