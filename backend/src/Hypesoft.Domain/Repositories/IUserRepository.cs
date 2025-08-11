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
    /// Defines the interface for a repository that manages users.
    /// </summary>
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        /// <summary>
        /// Gets a user by their ID, including related roles.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or null if not found.</returns>
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or null if not found.</returns>
        Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by their username.
        /// </summary>
        /// <param name="userName">The username of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user, or null if not found.</returns>
        Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a user with the specified email exists.
        /// </summary>
        /// <param name="email">The email address to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a user with the email exists; otherwise, false.</returns>
        Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a user with the specified username exists.
        /// </summary>
        /// <param name="userName">The username to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if a user with the username exists; otherwise, false.</returns>
        Task<bool> ExistsWithUserNameAsync(string userName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the roles for a user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of role names.</returns>
        Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a user to the specified role.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a user from the specified role.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a user is in the specified role.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the user is in the role; otherwise, false.</returns>
        Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default);

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
        /// Updates a user's password.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="currentPassword">The current password of the user.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> ChangePasswordAsync(
            ApplicationUser user, 
            string currentPassword, 
            string newPassword, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets a user's password using a reset token.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="token">The password reset token.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> ResetPasswordAsync(
            ApplicationUser user, 
            string token, 
            string newPassword, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a password reset token for the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the password reset token.</returns>
        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Locks out a user until the specified date and time.
        /// </summary>
        /// <param name="user">The user to lock out.</param>
        /// <param name="lockoutEnd">The date and time after which the user will no longer be locked out.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> SetLockoutEndDateAsync(
            ApplicationUser user, 
            DateTimeOffset? lockoutEnd, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Increments the access failed count for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the new access failed count.</returns>
        Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resets the access failed count for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="IdentityResult"/> of the operation.</returns>
        Task<IdentityResult> ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of failed access attempts for the user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of failed access attempts.</returns>
        Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken = default);
    }
}
