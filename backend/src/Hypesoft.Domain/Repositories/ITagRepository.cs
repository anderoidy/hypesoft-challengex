using Hypesoft.Domain.Common.Interfaces;
using Hypesoft.Domain.Entities;

namespace Hypesoft.Domain.Repositories;

public interface ITagRepository : INamedEntityRepository<Tag>
{
    // Métodos específicos de Tag podem ser adicionados aqui
    Task<IEnumerable<Tag>> GetActiveTagsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Tag>> GetTagsByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
