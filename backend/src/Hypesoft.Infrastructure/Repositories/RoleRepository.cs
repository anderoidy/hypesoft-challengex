using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Hypesoft.Infrastructure.Repositories
{
    /// <summary>
    /// Represents a repository for managing roles.
    /// </summary>
    public class RoleRepository : IRoleRepository, IIdentityRepository<ApplicationRole>
    {
        private readonly IMongoCollection<ApplicationRole> _rolesCollection;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleRepository(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager
        )
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _rolesCollection = context.GetCollection<ApplicationRole>("Roles");
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<ApplicationRole?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            return await _rolesCollection
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRole>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _rolesCollection
                .Find(FilterDefinition<ApplicationRole>.Empty)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRole>> GetListBySpecAsync(
            ISpecification<ApplicationRole> spec,
            CancellationToken cancellationToken = default
        )
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            var query = _rolesCollection.AsQueryable();

            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

            if (spec.OrderBy != null)
            {
                query = spec.OrderBy(query);
            }

            if (spec.Skip.HasValue)
            {
                query = query.Skip(spec.Skip.Value);
            }

            if (spec.Take.HasValue)
            {
                query = query.Take(spec.Take.Value);
            }

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<ApplicationRole> AddAsync(
            ApplicationRole entity,
            CancellationToken cancellationToken = default
        )
        {
            entity.SetUpdatedAt(DateTime.UtcNow);

            var result = await _roleManager.CreateAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            return entity;
        }

        public async Task UpdateAsync(
            ApplicationRole entity,
            CancellationToken cancellationToken = default
        )
        {
            entity.SetUpdatedAt(DateTime.UtcNow);
            var result = await _roleManager.UpdateAsync(entity);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to update role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }
        }

        public async Task DeleteAsync(
            ApplicationRole entity,
            CancellationToken cancellationToken = default
        )
        {
            var result = await _roleManager.DeleteAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to delete role: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var role = await GetByIdAsync(id, cancellationToken);
            if (role != null)
            {
                await DeleteAsync(role, cancellationToken);
            }
        }

        public async Task<ApplicationRole?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            return await _rolesCollection
                .Find(r => r.NormalizedName == name.ToUpper())
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            return await _rolesCollection
                .Find(r => r.NormalizedName == roleName.ToUpper())
                .AnyAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRole>> GetRolesForUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var userRoles = await _context
                .UserRoles.Find(ur => ur.UserId == userId)
                .ToListAsync(cancellationToken);

            var roleIds = userRoles.Select(ur => ur.RoleId).ToList();

            if (!roleIds.Any())
                return new List<ApplicationRole>();

            return await _rolesCollection
                .Find(r => roleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsInRoleAsync(
            Guid userId,
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            var role = await GetByNameAsync(roleName, cancellationToken);
            if (role == null)
                return false;

            var userRole = await _context
                .UserRoles.Find(ur => ur.UserId == userId && ur.RoleId == role.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return userRole != null;
        }

        public async Task<bool> RoleExistsAsync(
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }
    }
}
