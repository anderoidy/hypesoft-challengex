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
    /// Represents a repository for managing tags.
    /// </summary>
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TagRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<Tag> AddAsync(Tag entity, CancellationToken cancellationToken = default)
        {
            await _context.Tags.AddAsync(entity, cancellationToken);
            return entity;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Tag>> AddRangeAsync(IEnumerable<Tag> entities, CancellationToken cancellationToken = default)
        {
            await _context.Tags.AddRangeAsync(entities, cancellationToken);
            return entities;
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(ISpecification<Tag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tags.AnyAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(ISpecification<Tag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tags.CountAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void Delete(Tag entity)
        {
            _context.Tags.Remove(entity);
        }

        /// <inheritdoc />
        public void DeleteRange(IEnumerable<Tag> entities)
        {
            _context.Tags.RemoveRange(entities);
        }

        /// <inheritdoc />
        public async Task<Tag?> FirstOrDefaultAsync(ISpecification<Tag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<Tag, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.ProductTags)
                    .ThenInclude(pt => pt.Product)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Tag?> GetByIdWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.ProductTags)
                    .ThenInclude(pt => pt.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Tag>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.ProductTags)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Tag>> ListAsync(ISpecification<Tag> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .Include(t => t.ProductTags)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<Tag, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void Update(Tag entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        /// <inheritdoc />
        public void UpdateRange(IEnumerable<Tag> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        /// <inheritdoc />
        public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.ProductTags)
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.ProductTags)
                .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Include(t => t.ProductTags)
                .OrderByDescending(t => t.ProductTags.Count)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Tag>> GetTagsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            return await _context.Tags
                .Where(t => t.ProductTags.Any(pt => pt.ProductId == productId))
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            return !await _context.Tags
                .AnyAsync(t => t.Name == name && (!excludeId.HasValue || t.Id != excludeId.Value), cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> IsSlugUniqueAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            return !await _context.Tags
                .AnyAsync(t => t.Slug == slug && (!excludeId.HasValue || t.Id != excludeId.Value), cancellationToken);
        }

        private IQueryable<Tag> ApplySpecification(ISpecification<Tag> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.Tags.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<Tag, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.Tags.AsQueryable(), specification);
        }
    }
}
