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
    /// Defines the interface for a repository that manages user sessions.
    /// </summary>
    public interface IUserSessionRepository : IRepository<UserSession>
    {
        /// <summary>
        /// Gets an active user session by its ID.
        /// </summary>
        /// <param name="id">The ID of the session.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user session, or null if not found or expired.</returns>
        Task<UserSession?> GetActiveSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an active user session by its refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user session, or null if not found or expired.</returns>
        Task<UserSession?> GetActiveSessionByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active sessions for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of active user sessions.</returns>
        Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a user session by its ID.
        /// </summary>
        /// <param name="sessionId">The ID of the session to revoke.</param>
        /// <param name="revokedBy">The ID of the user who is revoking the session.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the session was successfully revoked.</returns>
        Task<bool> RevokeSessionAsync(Guid sessionId, Guid? revokedBy = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all active sessions for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="exceptSessionId">Optional. The ID of a session to exclude from revocation.</param>
        /// <param name="revokedBy">Optional. The ID of the user who is revoking the sessions.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates the number of sessions that were revoked.</returns>
        Task<int> RevokeAllSessionsForUserAsync(Guid userId, Guid? exceptSessionId = null, Guid? revokedBy = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all expired sessions.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates the number of sessions that were revoked.</returns>
        Task<int> RevokeExpiredSessionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a session is valid (exists, is active, and not expired).
        /// </summary>
        /// <param name="sessionId">The ID of the session to check.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the session is valid; otherwise, false.</returns>
        Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Refreshes a session by updating its last activity timestamp and optionally extending its expiration.
        /// </summary>
        /// <param name="sessionId">The ID of the session to refresh.</param>
        /// <param name="extendExpiration">Whether to extend the session's expiration time.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the session was successfully refreshed.</returns>
        Task<bool> RefreshSessionAsync(Guid sessionId, bool extendExpiration = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of active sessions for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of active sessions.</returns>
        Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of active sessions for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a paginated list of active user sessions.</returns>
        Task<PaginatedList<UserSession>> GetPaginatedActiveSessionsAsync(
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}
