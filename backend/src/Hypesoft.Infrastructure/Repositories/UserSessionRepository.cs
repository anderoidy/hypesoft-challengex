using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories
{
    /// <summary>
    /// Represents a repository for managing user sessions.
    /// </summary>
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSpan _defaultSessionLifetime = TimeSpan.FromDays(30);
        private readonly TimeSpan _sessionInactivityTimeout = TimeSpan.FromDays(7);

        /// <summary>
        /// Initializes a new instance of the <see cref="UserSessionRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public UserSessionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region IRepository<UserSession> Implementation

        public async Task<UserSession> AddAsync(UserSession entity, CancellationToken cancellationToken = default)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.LastActivityAt = DateTime.UtcNow;
            
            if (entity.ExpiresAt == default)
            {
                entity.ExpiresAt = DateTime.UtcNow.Add(_defaultSessionLifetime);
            }

            await _context.UserSessions.AddAsync(entity, cancellationToken);
            return entity;
        }

        public async Task<IEnumerable<UserSession>> AddRangeAsync(IEnumerable<UserSession> entities, CancellationToken cancellationToken = default)
        {
            var sessions = entities.ToList();
            var now = DateTime.UtcNow;
            
            foreach (var session in sessions)
            {
                session.CreatedAt = now;
                session.UpdatedAt = now;
                session.LastActivityAt = now;
                
                if (session.ExpiresAt == default)
                {
                    session.ExpiresAt = now.Add(_defaultSessionLifetime);
                }
            }

            await _context.UserSessions.AddRangeAsync(sessions, cancellationToken);
            return sessions;
        }

        public async Task<bool> AnyAsync(ISpecification<UserSession> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions.AnyAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<UserSession> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions.CountAsync(cancellationToken);
        }

        public void Delete(UserSession entity)
        {
            // Soft delete by marking as revoked
            entity.IsRevoked = true;
            entity.RevokedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void DeleteRange(IEnumerable<UserSession> entities)
        {
            var now = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                entity.IsRevoked = true;
                entity.RevokedAt = now;
                entity.UpdatedAt = now;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public async Task<UserSession?> FirstOrDefaultAsync(ISpecification<UserSession> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<UserSession, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<UserSession>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserSessions
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<UserSession>> ListAsync(ISpecification<UserSession> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<UserSession, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public void Update(UserSession entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<UserSession> entities)
        {
            var now = DateTime.UtcNow;
            foreach (var entity in entities)
            {
                entity.UpdatedAt = now;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        #endregion

        #region IUserSessionRepository Implementation

        public async Task<UserSession?> GetActiveSessionByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.UserSessions
                .FirstOrDefaultAsync(s => 
                    s.Id == id && 
                    !s.IsRevoked && 
                    s.ExpiresAt > now && 
                    s.LastActivityAt > now.Subtract(_sessionInactivityTimeout), 
                    cancellationToken);
        }

        public async Task<UserSession?> GetActiveSessionByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.UserSessions
                .FirstOrDefaultAsync(s => 
                    s.RefreshToken == refreshToken && 
                    !s.IsRevoked && 
                    s.ExpiresAt > now && 
                    s.LastActivityAt > now.Subtract(_sessionInactivityTimeout), 
                    cancellationToken);
        }

        public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.UserSessions
                .Where(s => 
                    s.UserId == userId && 
                    !s.IsRevoked && 
                    s.ExpiresAt > now && 
                    s.LastActivityAt > now.Subtract(_sessionInactivityTimeout))
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> RevokeSessionAsync(Guid sessionId, Guid? revokedBy = null, CancellationToken cancellationToken = default)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsRevoked, cancellationToken);

            if (session == null)
                return false;

            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedBy = revokedBy;
            session.UpdatedAt = DateTime.UtcNow;

            _context.Entry(session).State = EntityState.Modified;
            return true;
        }

        public async Task<int> RevokeAllSessionsForUserAsync(Guid userId, Guid? exceptSessionId = null, Guid? revokedBy = null, CancellationToken cancellationToken = default)
        {
            var query = _context.UserSessions
                .Where(s => s.UserId == userId && !s.IsRevoked);

            if (exceptSessionId.HasValue)
            {
                query = query.Where(s => s.Id != exceptSessionId.Value);
            }

            var sessions = await query.ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;

            foreach (var session in sessions)
            {
                session.IsRevoked = true;
                session.RevokedAt = now;
                session.RevokedBy = revokedBy;
                session.UpdatedAt = now;
                _context.Entry(session).State = EntityState.Modified;
            }

            return sessions.Count;
        }

        public async Task<int> RevokeExpiredSessionsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var expiredSessions = await _context.UserSessions
                .Where(s => !s.IsRevoked && (s.ExpiresAt <= now || s.LastActivityAt <= now.Subtract(_sessionInactivityTimeout)))
                .ToListAsync(cancellationToken);

            foreach (var session in expiredSessions)
            {
                session.IsRevoked = true;
                session.RevokedAt = now;
                session.UpdatedAt = now;
                _context.Entry(session).State = EntityState.Modified;
            }

            return expiredSessions.Count;
        }

        public async Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.UserSessions
                .AnyAsync(s => 
                    s.Id == sessionId && 
                    !s.IsRevoked && 
                    s.ExpiresAt > now && 
                    s.LastActivityAt > now.Subtract(_sessionInactivityTimeout), 
                    cancellationToken);
        }

        public async Task<bool> RefreshSessionAsync(Guid sessionId, bool extendExpiration = true, CancellationToken cancellationToken = default)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsRevoked, cancellationToken);

            if (session == null)
                return false;

            var now = DateTime.UtcNow;
            session.LastActivityAt = now;
            
            if (extendExpiration)
            {
                session.ExpiresAt = now.Add(_defaultSessionLifetime);
            }
            
            session.UpdatedAt = now;
            _context.Entry(session).State = EntityState.Modified;
            
            return true;
        }

        public async Task<int> GetActiveSessionCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.UserSessions
                .CountAsync(s => 
                    s.UserId == userId && 
                    !s.IsRevoked && 
                    s.ExpiresAt > now && 
                    s.LastActivityAt > now.Subtract(_sessionInactivityTimeout), 
                    cancellationToken);
        }

        public async Task<PaginatedList<UserSession>> GetPaginatedActiveSessionsAsync(
            Guid userId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = _context.UserSessions
                .Where(s => 
                    s.UserId == userId && 
                    !s.IsRevoked && 
                    s.ExpiresAt > now && 
                    s.LastActivityAt > now.Subtract(_sessionInactivityTimeout))
                .OrderByDescending(s => s.LastActivityAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedList<UserSession>(items, totalCount, pageNumber, pageSize);
        }

        #endregion

        #region Private Methods

        private IQueryable<UserSession> ApplySpecification(ISpecification<UserSession> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.UserSessions.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<UserSession, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_context.UserSessions.AsQueryable(), specification);
        }

        #endregion
    }
}
