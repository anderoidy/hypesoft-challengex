using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
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
    public class UserRepository : IUserRepository, IIdentityRepository<ApplicationUser>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMongoCollection<ApplicationUser> _usersCollection;
        private readonly IMongoCollection<ApplicationRole> _rolesCollection;
        private readonly IMongoCollection<IdentityUserRole<Guid>> _userRolesCollection;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClientSessionHandle _session;

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ApplicationDbContext context,
            IUnitOfWork unitOfWork
        )
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _usersCollection = context.Users;
            _rolesCollection = context.Roles;
            _userRolesCollection = context.Database.GetCollection<IdentityUserRole<Guid>>(
                "user_roles"
            );
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _session = context.Session;
        }

        public IUnitOfWork UnitOfWork => _unitOfWork;

        public async Task<ApplicationUser?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _usersCollection
                .Find(u => u.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                await LoadUserRolesAsync(user, cancellationToken);
            }

            return user;
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetAllAsync(
            CancellationToken cancellationToken = default
        )
        {
            var users = await _usersCollection.Find(_ => true).ToListAsync(cancellationToken);

            // Carregar roles em lote para todos os usuários
            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _userRolesCollection
                .Find(ur => userIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);

            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
            var roles = await _rolesCollection
                .Find(r => roleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            var rolesLookup = roles.ToDictionary(r => r.Id, r => r);
            var userRolesLookup = userRoles.ToLookup(ur => ur.UserId);

            foreach (var user in users)
            {
                var userRoleEntries = userRolesLookup[user.Id];
                user.Roles = userRoleEntries
                    .Select(ur => new ApplicationUserRole
                    {
                        UserId = ur.UserId,
                        RoleId = ur.RoleId,
                        Role = rolesLookup.GetValueOrDefault(ur.RoleId),
                    })
                    .ToList();
            }

            return users;
        }

        public async Task<IReadOnlyList<ApplicationUser>> GetListBySpecAsync(
            ISpecification<ApplicationUser> spec,
            CancellationToken cancellationToken = default
        )
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            var query = _usersCollection.AsQueryable();

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

            var users = await query.ToListAsync(cancellationToken);

            // Carregar roles para cada usuário
            foreach (var user in users)
            {
                user.Roles = await GetUserRolesAsync(user.Id, cancellationToken);
            }

            return users;
        }

        private async Task LoadUserRolesBatch(
            IEnumerable<ApplicationUser> users,
            CancellationToken cancellationToken
        )
        {
            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _userRolesCollection
                .Find(ur => userIds.Contains(ur.UserId))
                .ToListAsync(cancellationToken);

            if (!userRoles.Any())
                return;

            var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();
            var roles = await _rolesCollection
                .Find(r => roleIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            var rolesLookup = roles.ToDictionary(r => r.Id, r => r);
            var userRolesLookup = userRoles.ToLookup(ur => ur.UserId);

            foreach (var user in users)
            {
                var userRoleEntries = userRolesLookup[user.Id];
                user.Roles = userRoleEntries
                    .Select(ur => new ApplicationUserRole
                    {
                        UserId = ur.UserId,
                        RoleId = ur.RoleId,
                        Role = rolesLookup.GetValueOrDefault(ur.RoleId),
                    })
                    .ToList();
            }
        }

        public async Task<ApplicationUser> AddAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        )
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.SetUpdatedAt(DateTime.UtcNow);

            var result = await _userManager.CreateAsync(entity);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }

            return entity;
        }

        public async Task UpdateAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        )
        {
            entity.SetUpdatedAt(DateTime.UtcNow);
            var result = await _userManager.UpdateAsync(entity);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                );
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await GetByIdAsync(id, cancellationToken);
            if (user != null)
            {
                await DeleteAsync(user, cancellationToken);
            }
        }

        public async Task DeleteAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        )
        {
            using (var session = await _usersCollection.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    // Remover relacionamentos de usuário com roles
                    await _userRolesCollection.DeleteManyAsync(
                        session,
                        ur => ur.UserId == entity.Id,
                        cancellationToken
                    );

                    // Remover o usuário
                    await _usersCollection.DeleteOneAsync(
                        session,
                        u => u.Id == entity.Id,
                        cancellationToken: cancellationToken
                    );

                    await session.CommitTransactionAsync(cancellationToken);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync(cancellationToken);
                    throw;
                }
            }
        }

        public async Task<ApplicationUser?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _usersCollection
                .Find(u => u.NormalizedEmail == email.ToUpper())
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                await LoadUserRolesAsync(user, cancellationToken);
            }

            return user;
        }

        public async Task<ApplicationUser?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _usersCollection
                .Find(u => u.NormalizedUserName == userName.ToUpper())
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
            {
                await LoadUserRolesAsync(user, cancellationToken);
            }

            return user;
        }

        public async Task<bool> IsEmailUniqueAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            var existingUser = await GetByEmailAsync(email, cancellationToken);
            return existingUser == null;
        }

        private async Task LoadUserRolesAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            if (user == null)
                return;

            var userRoles = await _userRolesCollection
                .Find(ur => ur.UserId == user.Id)
                .ToListAsync(cancellationToken);

            if (userRoles.Any())
            {
                var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
                var roles = await _rolesCollection
                    .Find(r => roleIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

                user.Roles = userRoles
                    .Select(ur => new ApplicationUserRole
                    {
                        UserId = ur.UserId,
                        RoleId = ur.RoleId,
                        Role = roles.FirstOrDefault(r => r.Id == ur.RoleId),
                    })
                    .ToList();
            }
            else
            {
                user.Roles = new List<ApplicationUserRole>();
            }
        }

        public async Task<IdentityResult> CreateAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default
        )
        {
            using (var session = await _usersCollection.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var result = await _userManager.CreateAsync(user, password);

                    if (result.Succeeded)
                    {
                        await session.CommitTransactionAsync(cancellationToken);
                        return result;
                    }

                    await session.AbortTransactionAsync(cancellationToken);
                    return result;
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);
                    return IdentityResult.Failed(new IdentityError { Description = ex.Message });
                }
            }
        }

        public async Task<IdentityResult> AddToRoleAsync(
            ApplicationUser user,
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            using (var session = await _usersCollection.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        return IdentityResult.Failed(
                            new IdentityError { Description = $"Role '{roleName}' not found." }
                        );
                    }

                    var userRole = new IdentityUserRole<Guid>
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                    };

                    await _userRolesCollection.InsertOneAsync(
                        session,
                        userRole,
                        cancellationToken: cancellationToken
                    );

                    await session.CommitTransactionAsync(cancellationToken);

                    // Atualizar cache de roles do usuário
                    if (user.Roles == null)
                        user.Roles = new List<ApplicationUserRole>();

                    user.Roles.Add(
                        new ApplicationUserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            Role = role,
                        }
                    );

                    return IdentityResult.Success;
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);
                    return IdentityResult.Failed(new IdentityError { Description = ex.Message });
                }
            }
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(
            ApplicationUser user,
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            using (var session = await _usersCollection.Database.Client.StartSessionAsync())
            {
                session.StartTransaction();

                try
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        return IdentityResult.Failed(
                            new IdentityError { Description = $"Role '{roleName}' not found." }
                        );
                    }

                    await _userRolesCollection.DeleteOneAsync(
                        session,
                        ur => ur.UserId == user.Id && ur.RoleId == role.Id,
                        cancellationToken: cancellationToken
                    );

                    await session.CommitTransactionAsync(cancellationToken);

                    // Atualizar cache de roles do usuário
                    if (user.Roles != null)
                    {
                        var roleToRemove = user.Roles.FirstOrDefault(r => r.RoleId == role.Id);
                        if (roleToRemove != null)
                        {
                            user.Roles.Remove(roleToRemove);
                        }
                    }

                    return IdentityResult.Success;
                }
                catch (Exception ex)
                {
                    await session.AbortTransactionAsync(cancellationToken);
                    return IdentityResult.Failed(new IdentityError { Description = ex.Message });
                }
            }
        }

        public async Task<IList<string>> GetRolesAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            await LoadUserRolesAsync(user, cancellationToken);
            var roleIds = user.Roles?.Select(ur => ur.RoleId).ToList() ?? new List<Guid>();

            if (!roleIds.Any())
                return new List<string>();

            var roles = await _rolesCollection
                .Find(r => roleIds.Contains(r.Id))
                .Project(r => r.Name)
                .ToListAsync(cancellationToken);

            return roles;
        }

        public async Task<bool> IsInRoleAsync(
            ApplicationUser user,
            string roleName,
            CancellationToken cancellationToken = default
        )
        {
            var roles = await GetRolesAsync(user, cancellationToken);
            return roles.Contains(roleName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            await LoadUserRolesAsync(user, cancellationToken);
            var claims = new List<Claim>();

            // Adicionar claims do usuário
            if (user.Claims != null)
            {
                claims.AddRange(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)));
            }

            // Adicionar claims das roles
            var roleIds = user.Roles?.Select(ur => ur.RoleId).ToList() ?? new List<Guid>();

            if (roleIds.Any())
            {
                var roles = await _rolesCollection
                    .Find(r => roleIds.Contains(r.Id))
                    .ToListAsync(cancellationToken);

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));

                    if (role.Claims != null)
                    {
                        claims.AddRange(
                            role.Claims.Select(rc => new Claim(rc.ClaimType, rc.ClaimValue))
                        );
                    }
                }
            }

            return claims;
        }

        public async Task<IdentityResult> AddClaimsAsync(
            ApplicationUser user,
            IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default
        )
        {
            var userClaims = claims
                .Select(c => new IdentityUserClaim<Guid>
                {
                    UserId = user.Id,
                    ClaimType = c.Type,
                    ClaimValue = c.Value,
                })
                .ToList();

            var update = Builders<ApplicationUser>.Update.PushEach(u => u.Claims, userClaims);
            var result = await _usersCollection.UpdateOneAsync(
                u => u.Id == user.Id,
                update,
                cancellationToken: cancellationToken
            );

            if (result.ModifiedCount > 0)
            {
                // Atualizar cache de claims do usuário
                if (user.Claims == null)
                    user.Claims = new List<IdentityUserClaim<Guid>>();

                user.Claims.AddRange(userClaims);
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(
                new IdentityError { Description = "Failed to add claims to user." }
            );
        }

        public async Task<IdentityResult> RemoveClaimsAsync(
            ApplicationUser user,
            IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default
        )
        {
            var claimValues = claims.Select(c => c.Type).ToHashSet();

            var filter = Builders<ApplicationUser>.Filter.And(
                Builders<ApplicationUser>.Filter.Eq(u => u.Id, user.Id),
                Builders<ApplicationUser>.Filter.ElemMatch(
                    u => u.Claims,
                    c => claimValues.Contains(c.ClaimType)
                )
            );

            var update = Builders<ApplicationUser>.Update.PullFilter(
                u => u.Claims,
                c => claimValues.Contains(c.ClaimType)
            );
            var result = await _usersCollection.UpdateOneAsync(
                filter,
                update,
                cancellationToken: cancellationToken
            );

            if (result.ModifiedCount > 0 && user.Claims != null)
            {
                // Atualizar cache de claims do usuário
                user.Claims.RemoveAll(c => claimValues.Contains(c.ClaimType));
                return IdentityResult.Success;
            }

            return IdentityResult.Failed(
                new IdentityError { Description = "Failed to remove claims from user." }
            );
        }

        public async Task<bool> CheckPasswordAsync(
            ApplicationUser user,
            string password,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task<bool> HasPasswordAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            return !string.IsNullOrEmpty(user.PasswordHash);
        }

        public async Task<IdentityResult> ChangePasswordAsync(
            ApplicationUser user,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<string> GeneratePasswordResetTokenAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ResetPasswordAsync(
            ApplicationUser user,
            string token,
            string newPassword,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(
            ApplicationUser user,
            string token,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        public async Task<bool> IsEmailConfirmedAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<ApplicationUser?> FindByLoginAsync(
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.FindByLoginAsync(loginProvider, providerKey);
        }

        public async Task<IdentityResult> AddLoginAsync(
            ApplicationUser user,
            UserLoginInfo login,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.AddLoginAsync(user, login);
        }

        public async Task<IdentityResult> RemoveLoginAsync(
            ApplicationUser user,
            string loginProvider,
            string providerKey,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default
        )
        {
            return await _userManager.GetLoginsAsync(user);
        }
    }
}
