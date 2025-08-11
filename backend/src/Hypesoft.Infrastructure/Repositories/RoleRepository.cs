using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories
{
    /// <summary>
    /// Represents a repository for managing roles.
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="roleManager">The role manager.</param>
        public RoleRepository(
            ApplicationDbContext context,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        #region IRepository<ApplicationRole> Implementation

        public async Task<ApplicationRole> AddAsync(ApplicationRole entity, CancellationToken cancellationToken = default)
        {
            var result = await _roleManager.CreateAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return entity;
        }

        public async Task<IEnumerable<ApplicationRole>> AddRangeAsync(IEnumerable<ApplicationRole> entities, CancellationToken cancellationToken = default)
        {
            var roles = entities.ToList();
            foreach (var role in roles)
            {
                await AddAsync(role, cancellationToken);
            }
            return roles;
        }

        public async Task<bool> AnyAsync(ISpecification<ApplicationRole> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles.AnyAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<ApplicationRole> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles.CountAsync(cancellationToken);
        }

        public void Delete(ApplicationRole entity)
        {
            _context.Roles.Remove(entity);
        }

        public void DeleteRange(IEnumerable<ApplicationRole> entities)
        {
            _context.Roles.RemoveRange(entities);
        }

        public async Task<ApplicationRole?> FirstOrDefaultAsync(ISpecification<ApplicationRole> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<ApplicationRole, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ApplicationRole?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .Include(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRole>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .Include(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationRole>> ListAsync(ISpecification<ApplicationRole> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .Include(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<ApplicationRole, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken);
        }

        public void Update(ApplicationRole entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<ApplicationRole> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        #endregion

        #region IRoleRepository Implementation

        public async Task<ApplicationRole?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .Include(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.NormalizedName == _roleManager.NormalizeKey(name), cancellationToken);
        }

        public async Task<ApplicationRole?> GetByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .Include(r => r.UserRoles)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        }

        public async Task<bool> ExistsWithNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _roleManager.Roles
                .AnyAsync(r => r.NormalizedName == _roleManager.NormalizeKey(name), cancellationToken);
        }

        public async Task<int> GetUsersInRoleCountAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var role = await GetByNameAsync(roleName, cancellationToken);
            if (role == null)
                return 0;

            return await _context.UserRoles
                .CountAsync(ur => ur.RoleId == role.Id, cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetUsersInRoleAsync(
            string roleName,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var role = await GetByNameAsync(roleName, cancellationToken);
            if (role == null)
                return new List<ApplicationUser>();

            return await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == role.Id))
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<IdentityRoleClaim<Guid>>> GetClaimsAsync(ApplicationRole role, CancellationToken cancellationToken = default)
        {
            return await _context.RoleClaims
                .Where(rc => rc.RoleId == role.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<IdentityResult> AddClaimAsync(
            ApplicationRole role,
            IdentityRoleClaim<Guid> claim,
            CancellationToken cancellationToken = default)
        {
            return await _roleManager.AddClaimAsync(role, claim.ToClaim());
        }

        public async Task<IdentityResult> RemoveClaimAsync(
            ApplicationRole role,
            IdentityRoleClaim<Guid> claim,
            CancellationToken cancellationToken = default)
        {
            var claims = await _roleManager.GetClaimsAsync(role);
            var claimToRemove = claims.FirstOrDefault(c => c.Type == claim.ClaimType && c.Value == claim.ClaimValue);
            
            if (claimToRemove != null)
            {
                return await _roleManager.RemoveClaimAsync(role, claimToRemove);
            }

            return IdentityResult.Success;
        }

        public async Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken = default)
        {
            return await _roleManager.GetNormalizedRoleNameAsync(role);
        }

        public async Task<IdentityResult> SetNormalizedRoleNameAsync(
            ApplicationRole role,
            string normalizedName,
            CancellationToken cancellationToken = default)
        {
            return await _roleManager.SetNormalizedRoleNameAsync(role, normalizedName);
        }

        #endregion

        #region Private Methods

        private IQueryable<ApplicationRole> ApplySpecification(ISpecification<ApplicationRole> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_roleManager.Roles.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<ApplicationRole, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_roleManager.Roles.AsQueryable(), specification);
        }

        #endregion
    }
}
