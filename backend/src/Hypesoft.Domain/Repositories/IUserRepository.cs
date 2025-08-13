using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    /// <summary>
    /// Defines the interface(s) for user repository.
    /// </summary>
    public interface IUserRepository : IIdentityRepository<ApplicationUser>
    {
        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user if found; otherwise, null.</returns>
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by their username.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user if found; otherwise, null.</returns>
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an email address is unique.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the email is unique; otherwise, false.</returns>
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
    }
}
