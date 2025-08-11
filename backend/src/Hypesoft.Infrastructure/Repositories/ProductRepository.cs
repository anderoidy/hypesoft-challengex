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
    /// Represents a repository for managing products.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ProductRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region IRepository<Product> Implementation

        public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            // Generate slug if not provided
            if (string.IsNullOrEmpty(entity.Slug))
            {
                entity.Slug = GenerateSlug(entity.Name);
            }

            await _context.Products.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
        {
            var products = entities.ToList();
            var now = DateTime.UtcNow;
            
            foreach (var product in products)
            {
                product.CreatedAt = now;
                product.UpdatedAt = now;
                
                if (string.IsNullOrEmpty(product.Slug))
                {
                    product.Slug = GenerateSlug(product.Name);
                }
            }

            await _context.Products.AddRangeAsync(products, cancellationToken);
            return products;
        }

        public async Task<bool> AnyAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.AnyAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.CountAsync(cancellationToken);
        }

        public void Delete(Product entity)
        {
            _context.Products.Remove(entity);
        }

        public void DeleteRange(IEnumerable<Product> entities)
        {
            _context.Products.RemoveRange(entities);
        }

        public async Task<Product?> FirstOrDefaultAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<Product, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<Product, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }

        public void Update(Product entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<Product> entities)
        {
            var now = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                entity.UpdatedAt = now;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        #endregion

        #region IProductRepository Implementation

        public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
        }

        public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
        }

        public async Task<PaginatedList<Product>> GetPaginatedProductsWithDetailsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Product>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedList<Product>> GetProductsByCategoryIdAsync(
            Guid categoryId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Product>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PaginatedList<Product>> GetProductsByTagIdAsync(
            Guid tagId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Products
                .Where(p => p.ProductTags.Any(pt => pt.TagId == tagId))
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Product>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
            int count = 5,
            CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.IsFeatured)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(
            Guid productId,
            int count = 5,
            CancellationToken cancellationToken = default)
        {
            // Get the product to find related products for
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
                return new List<Product>();

            // Get products in the same category, excluding the current product
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != productId)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);

            // If we don't have enough related products, get some from other categories
            if (relatedProducts.Count < count)
            {
                var additionalProducts = await _context.Products
                    .Where(p => p.CategoryId != product.CategoryId && p.Id != productId)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count - relatedProducts.Count)
                    .ToListAsync(cancellationToken);

                relatedProducts.AddRange(additionalProducts);
            }

            return relatedProducts;
        }

        public async Task<PaginatedList<Product>> SearchProductsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var searchTermLower = searchTerm.ToLower();
            
            var query = _context.Products
                .Where(p => 
                    p.Name.ToLower().Contains(searchTermLower) ||
                    p.Description.ToLower().Contains(searchTermLower) ||
                    p.Sku.ToLower().Contains(searchTermLower))
                .Include(p => p.Category)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Product>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products.CountAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithSkuAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Products.Where(p => p.Sku == sku);
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithSlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Products.Where(p => p.Slug == slug);
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }
            
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetOutOfStockProductIdsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .Where(p => p.StockQuantity <= 0)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateStockQuantityAsync(Guid productId, int quantityChange, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.FindAsync(new object[] { productId }, cancellationToken);
            
            if (product == null)
                return false;

            // Ensure we don't go below zero
            var newQuantity = product.StockQuantity + quantityChange;
            product.StockQuantity = Math.Max(0, newQuantity);
            product.UpdatedAt = DateTime.UtcNow;
            
            _context.Entry(product).State = EntityState.Modified;
            return true;
        }

        #endregion

        #region Private Methods

        private IQueryable<Product> ApplySpecification(ISpecification<Product> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.Products.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<Product, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.Products.AsQueryable(), specification);
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

        #endregion
    }
}
