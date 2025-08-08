using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public class CategoryRepository : NamedEntityRepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) 
        : base(context) 
    { 
    }

    public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
        => !await _dbSet.AnyAsync(c => c.Name == name, cancellationToken);

    public async Task<IEnumerable<Category>> GetMainCategoriesAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(c => c.IsMainCategory)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentCategoryId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(c => c.ParentCategoryId == parentCategoryId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
}
