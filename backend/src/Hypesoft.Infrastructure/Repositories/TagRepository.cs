using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public class TagRepository : RepositoryBase<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
        => !await _dbSet.AnyAsync(t => t.Name == name, cancellationToken);
}
