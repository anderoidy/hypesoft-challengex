using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Hypesoft.Infrastructure.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        private readonly IMongoCollection<Product> _productCollection;

        public CategoryRepository(ApplicationDbContext context)
            : base(context, nameof(ApplicationDbContext.Categories))
        {
            _productCollection = context.Database.GetCollection<Product>("Products");
        }

        public async Task<bool> IsNameUniqueAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            return !await ExistsWithNameAsync(name, excludeId, cancellationToken);
        }

        public async Task<Category?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection
                .Find(c => c.Slug == slug)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Category?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            var category = await _collection
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (category != null)
            {
                // Carrega a categoria pai, se houver
                if (category.ParentCategoryId.HasValue)
                {
                    category.ParentCategory = await _collection
                        .Find(c => c.Id == category.ParentCategoryId.Value)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                // Carrega as categorias filhas
                category.ChildCategories = await _collection
                    .Find(c => c.ParentCategoryId == category.Id)
                    .ToListAsync(cancellationToken);
            }

            return category;
        }

        public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _collection
                .Find(c => c.ParentCategoryId == null || !c.ParentCategoryId.HasValue)
                .SortBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetChildCategoriesAsync(
            Guid parentId,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection
                .Find(c => c.ParentCategoryId == parentId)
                .SortBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Category>> GetCategoryTreeAsync(
            CancellationToken cancellationToken = default
        )
        {
            var allCategories = await _collection
                .Find(_ => true)
                .SortBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

            // Função recursiva para construir a árvore
            void BuildTree(List<Category> categories)
            {
                foreach (var category in categories)
                {
                    category.ChildCategories = lookup[category.Id].ToList();
                    if (category.ChildCategories.Any())
                    {
                        BuildTree(category.ChildCategories);
                    }
                }
            }

            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();
            BuildTree(rootCategories);

            return rootCategories;
        }

        public async Task<PaginatedList<Category>> GetPaginatedCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default
        )
        {
            var count = await _collection.CountDocumentsAsync(
                _ => true,
                cancellationToken: cancellationToken
            );
            var items = await _collection
                .Find(_ => true)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .SortBy(c => c.Name)
                .ToListAsync(cancellationToken);

            // Carrega os nomes das categorias pai
            var parentIds = items
                .Where(c => c.ParentCategoryId.HasValue)
                .Select(c => c.ParentCategoryId.Value)
                .Distinct()
                .ToList();

            if (parentIds.Any())
            {
                var parents = await _collection
                    .Find(c => parentIds.Contains(c.Id))
                    .Project(c => new { c.Id, c.Name })
                    .ToListAsync(cancellationToken);

                var parentLookup = parents.ToDictionary(p => p.Id, p => p.Name);

                foreach (var category in items.Where(c => c.ParentCategoryId.HasValue))
                {
                    if (
                        parentLookup.TryGetValue(
                            category.ParentCategoryId.Value,
                            out var parentName
                        )
                    )
                    {
                        category.ParentCategory = new Category
                        {
                            Id = category.ParentCategoryId.Value,
                            Name = parentName,
                        };
                    }
                }
            }

            return new PaginatedList<Category>(items, (int)count, pageNumber, pageSize);
        }

        public async Task<bool> ExistsWithNameAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var filter = excludeId.HasValue
                ? Builders<Category>.Filter.And(
                    Builders<Category>.Filter.Eq(c => c.Name, name),
                    Builders<Category>.Filter.Ne(c => c.Id, excludeId.Value)
                )
                : Builders<Category>.Filter.Eq(c => c.Name, name);

            return await _collection.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithSlugAsync(
            string slug,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var filter = excludeId.HasValue
                ? Builders<Category>.Filter.And(
                    Builders<Category>.Filter.Eq(c => c.Slug, slug),
                    Builders<Category>.Filter.Ne(c => c.Id, excludeId.Value)
                )
                : Builders<Category>.Filter.Eq(c => c.Slug, slug);

            return await _collection.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<int> GetProductCountAsync(
            Guid categoryId,
            bool includeChildren = false,
            CancellationToken cancellationToken = default
        )
        {
            if (!includeChildren)
            {
                return (int)
                    await _productCollection.CountDocumentsAsync(
                        p => p.CategoryId == categoryId,
                        cancellationToken: cancellationToken
                    );
            }

            var categoryIds = new List<Guid> { categoryId };
            await GetDescendantCategoryIds(categoryId, categoryIds, cancellationToken);

            return (int)
                await _productCollection.CountDocumentsAsync(
                    p => categoryIds.Contains(p.CategoryId),
                    cancellationToken: cancellationToken
                );
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
            await GetAncestorsRecursive(categoryId, ancestors, cancellationToken);
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
                    return false; // Não pode mover uma categoria para um de seus próprios descendentes
                }
            }

            var category = await _collection
                .Find(c => c.Id == categoryId)
                .FirstOrDefaultAsync(cancellationToken);
            if (category == null)
                return false;

            category.ParentCategoryId = newParentId;
            category.IsMainCategory = !newParentId.HasValue;
            category.SetUpdatedAt(DateTime.UtcNow);

            await _collection.ReplaceOneAsync(
                c => c.Id == categoryId,
                category,
                cancellationToken: cancellationToken
            );
            return true;
        }

        private async Task GetDescendantsRecursive(
            Guid parentId,
            List<Category> descendants,
            CancellationToken cancellationToken
        )
        {
            var children = await _collection
                .Find(c => c.ParentCategoryId == parentId)
                .ToListAsync(cancellationToken);

            foreach (var child in children)
            {
                descendants.Add(child);
                await GetDescendantsRecursive(child.Id, descendants, cancellationToken);
            }
        }

        private async Task GetAncestorsRecursive(
            Guid categoryId,
            List<Category> ancestors,
            CancellationToken cancellationToken
        )
        {
            var category = await _collection
                .Find(c => c.Id == categoryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (category?.ParentCategoryId.HasValue == true)
            {
                var parent = await _collection
                    .Find(c => c.Id == category.ParentCategoryId.Value)
                    .FirstOrDefaultAsync(cancellationToken);

                if (parent != null)
                {
                    ancestors.Add(parent);
                    await GetAncestorsRecursive(parent.Id, ancestors, cancellationToken);
                }
            }
        }

        private async Task GetDescendantCategoryIds(
            Guid parentId,
            List<Guid> categoryIds,
            CancellationToken cancellationToken
        )
        {
            var children = await _collection
                .Find(c => c.ParentCategoryId == parentId)
                .Project(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var childId in children)
            {
                categoryIds.Add(childId);
                await GetDescendantCategoryIds(childId, categoryIds, cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(
            Expression<Func<Category, bool>> predicate,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection.Find(predicate).AnyAsync(cancellationToken);
        }
    }
}
