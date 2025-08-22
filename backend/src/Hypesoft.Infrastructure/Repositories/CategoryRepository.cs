using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>
    {
        public CategoryRepository(ApplicationDbContext context, ILogger logger)
            : base(context, logger) { }

        public async Task<bool> IsNameUniqueAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = DbContext.Set<Category>().Where(c => c.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<Category?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default
        )
        {
            return await DbContext
                .Set<Category>()
                .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        }

        public async Task<Category?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            return await DbContext
                .Set<Category>()
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await DbContext
                .Set<Category>()
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetChildCategoriesAsync(
            Guid parentId,
            CancellationToken cancellationToken = default
        )
        {
            return await DbContext
                .Set<Category>()
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetCategoryTreeAsync(
            CancellationToken cancellationToken = default
        )
        {
            var allCategories = await DbContext
                .Set<Category>()
                .Include(c => c.ChildCategories)
                .ThenInclude(c => c.ChildCategories)
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return allCategories;
        }

        public async Task<PaginatedList<Category>> GetPaginatedCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default
        )
        {
            var query = DbContext
                .Set<Category>()
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.Name);

            var count = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Category>(items, count, pageNumber, pageSize);
        }

        public async Task<bool> ExistsWithNameAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = DbContext.Set<Category>().Where(c => c.Name == name);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithSlugAsync(
            string slug,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var query = DbContext.Set<Category>().Where(c => c.Slug == slug);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> GetProductCountAsync(
            Guid categoryId,
            bool includeChildren = false,
            CancellationToken cancellationToken = default
        )
        {
            if (!includeChildren)
            {
                return await DbContext
                    .Set<Product>()
                    .CountAsync(p => p.CategoryId == categoryId, cancellationToken);
            }

            // Para incluir filhos, busca todas as categorias descendentes
            var categoryIds = await GetDescendantCategoryIds(categoryId, cancellationToken);
            categoryIds.Add(categoryId);

            return await DbContext
                .Set<Product>()
                .CountAsync(p => categoryIds.Contains(p.CategoryId), cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetDescendantCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            var descendants = new List<Category>();
            await GetDescendantsRecursive(categoryId, descendants, cancellationToken);
            return descendants;
        }

        public async Task<IReadOnlyList<Category>> GetAncestorCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        )
        {
            var ancestors = new List<Category>();
            var category = await DbContext
                .Set<Category>()
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

            while (category?.ParentCategory != null)
            {
                ancestors.Add(category.ParentCategory);
                category = await DbContext
                    .Set<Category>()
                    .Include(c => c.ParentCategory)
                    .FirstOrDefaultAsync(
                        c => c.Id == category.ParentCategory.Id,
                        cancellationToken
                    );
            }

            return ancestors;
        }

        public async Task<bool> MoveCategoryAsync(
            Guid categoryId,
            Guid? newParentId,
            CancellationToken cancellationToken = default
        )
        {
            // Verifica se a nova categoria pai não é um descendente da categoria atual
            if (newParentId.HasValue)
            {
                var descendants = await GetDescendantCategoriesAsync(categoryId, cancellationToken);
                if (descendants.Any(d => d.Id == newParentId.Value))
                {
                    return false;
                }
            }

            var category = await DbContext
                .Set<Category>()
                .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

            if (category == null)
                return false;

            // ✅ Resolvido
            category.ChangeParent(newParentId);

            await DbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        // Métodos auxiliares privados
        private async Task GetDescendantsRecursive(
            Guid parentId,
            List<Category> descendants,
            CancellationToken cancellationToken
        )
        {
            var children = await DbContext
                .Set<Category>()
                .Where(c => c.ParentCategoryId == parentId)
                .ToListAsync(cancellationToken);

            foreach (var child in children)
            {
                descendants.Add(child);
                await GetDescendantsRecursive(child.Id, descendants, cancellationToken);
            }
        }

        private async Task<List<Guid>> GetDescendantCategoryIds(
            Guid parentId,
            CancellationToken cancellationToken
        )
        {
            var categoryIds = new List<Guid>();
            var children = await DbContext
                .Set<Category>()
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var childId in children)
            {
                categoryIds.Add(childId);
                var grandChildren = await GetDescendantCategoryIds(childId, cancellationToken);
                categoryIds.AddRange(grandChildren);
            }

            return categoryIds;
        }
    }
}
