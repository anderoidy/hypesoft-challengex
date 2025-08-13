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
    public class ProductRepository : RepositoryBase<Product>, IProductRepository
    {
        private readonly IMongoCollection<Category> _categoryCollection;

        public ProductRepository(ApplicationDbContext context)
            : base(context, nameof(ApplicationDbContext.Products))
        {
            _categoryCollection = context.Database.GetCollection<Category>("Categories");
        }

        public async Task<Product?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection
                .Find(p => p.Slug == slug)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Product?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            var product = await _collection
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (product != null)
            {
                // Carrega a categoria relacionada
                var category = await _categoryCollection
                    .Find(c => c.Id == product.CategoryId)
                    .FirstOrDefaultAsync(cancellationToken);

                product.Category = category;
            }

            return product;
        }

        public async Task<Product?> GetBySkuAsync(
            string sku,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection.Find(p => p.Sku == sku).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<PaginatedList<Product>> GetPaginatedProductsWithDetailsAsync(
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
                .ToListAsync(cancellationToken);

            // Carrega as categorias para os produtos
            var categoryIds = items.Select(p => p.CategoryId).Distinct().ToList();
            var categories = await _categoryCollection
                .Find(c => categoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            var categoriesDict = categories.ToDictionary(c => c.Id, c => c);

            foreach (var product in items)
            {
                if (categoriesDict.TryGetValue(product.CategoryId, out var category))
                {
                    product.Category = category;
                }
            }

            return new PaginatedList<Product>(items, (int)count, pageNumber, pageSize);
        }

        public async Task<PaginatedList<Product>> GetProductsByCategoryIdAsync(
            Guid categoryId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
            var count = await _collection.CountDocumentsAsync(
                filter,
                cancellationToken: cancellationToken
            );

            var items = await _collection
                .Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Product>(items, (int)count, pageNumber, pageSize);
        }

        public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
            int count = 5,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection
                .Find(p => p.IsFeatured)
                .SortByDescending(p => p.CreatedAt)
                .Limit(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Product>> GetRelatedProductsAsync(
            Guid productId,
            int count = 5,
            CancellationToken cancellationToken = default
        )
        {
            // Primeiro, obtém o produto atual para encontrar a categoria
            var product = await GetByIdAsync(productId, cancellationToken);
            if (product == null)
                return new List<Product>();

            // Busca produtos da mesma categoria, excluindo o produto atual
            return await _collection
                .Find(p => p.CategoryId == product.CategoryId && p.Id != productId)
                .Limit(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaginatedList<Product>> SearchProductsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Builders<Product>.Filter.Or(
                Builders<Product>.Filter.Regex(
                    p => p.Name,
                    new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")
                ),
                Builders<Product>.Filter.Regex(
                    p => p.Description,
                    new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")
                )
            );

            var count = await _collection.CountDocumentsAsync(
                filter,
                cancellationToken: cancellationToken
            );
            var items = await _collection
                .Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<Product>(items, (int)count, pageNumber, pageSize);
        }

        public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            return (int)
                await _collection.CountDocumentsAsync(
                    _ => true,
                    cancellationToken: cancellationToken
                );
        }

        public async Task<bool> ExistsWithSkuAsync(
            string sku,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var filter = excludeId.HasValue
                ? Builders<Product>.Filter.And(
                    Builders<Product>.Filter.Eq(p => p.Sku, sku),
                    Builders<Product>.Filter.Ne(p => p.Id, excludeId.Value)
                )
                : Builders<Product>.Filter.Eq(p => p.Sku, sku);

            return await _collection.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithSlugAsync(
            string slug,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            var filter = excludeId.HasValue
                ? Builders<Product>.Filter.And(
                    Builders<Product>.Filter.Eq(p => p.Slug, slug),
                    Builders<Product>.Filter.Ne(p => p.Id, excludeId.Value)
                )
                : Builders<Product>.Filter.Eq(p => p.Slug, slug);

            return await _collection.Find(filter).AnyAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetOutOfStockProductIdsAsync(
            CancellationToken cancellationToken = default
        )
        {
            var filter = Builders<Product>.Filter.Lte(p => p.StockQuantity, 0);
            var projection = Builders<Product>.Projection.Include(p => p.Id);

            var cursor = await _collection.FindAsync(
                filter,
                new FindOptions<Product, Product> { Projection = projection },
                cancellationToken
            );

            var products = await cursor.ToListAsync(cancellationToken);
            return products.Select(p => p.Id).ToList();
        }

        public async Task<bool> UpdateStockQuantityAsync(
            Guid productId,
            int quantityChange,
            CancellationToken cancellationToken = default
        )
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>
                .Update.Inc(p => p.StockQuantity, quantityChange)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _collection.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken
            );
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task<bool> IsSkuUniqueAsync(
            string sku,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        )
        {
            return !await ExistsWithSkuAsync(sku, excludeId, cancellationToken);
        }

        public async Task<(IReadOnlyList<Product> Items, int TotalItems)> GetPagedAsync(
            Expression<Func<Product, bool>>? predicate = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default
        )
        {
            var filter =
                predicate != null
                    ? Builders<Product>.Filter.Where(predicate)
                    : Builders<Product>.Filter.Empty;

            var count = await _collection.CountDocumentsAsync(
                filter,
                cancellationToken: cancellationToken
            );
            var items = await _collection
                .Find(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (items, (int)count);
        }

        // CORREÇÃO: Implementação direta que retorna Task<Product> para IProductRepository
        public override async Task<Product> AddAsync(
            Product entity,
            CancellationToken cancellationToken = default
        )
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreatedAt = DateTime.UtcNow;
            entity.SetUpdatedAt(DateTime.UtcNow);

            await _collection.InsertOneAsync(entity, cancellationToken: cancellationToken);
            return entity; // ← Retorna o produto adicionado
        }

        // CORREÇÃO: Implementação direta do Delete para IProductRepository
        public void Delete(Product entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            _collection.DeleteOne(p => p.Id == entity.Id);
        }

        // CORREÇÃO: CountAsync com nullability correta
        public override async Task<int> CountAsync(
            Expression<Func<Product, bool>>? predicate = null,
            CancellationToken cancellationToken = default
        )
        {
            var filter =
                predicate != null
                    ? Builders<Product>.Filter.Where(predicate)
                    : Builders<Product>.Filter.Empty;

            return (int)
                await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        }

        // CORREÇÃO: ExistsAsync para IProductRepository
        public async Task<bool> ExistsAsync(
            Expression<Func<Product, bool>> predicate,
            CancellationToken cancellationToken = default
        )
        {
            return await _collection.Find(predicate).AnyAsync(cancellationToken);
        }
    }
}
