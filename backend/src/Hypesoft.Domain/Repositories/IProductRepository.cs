using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    /// <summary>
    /// Defines the interface for a repository that manages products.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>
        /// Gets a product by its slug asynchronously.
        /// </summary>
        /// <param name="slug">The slug of the product.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product, or null if not found.</returns>
        Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a product with its category and tags by ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product with its category and tags, or null if not found.</returns>
        Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a product by its SKU asynchronously.
        /// </summary>
        /// <param name="sku">The SKU of the product.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product, or null if not found.</returns>
        Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of products with their categories and tags asynchronously.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of products with their categories and tags.</returns>
        Task<PaginatedList<Product>> GetPaginatedProductsWithDetailsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of products by category ID asynchronously.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of products in the specified category.</returns>
        Task<PaginatedList<Product>> GetProductsByCategoryIdAsync(
            Guid categoryId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of products by tag ID asynchronously.
        /// </summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of products with the specified tag.</returns>
        Task<PaginatedList<Product>> GetProductsByTagIdAsync(
            Guid tagId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of featured products asynchronously.
        /// </summary>
        /// <param name="count">The maximum number of featured products to return.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of featured products.</returns>
        Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(
            int count = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of related products asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product to find related products for.</param>
        /// <param name="count">The maximum number of related products to return.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of related products.</returns>
        Task<IReadOnlyList<Product>> GetRelatedProductsAsync(
            Guid productId,
            int count = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Searches for products by name or description asynchronously.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of matching products.</returns>
        Task<PaginatedList<Product>> SearchProductsAsync(
            string searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the total count of products asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the total count of products.</returns>
        Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a product with the specified SKU already exists asynchronously.
        /// </summary>
        /// <param name="sku">The SKU to check.</param>
        /// <param name="excludeId">Optional. The ID of a product to exclude from the check (useful for updates).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a product with the specified SKU exists; otherwise, false.</returns>
        Task<bool> ExistsWithSkuAsync(string sku, Guid? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a product with the specified slug already exists asynchronously.
        /// </summary>
        /// <param name="slug">The slug to check.</param>
        /// <param name="excludeId">Optional. The ID of a product to exclude from the check (useful for updates).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a product with the specified slug exists; otherwise, false.</returns>
        Task<bool> ExistsWithSlugAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of product IDs that are out of stock asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of product IDs that are out of stock.</returns>
        Task<IReadOnlyList<Guid>> GetOutOfStockProductIdsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a product's stock quantity asynchronously.
        /// </summary>
        /// <param name="productId">The ID of the product to update.</param>
        /// <param name="quantityChange">The change in quantity (positive for addition, negative for subtraction).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateStockQuantityAsync(Guid productId, int quantityChange, CancellationToken cancellationToken = default);
    }
}