using Hypesoft.Domain.Common;
using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Interfaces;
using Hypesoft.Domain.Repositories;

namespace Hypesoft.Domain.Repositories;

public interface INamedEntityRepository<TEntity>
    where TEntity : BaseEntity, INamedEntity
{
    Task<bool> IsNameUniqueAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default
    );
}
