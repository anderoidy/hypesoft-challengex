using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        );
        Task<ApplicationUser?> GetByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        );
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);

        // Métodos básicos de repositório
        Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ApplicationUser> AddAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        );
        Task<ApplicationUser> UpdateAsync(
            ApplicationUser entity,
            CancellationToken cancellationToken = default
        );
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
