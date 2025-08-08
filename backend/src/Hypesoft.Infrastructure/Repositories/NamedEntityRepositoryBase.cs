using System.Linq.Expressions;
using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hypesoft.Infrastructure.Repositories;

public abstract class NamedEntityRepositoryBase<TEntity> : RepositoryBase<TEntity>, INamedEntityRepository<TEntity>
    where TEntity : EntityBase, INamedEntity
{
    protected NamedEntityRepositoryBase(ApplicationDbContext context) 
        : base(context)
    {
    }

    public virtual async Task<bool> IsNameUniqueAsync(
        string name, 
        Guid? excludeId = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome não pode ser vazio", nameof(name));

        var normalizedName = name.Trim().ToLowerInvariant();
        
        return !await _dbSet.AnyAsync(
            e => e.Id != excludeId && 
                 EF.Functions.Like(EF.Functions.Collate(e.Name, "SQL_Latin1_General_CP1_CI_AI"), normalizedName), 
            cancellationToken);
    }
}
