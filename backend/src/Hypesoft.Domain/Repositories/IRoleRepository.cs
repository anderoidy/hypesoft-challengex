using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    /// <summary>
    /// Defines the interface(s) for role repository.
    /// </summary>
    public interface IRoleRepository : IIdentityRepository<ApplicationRole>
    {
        /// <summary>
        /// Gets a role by its name.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the role if found; otherwise, null.</returns>
        Task<ApplicationRole?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a role with the specified name exists.
        /// </summary>
        /// <param name="roleName">The name of the role to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the role exists; otherwise, false.</returns>
        Task<bool> RoleExistsAsync(string roleName, CancellationToken cancellationToken = default);
    }
}
