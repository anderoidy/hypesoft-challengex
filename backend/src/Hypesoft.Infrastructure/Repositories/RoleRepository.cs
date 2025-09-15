using System.Threading;
using System.Threading.Tasks;
using Ardalis.Specification.EntityFrameworkCore;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Specifications;
using Hypesoft.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Infrastructure.Repositories
{
    public class RoleRepository : BaseRepository<ApplicationRole>
    {
        public RoleRepository(ApplicationDbContext context, ILogger<RoleRepository> roleLogger)
            : base(context, roleLogger) { }

        public async Task<ApplicationRole?> GetByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            var spec = new GetRoleByNameSpec(name);
            return await FirstOrDefaultAsync(spec, cancellationToken);
        }
    }
}
