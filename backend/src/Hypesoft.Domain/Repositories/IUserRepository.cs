using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Interfaces;

namespace Hypesoft.Domain.Repositories
{
    public interface IUserRepository : IIdentityRepository<ApplicationUser>
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
    }
}
