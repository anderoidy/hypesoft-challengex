using System;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    /// <summary>
    /// UserRepository simplificado compatível com Entity Framework Core + MongoDB Provider
    /// </summary>
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
            : base(context, logger)
        {
            _context = context;
        }

        // ✅ MÉTODOS ESPECÍFICOS DE IUserRepository:

        public async Task<ApplicationUser?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Users.FirstOrDefaultAsync(
                u => u.Email == email,
                cancellationToken
            );
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Users.FirstOrDefaultAsync(
                u => u.UserName == userName,
                cancellationToken
            );
        }

        public async Task<bool> IsEmailUniqueAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }

        // ✅ MÉTODOS BÁSICOS DE REPOSITÓRIO:

        public new async Task<ApplicationUser?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        }

        public new async Task<ApplicationUser> AddAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        )
        {
            // Auditoria correta (CreatedAt, UpdatedAt, LastModifiedBy)
            entity.SetCreated(
                string.IsNullOrWhiteSpace(entity.CreatedBy) ? "system" : entity.CreatedBy
            );

            await _context.Users.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public new async Task<ApplicationUser> UpdateAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        )
        {
            entity.SetLastModifiedBy(entity.LastModifiedBy ?? "system");
            _context.Users.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
