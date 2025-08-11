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
    /// Represents a repository for managing product-tag relationships.
    /// </summary>
    public class ProductTagRepository : IProductTagRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductTagRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ProductTagRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<ProductTag> AddAsync(ProductTag entity, CancellationToken cancellationToken = default)
        {
            await _context.ProductTags.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProductTag>> AddRangeAsync(IEnumerable<ProductTag> entities, CancellationToken cancellationToken = default)
        {
            await _context.ProductTags.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(ISpecification<ProductTag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags.AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(ISpecification<ProductTag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags.CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void Delete(ProductTag entity)
        {
            _context.ProductTags.Remove(entity);
        }

        /// <inheritdoc />
        public void DeleteRange(IEnumerable<ProductTag> entities)
        {
            _context.ProductTags.RemoveRange(entities);
        }

        /// <inheritdoc />
        public async Task<ProductTag?> FirstOrDefaultAsync(ISpecification<ProductTag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<ProductTag, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags
                .Include(pt => pt.Product)
                .Include(pt => pt.Tag)
                .FirstOrDefaultAsync(pt => pt.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ProductTag>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags
                .Include(pt => pt.Product)
                .Include(pt => pt.Tag)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ProductTag>> ListAsync(ISpecification<ProductTag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .Include(pt => pt.Product)
                .Include(pt => pt.Tag)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<ProductTag, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void Update(ProductTag entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<ProductTag> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        /// <inheritdoc />
        public async Task<ProductTag?> GetByProductAndTagIdAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags
                .Include(pt => pt.Product)
                .Include(pt => pt.Tag)
                .FirstOrDefaultAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ProductTag>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags
                .Where(pt => pt.ProductId == productId)
                .Include(pt => pt.Tag)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ProductTag>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags
                .Where(pt => pt.TagId == tagId)
                .Include(pt => pt.Product)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default)
        {
            return await _context.ProductTags
                .AnyAsync(pt => pt.ProductId == productId && pt.TagId == tagId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            var productTags = await _context.ProductTags
                .Where(pt => pt.ProductId == productId)
                .ToListAsync(cancellationToken);

            _context.ProductTags.RemoveRange(productTags);
        }

        /// <inheritdoc />
        public async Task RemoveByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
        {
            var productTags = await _context.ProductTags
                .Where(pt => pt.TagId == tagId)
                .ToListAsync(cancellationToken);

            _context.ProductTags.RemoveRange(productTags);
        }

        private IQueryable<ProductTag> ApplySpecification(ISpecification<ProductTag> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.ProductTags.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<ProductTag, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.ProductTags.AsQueryable(), specification);
        }
    }
}
