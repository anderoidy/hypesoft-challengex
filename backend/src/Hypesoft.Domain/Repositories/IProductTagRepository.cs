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
    /// Defines the interface for a repository that manages the many-to-many relationship between products and tags.
    /// </summary>
    public interface IProductTagRepository : IRepository<ProductTag>
    {
        /// <summary>
        /// Gets a product-tag relationship by its ID, including related product and tag.
        /// </summary>
        /// <param name="id">The ID of the product-tag relationship.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product-tag relationship, or null if not found.</returns>
        Task<ProductTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a product-tag relationship by product ID and tag ID, including related product and tag.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the product-tag relationship, or null if not found.</returns>
        Task<ProductTag?> GetByProductAndTagIdAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all product-tag relationships for a specific product, including related tags.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of product-tag relationships.</returns>
        Task<IReadOnlyList<ProductTag>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all product-tag relationships for a specific tag, including related products.
        /// </summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of product-tag relationships.</returns>
        Task<IReadOnlyList<ProductTag>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a product-tag relationship exists.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the relationship exists; otherwise, false.</returns>
        Task<bool> ExistsAsync(Guid productId, Guid tagId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all product-tag relationships for a specific product.
        /// </summary>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all product-tag relationships for a specific tag.
        /// </summary>
        /// <param name="tagId">The ID of the tag.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    }
}
