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
    /// Represents a repository for managing users.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="roleManager">The role manager.</param>
        public UserRepository(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        #region IRepository<ApplicationUser> Implementation

        public async Task<ApplicationUser> AddAsync(ApplicationUser entity, CancellationToken cancellationToken = default)
        {
            var result = await _userManager.CreateAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return entity;
        }

        public async Task<IEnumerable<ApplicationUser>> AddRangeAsync(IEnumerable<ApplicationUser> entities, CancellationToken cancellationToken = default)
        {
            var users = entities.ToList();
            foreach (var user in users)
            {
                await AddAsync(user, cancellationToken);
            }
            return users;
        }

        public async Task<bool> AnyAsync(ISpecification<ApplicationUser> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).AnyAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await _userManager.Users.AnyAsync(cancellationToken);
        }

        public async Task<int> CountAsync(ISpecification<ApplicationUser> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _userManager.Users.CountAsync(cancellationToken);
        }

        public void Delete(ApplicationUser entity)
        {
            _context.Users.Remove(entity);
        }

        public void DeleteRange(IEnumerable<ApplicationUser> entities)
        {
            _context.Users.RemoveRange(entities);
        }

        public async Task<ApplicationUser?> FirstOrDefaultAsync(ISpecification<ApplicationUser> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<ApplicationUser, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationUser>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ApplicationUser>> ListAsync(ISpecification<ApplicationUser> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(ISpecification<ApplicationUser, TResult> specification, CancellationToken cancellationToken = default)
        {
            return await ApplySpecification(specification)
                .OrderBy(u => u.UserName)
                .ToListAsync(cancellationToken);
        }

        public void Update(ApplicationUser entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<ApplicationUser> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        #endregion

        #region IUserRepository Implementation

        public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == _userManager.NormalizeEmail(email), cancellationToken);
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == _userManager.NormalizeName(userName), cancellationToken);
        }

        public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .AnyAsync(u => u.NormalizedEmail == _userManager.NormalizeEmail(email), cancellationToken);
        }

        public async Task<bool> ExistsWithUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .AnyAsync(u => u.NormalizedUserName == _userManager.NormalizeName(userName), cancellationToken);
        }

        public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
        {
            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
        {
            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken = default)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<int> GetUsersInRoleCountAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
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
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return new List<ApplicationUser>();

            return await _userManager.Users
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == role.Id))
                .OrderBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IdentityResult> ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ResetPasswordAsync(
            ApplicationUser user,
            string token,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> SetLockoutEndDateAsync(
            ApplicationUser user,
            DateTimeOffset? lockoutEnd,
            CancellationToken cancellationToken = default)
        {
            return await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        }

        public async Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var result = await _userManager.AccessFailedAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to increment access failed count: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            return await _userManager.GetAccessFailedCountAsync(user);
        }

        public async Task<IdentityResult> ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.ResetAccessFailedCountAsync(user);
        }

        public async Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            return await _userManager.GetAccessFailedCountAsync(user);
        }

        #endregion

        #region Private Methods

        private IQueryable<ApplicationUser> ApplySpecification(ISpecification<ApplicationUser> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_userManager.Users.AsQueryable(), specification);
        }

        private IQueryable<TResult> ApplySpecification<TResult>(ISpecification<ApplicationUser, TResult> specification)
        {
            return SpecificationEvaluator.Default.GetQuery(_userManager.Users.AsQueryable(), specification);
        }

        #endregion
    }
}
