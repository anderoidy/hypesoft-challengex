using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Hypesoft.Domain.Repositories
{
    /// <summary>
    /// Defines the interface for a repository that manages roles.
    /// </summary>
    public interface IRoleRepository : IRepository<ApplicationRole>
    {
        /// <summary>
        /// Gets a role by its ID, including related users.
        /// </summary>
        /// <param name="id">The ID of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the role, or null if not found.</returns>
        Task<ApplicationRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a role by its name.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the role, or null if not found.</returns>
        Task<ApplicationRole?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a role by its normalized name.
        /// </summary>
        /// <param name="normalizedName">The normalized name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the role, or null if not found.</returns>
        Task<ApplicationRole?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a role with the specified name exists.
        /// </summary>
        /// <param name="name">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a role with the name exists; otherwise, false.</returns>
        Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of users in the specified role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of users in the role.</returns>
        Task<int> GetUsersInRoleCountAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of users in the specified role.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of users in the role.</returns>
        Task<IReadOnlyList<ApplicationUser>> GetUsersInRoleAsync(
            string roleName,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the claims associated with the specified role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of claims.</returns>
        Task<IList<IdentityRoleClaim<Guid>>> GetClaimsAsync(ApplicationRole role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a claim to the specified role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="claim">The claim to add.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> AddClaimAsync(
            ApplicationRole role,
            IdentityRoleClaim<Guid> claim,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a claim from the specified role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="claim">The claim to remove.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> RemoveClaimAsync(
            ApplicationRole role,
            IdentityRoleClaim<Guid> claim,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the normalized name of the specified role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the normalized name of the role.</returns>
        Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the normalized name for the specified role.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="normalizedName">The normalized name to set.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> SetNormalizedRoleNameAsync(
            ApplicationRole role,
            string normalizedName,
            CancellationToken cancellationToken = default);
    }
}
