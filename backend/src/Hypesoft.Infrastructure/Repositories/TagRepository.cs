using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public class TagRepository : NamedEntityRepositoryBase<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext context) 
        : base(context) 
    { 
    }

    public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
        => !await _dbSet.AnyAsync(t => t.Name == name, cancellationToken);

    public async Task<IEnumerable<Tag>> GetActiveTagsAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Tag>> GetTagsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(t => t.ProductTags.Any(pt => pt.ProductId == productId))
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
}
