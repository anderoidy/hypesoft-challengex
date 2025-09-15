using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context, ILogger<CategoryRepository> logger)
            : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Category> AddAsync(
            Category category,
            CancellationToken cancellationToken = default
        )
        {
            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return category;
        }

        public new async Task<IEnumerable<Category>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Categories.ToListAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Category?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Categories.FirstOrDefaultAsync(
                c => c.Name == name,
                cancellationToken
            );
        }

        public new async Task<Category> UpdateAsync(
            Category category,
            CancellationToken cancellationToken = default
        )
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync(cancellationToken);
            return category;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var category = await GetByIdAsync(id, cancellationToken);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }

        public async Task DeleteAsync(
            Category category,
            CancellationToken cancellationToken = default
        )
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<bool> HasSubCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Categories.AnyAsync(
                c => c.ParentCategoryId == categoryId,
                cancellationToken
            );
        }

        public async Task<bool> HasProductsAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            // Assumindo que existe uma entidade Product com CategoryId
            return await _context.Products.AnyAsync(
                p => p.CategoryId == categoryId,
                cancellationToken
            );
        }

        public async Task<IEnumerable<Category>> GetMainCategoriesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Categories.Where(c => c.ParentCategoryId == null)
                .ToListAsync(cancellationToken);
        }
    }
}
