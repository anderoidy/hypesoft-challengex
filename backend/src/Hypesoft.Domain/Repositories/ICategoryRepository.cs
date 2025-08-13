using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    /// <summary>
    /// Defines the interface for a repository that manages categories.
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Verifies if a category name is unique.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">Optional ID to exclude from the check (useful for updates).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>True if the name is unique, false otherwise.</returns>
        Task<bool> IsNameUniqueAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a category by its slug asynchronously.
        /// </summary>
        /// <param name="slug">The slug of the category.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the category, or null if not found.</returns>
        Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if any category matches the given predicate.
        /// </summary>
        /// <param name="predicate">The predicate to filter categories.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if any category matches the predicate; otherwise, false.</returns>
        Task<bool> ExistsAsync(
            Expression<Func<Category, bool>> predicate,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a category with its parent and child categories by ID asynchronously.
        /// </summary>
        /// <param name="id">The ID of the category.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the category with its parent and children, or null if not found.</returns>
        Task<Category?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets all root categories (categories with no parent) asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of root categories.</returns>
        Task<IReadOnlyList<Category>> GetRootCategoriesAsync(
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets all child categories of a specific parent category asynchronously.
        /// </summary>
        /// <param name="parentId">The ID of the parent category.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of child categories.</returns>
        Task<IReadOnlyList<Category>> GetChildCategoriesAsync(
            Guid parentId,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets all categories in a hierarchical structure asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of categories in a hierarchical structure.</returns>
        Task<IReadOnlyList<Category>> GetCategoryTreeAsync(
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a paginated list of categories asynchronously.
        /// </summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of categories.</returns>
        Task<PaginatedList<Category>> GetPaginatedCategoriesAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Checks if a category with the specified name already exists asynchronously.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <param name="excludeId">Optional. The ID of a category to exclude from the check (useful for updates).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a category with the specified name exists; otherwise, false.</returns>
        Task<bool> ExistsWithNameAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Checks if a category with the specified slug already exists asynchronously.
        /// </summary>
        /// <param name="slug">The slug to check.</param>
        /// <param name="excludeId">Optional. The ID of a category to exclude from the check (useful for updates).</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a category with the specified slug exists; otherwise, false.</returns>
        Task<bool> ExistsWithSlugAsync(
            string slug,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets the number of products in a category asynchronously.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <param name="includeChildren">Whether to include products from child categories in the count.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of products in the category.</returns>
        Task<int> GetProductCountAsync(
            Guid categoryId,
            bool includeChildren = false,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets all descendant categories of a specific category asynchronously.
        /// </summary>
        /// <param name="categoryId">The ID of the parent category.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all descendant categories.</returns>
        Task<IReadOnlyList<Category>> GetDescendantCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets all ancestor categories of a specific category asynchronously.
        /// </summary>
        /// <param name="categoryId">The ID of the child category.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of all ancestor categories in order from parent to root.</returns>
        Task<IReadOnlyList<Category>> GetAncestorCategoriesAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Moves a category to a new parent category asynchronously.
        /// </summary>
        /// <param name="categoryId">The ID of the category to move.</param>
        /// <param name="newParentId">The ID of the new parent category, or null to make it a root category.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the move was successful; otherwise, false.</returns>
        Task<bool> MoveCategoryAsync(
            Guid categoryId,
            Guid? newParentId,
            CancellationToken cancellationToken = default
        );
    }
}
